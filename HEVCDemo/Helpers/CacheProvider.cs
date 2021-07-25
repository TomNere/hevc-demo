using HEVCDemo.Parsers;
using Rasyidf.Localization;
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

        private const string imageExtension = ".png";
        private const string textExtension = ".txt";
        private const string annexBExtension = ".bin";
        private const string yuvExtension = ".yuv";

        private readonly string cacheDirPath;

        public string StatsDirPath;
        public string CupuFilePath;
        public string PropsFilePath;
        public string AnnexBFilePath;
        public string YuvFilePath;
        public string YuvFramesDirPath;
        public string CupuFramesDirPath;

        public bool CacheExists => File.Exists(PropsFilePath);

        public readonly Dictionary<int, BitmapImage> YuvFramesBitmaps = new Dictionary<int, BitmapImage>();
        public readonly Dictionary<int, BitmapImage> CupuFramesBitmaps = new Dictionary<int, BitmapImage>();

        public double FileSize;
        public int Height { get; private set; }
        public int Width { get; private set; }
        public int MaxCUHeight { get; private set; }

        public int FramesCount;

        public CacheProvider(string filePath)
        {
            LoadedFilePath = filePath;
            cacheDirPath = $@".\{cachePrefix}\{Path.GetFileNameWithoutExtension(filePath)}";

            // Stats
            StatsDirPath = $@"{cacheDirPath}\stats";
            CupuFilePath = $@"{StatsDirPath}\cupu{textExtension}";
            PropsFilePath = $@"{StatsDirPath}\props{textExtension}";

            // Images
            YuvFramesDirPath = $@"{cacheDirPath}\yuvFrames";
            CupuFramesDirPath = $@"{cacheDirPath}\cupuFrames";

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

            var cupuParser = new CupuParser(Width, Height, MaxCUHeight);
            await cupuParser.ParseFile(this);
            await framesLoading;

            if (!InitFramesCount())
            {
                throw new Exception("FramesMismatchEx,Text".Localize());
            }
        }

        public void ParseProps()
        {
            var propsParser = new PropsParser();
            propsParser.ParseProps(this);
            Height = propsParser.SeqHeight;
            Width = propsParser.SeqWidth;
            MaxCUHeight = propsParser.MaxCUHeight;
        }

        private void InitCacheFolders()
        {
            Directory.CreateDirectory(YuvFramesDirPath);
            Directory.CreateDirectory(CupuFramesDirPath);
            Directory.CreateDirectory(StatsDirPath);
        }

        public async Task LoadIntoCache(int index, Action<string, bool> setAppState)
        {
            setAppState("LoadingIntoCacheState,Text".Localize(), false);

            int startIndex = (index / cacheSize) * cacheSize;

            var framesLoading = LoadFramesIntoCache(startIndex);
            await LoadCupusIntoCache(startIndex);
            await framesLoading;

            setAppState("ReadyState,Text".Localize(), true);
        }

        public async Task EnsureFrameInCache(int index, Action<string, bool> setAppState, Action<string, string> handleError)
        {
            if (!YuvFramesBitmaps.ContainsKey(index))
            {
                await ActionsHelper.InvokeSafelyAsync(async () =>
                {
                    await LoadIntoCache(index, setAppState);
                }, "LoadIntoCacheTitle,Title".Localize(), handleError);
            }
        }

        public bool InitFramesCount()
        {
            FramesCount = new DirectoryInfo(YuvFramesDirPath).GetFiles().Length;
            return FramesCount == new DirectoryInfo(CupuFramesDirPath).GetFiles().Length;
        }

        public async Task LoadFramesIntoCache(int startIndex)
        {
            var files = new DirectoryInfo(YuvFramesDirPath).GetFiles().ToList();
            files.OrderBy(file => int.Parse(Path.GetFileNameWithoutExtension(file.FullName)));
            await LoadBitmaps(YuvFramesBitmaps, files, startIndex);
        }

        public async Task LoadCupusIntoCache(int startIndex)
        {
            var files = new DirectoryInfo(CupuFramesDirPath).GetFiles().ToList();
            files.OrderBy(file => int.Parse(Path.GetFileNameWithoutExtension(file.FullName)));
            await LoadBitmaps(CupuFramesBitmaps, files, startIndex);
        }

        private async Task LoadBitmaps(Dictionary<int, BitmapImage> dictionary, List<FileInfo> files, int startIndex)
        {
            await Task.Run(() =>
            {
                dictionary.Clear();
                for (int i = startIndex; i < startIndex + cacheSize; i++)
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
            });
        }

        public void SaveCupuBitmap(BitmapSource bitmap, int number)
        {
            string name = number.ToString().PadLeft(3, '0');

            using (FileStream fs = File.Create($@"{CupuFramesDirPath}\{name}{imageExtension}"))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fs);
            }
        }
    }
}
