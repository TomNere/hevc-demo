using HEVCDemo.Parsers;
using Rasyidf.Localization;
using HEVCDemo.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

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
        
        public readonly Dictionary<int, BitmapImage> YuvFramesBitmaps = new Dictionary<int, BitmapImage>();

        public bool CacheExists => File.Exists(PropsFilePath);
        public string StatsDirPath;
        public string CodingUnitsFilePath;
        public string PropsFilePath;
        public string PredictionTypeFilePath;
        public string IntraPredictionFilePath;
        public string InterPredictionFilePath;
        public string AnnexBFilePath;
        public string YuvFilePath;
        public string YuvFramesDirPath;

        public double FileSize;
        public VideoSequence VideoSequence = new VideoSequence();

        public CacheProvider(string filePath)
        {
            LoadedFilePath = filePath;
            cacheDirPath = $@".\{cachePrefix}\{Path.GetFileNameWithoutExtension(filePath)}";

            // Stats
            StatsDirPath = $@"{cacheDirPath}\stats";
            CodingUnitsFilePath = $@"{StatsDirPath}\cupu{textExtension}";
            PropsFilePath = $@"{StatsDirPath}\props{textExtension}";
            PredictionTypeFilePath = $@"{StatsDirPath}\prediction{textExtension}";
            IntraPredictionFilePath = $@"{StatsDirPath}\intra{textExtension}";
            InterPredictionFilePath = $@"{StatsDirPath}\motionVectors{textExtension}";

            // Frames images
            YuvFramesDirPath = $@"{cacheDirPath}\yuvFrames";

            // AnnexB file stays at his location
            AnnexBFilePath = Path.GetExtension(filePath).ToLower() == annexBExtension ? filePath : $@"{cacheDirPath}\annexB{annexBExtension}";
            YuvFilePath = $@"{cacheDirPath}\yuvFile{yuvExtension}";
        }

        public async Task CreateCache()
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
                GlobalActionsHelper.OnAppStateChanged("ConvertingAnnexBState,Text".Localize(), false);
                await FFmpegHelper.ConvertToAnnexB(this);
            }

            // Get stats data from annexB file
            GlobalActionsHelper.OnAppStateChanged("ProcessingAnnexBState,Text".Localize(), false);
            _ = await ProcessHelper.RunProcessAsync($@".\TAppDecoder.exe", $@"-b {AnnexBFilePath} -o {YuvFilePath} -p {StatsDirPath}");

            // Parse properties
            ParseProps();

            // Extract frames 
            GlobalActionsHelper.OnAppStateChanged("CreatingDemoState,Text".Localize(), false);
            var framesLoading = FFmpegHelper.ExtractFrames(this);

            await ProcessFiles();
            await framesLoading;
            CheckFramesCount();
        }

        public async Task ProcessFiles()
        {
            var cupuParser = new CupuParser(VideoSequence);
            _ = await cupuParser.ParseFile(this);

            var predictionParser = new PredictionParser(VideoSequence);
            _ = await predictionParser.ParseFile(this);

            var intraParser = new IntraParser(VideoSequence);
            var motionVectorsParser = new MotionVectorsParser(VideoSequence);

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
            propsParser.ParseProps(this, VideoSequence);
        }

        private void InitCacheFolders()
        {
            _ = Directory.CreateDirectory(YuvFramesDirPath);
            _ = Directory.CreateDirectory(StatsDirPath);
        }

        public async Task LoadIntoCache(int index)
        {
            int startIndex = (index / cacheSize) * cacheSize;
            await LoadFramesIntoCache(startIndex);
        }

        public void CheckFramesCount()
        {
            int yuvFramesCount = new DirectoryInfo(YuvFramesDirPath).GetFiles().Length;

            if (yuvFramesCount != VideoSequence.FramesCount)
            {
                throw new Exception("ErrorCreatingDemoData,Text".Localize());
            }
        }

        public async Task LoadFramesIntoCache(int startIndex)
        {
            var files = new DirectoryInfo(YuvFramesDirPath).GetFiles().ToList();
            files.OrderBy(file => int.Parse(Path.GetFileNameWithoutExtension(file.FullName)));
            await LoadYuvBitmaps(YuvFramesBitmaps, files, startIndex);
        }

        private async Task LoadYuvBitmaps(Dictionary<int, BitmapImage> dictionary, List<FileInfo> files, int startIndex)
        {
            await Task.Run(() => 
            {
                lock (dictionary)
                {
                    dictionary.Clear();
                    GC.Collect();

                    for (int i = startIndex; i < Math.Min(startIndex + cacheSize, VideoSequence.FramesCount); i++)
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

        public async Task<BitmapImage> GetYuvFrame(int index, string afterStateText)
        {
            if (!YuvFramesBitmaps.ContainsKey(index))
            {
                GlobalActionsHelper.OnAppStateChanged("LoadingIntoCacheState,Text".Localize(), false);
                await ActionsHelper.InvokeSafelyAsync(async () =>
                {
                    await LoadIntoCache(index);
                }, "LoadIntoCacheTitle,Title".Localize(), true);

                GlobalActionsHelper.OnAppStateChanged(afterStateText, true);
            }

            return YuvFramesBitmaps[index];
        }

        public WriteableBitmap GetIntraPredictionFrame(int index)
        {
            var frame = VideoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(VideoSequence.Width, VideoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                IntraParser.WriteBitmaps(cu, writeableBitmap);
            }

            writeableBitmap.Freeze();
            return writeableBitmap;
        }

        public WriteableBitmap GetPredictionTypeFrame(int index)
        {
            var frame = VideoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(VideoSequence.Width, VideoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                PredictionParser.WriteBitmaps(cu, writeableBitmap);
            }

            writeableBitmap.Freeze();
            return writeableBitmap;
        }

        public WriteableBitmap GetCodingUnitsFrame(int index)
        {
            var frame = VideoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(VideoSequence.Width, VideoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                CupuParser.WriteBitmaps(cu, writeableBitmap);
            }

            writeableBitmap.Freeze();
            return writeableBitmap;
        }

        public WriteableBitmap GetInterPredictionFrame(int index, bool isStartEnabled)
        {
            var frame = VideoSequence.GetFrameByPoc(index);

            var writeableBitmap = BitmapFactory.New(VideoSequence.Width, VideoSequence.Height);
            foreach (var cu in frame.CodingUnits)
            {
                MotionVectorsParser.WriteBitmaps(cu, writeableBitmap, isStartEnabled);
            }

            writeableBitmap.Freeze();
            return writeableBitmap;
        }
    }
}
