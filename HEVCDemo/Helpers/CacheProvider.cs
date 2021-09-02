using HEVCDemo.Parsers;
using Rasyidf.Localization;
using HEVCDemo.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Helpers
{
    public class CacheProvider
    {
        public readonly string LoadedFilePath;

        private const int cacheSize = 30;
        private const string cachePrefix = "cache";

        private const string textExtension = ".txt";
        private const string annexBExtension = ".bin";
        private const string yuvExtension = ".yuv";

        private readonly string cacheDirPath;

        public string StatsDirPath;
        public string CupuFilePath;
        public string PropsFilePath;
        public string PredictionFilePath;
        public string IntraFilePath;
        public string MotionVectorsFilePath;
        public string AnnexBFilePath;
        public string YuvFilePath;
        public string YuvFramesDirPath;

        public bool CacheExists => File.Exists(PropsFilePath);

        public readonly Dictionary<int, BitmapImage> YuvFramesBitmaps = new Dictionary<int, BitmapImage>();

        public double FileSize;
        public VideoSequence videoSequence = new VideoSequence();

        public CacheProvider(string filePath)
        {
            LoadedFilePath = filePath;
            cacheDirPath = $@".\{cachePrefix}\{Path.GetFileNameWithoutExtension(filePath)}";

            // Stats
            StatsDirPath = $@"{cacheDirPath}\stats";
            CupuFilePath = $@"{StatsDirPath}\cupu{textExtension}";
            PropsFilePath = $@"{StatsDirPath}\props{textExtension}";
            PredictionFilePath = $@"{StatsDirPath}\prediction{textExtension}";
            IntraFilePath = $@"{StatsDirPath}\intra{textExtension}";
            MotionVectorsFilePath = $@"{StatsDirPath}\motionVectors{textExtension}";

            // Images
            YuvFramesDirPath = $@"{cacheDirPath}\yuvFrames";

            // AnnexB file stays at his location
            AnnexBFilePath = Path.GetExtension(filePath).ToLower() == annexBExtension ? filePath : $@"{cacheDirPath}\annexB{annexBExtension}";
            YuvFilePath = $@"{cacheDirPath}\yuvFile{yuvExtension}";
        }

        public async Task CreateCache(Action<string, bool> setAppState)
        {
            // Clear at first
            if (Directory.Exists(cacheDirPath))
            {
                Directory.Delete(cacheDirPath, true);
            }

            InitCacheFolders();

            // Check if already annexB format and convert if not
            if (Path.GetExtension(LoadedFilePath).ToLower() != annexBExtension)
            {
                setAppState("ConvertingAnnexBState,Text".Localize(), false);
                await FFmpegHelper.ConvertToAnnexB(this);
            }

            // Get stats data from annexB file
            setAppState("ProcessingAnnexBState,Text".Localize(), false);
            await ProcessHelper.RunProcessAsync($@".\TAppDecoder.exe", $@"-b {AnnexBFilePath} -o {YuvFilePath} -p {StatsDirPath}");

            // Parse properties
            ParseProps();

            // Extract frames 
            setAppState("CreatingDemoState,Text".Localize(), false);
            var framesLoading = FFmpegHelper.ExtractFrames(this);

            await ProcessFiles();
            await framesLoading;
            InitFramesCount();
        }

        public async Task ProcessFiles()
        {
            var cupuParser = new CupuParser(videoSequence);
            await cupuParser.ParseFile(this);

            var predictionParser = new PredictionParser(videoSequence);
            await predictionParser.ParseFile(this);

            var intraParser = new IntraParser(videoSequence);
            var motionVectorsParser = new MotionVectorsParser(videoSequence);

            var tasks = new List<Task>
            {
                intraParser.ParseFile(this),
                motionVectorsParser.ParseFile(this)
            };

            await Task.WhenAll(tasks);
        }

        public void ParseProps()
        {
            var propsParser = new PropsParser();
            propsParser.ParseProps(this, this.videoSequence);
        }

        private void InitCacheFolders()
        {
            Directory.CreateDirectory(YuvFramesDirPath);
            Directory.CreateDirectory(StatsDirPath);
        }

        public async Task LoadIntoCache(int index)
        {
            int startIndex = (index / cacheSize) * cacheSize;
            await LoadFramesIntoCache(startIndex);
        }

        public async Task EnsureFrameInCache(int index, Action<string, string> handleError)
        {
            if (!YuvFramesBitmaps.ContainsKey(index))
            {
                await ActionsHelper.InvokeSafelyAsync(async () =>
                {
                    await LoadIntoCache(index);
                }, "LoadIntoCacheTitle,Title".Localize(), handleError);
            }
        }

        public void InitFramesCount()
        {
            videoSequence.FramesCount = new DirectoryInfo(YuvFramesDirPath).GetFiles().Length;
        }

        public async Task LoadFramesIntoCache(int startIndex)
        {
            var files = new DirectoryInfo(YuvFramesDirPath).GetFiles().ToList();
            files.OrderBy(file => int.Parse(Path.GetFileNameWithoutExtension(file.FullName)));
            await LoadBitmaps(YuvFramesBitmaps, files, startIndex);
        }

        private async Task LoadBitmaps(Dictionary<int, BitmapImage> dictionary, List<FileInfo> files, int startIndex)
        {
            await Task.Run(() => 
            {
                lock (dictionary)
                {
                    dictionary.Clear();
                    GC.Collect();

                    for (int i = startIndex; i < Math.Min(startIndex + cacheSize, videoSequence.FramesCount); i++)
                    {
                        var bitmap = new BitmapImage();

                        using (var imageStreamSource = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bitmap.BeginInit();
                            bitmap.StreamSource = imageStreamSource;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }

                        bitmap.Freeze();
                        dictionary.Add(i, bitmap);
                    }
                }
            });
        }

        public async Task<BitmapImage> GetYuvFrame(int index, Action<string, string> handleError)
        {
            await EnsureFrameInCache(index, handleError);
            return YuvFramesBitmaps[index];
        }

        public WriteableBitmap GetIntraFrame(int index)
        {
            var frame = videoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(videoSequence.Width, videoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                IntraParser.WriteBitmaps(cu, writeableBitmap);
            }

            return writeableBitmap;
        }

        public WriteableBitmap GetPredictionFrame(int index)
        {
            var frame = videoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(videoSequence.Width, videoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                PredictionParser.WriteBitmaps(cu, writeableBitmap);
            }

            return writeableBitmap;
        }

        public WriteableBitmap GetCuPuFrame(int index)
        {
            var frame = videoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(videoSequence.Width, videoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                CupuParser.WriteBitmaps(cu, writeableBitmap);
            }

            return writeableBitmap;
        }

        public WriteableBitmap GetMotionVectorsFrame(int index, bool isStartEnabled)
        {
            var frame = videoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(videoSequence.Width, videoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                MotionVectorsParser.WriteBitmaps(cu, writeableBitmap, isStartEnabled);
            }

            return writeableBitmap;
        }
    }
}
