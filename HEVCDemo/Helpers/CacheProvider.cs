using HEVCDemo.Parsers;
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
        public enum CacheItemType
        {
            Cupu
        }

        public readonly string LoadedFilePath;

        private const int cacheSize = 30;
        private const string cachePrefix = "cache";

        private const string imageExtension = ".png";
        private const string textExtension = ".txt";
        private const string annexBExtension = ".bin";
        private const string yuvExtension = "yuv";

        private readonly string cacheDirPath;

        public readonly Dictionary<int, BitmapImage> DecodedFramesBitmaps = new Dictionary<int, BitmapImage>();
        public readonly Dictionary<int, BitmapImage> CupuImagesBitmaps = new Dictionary<int, BitmapImage>();

        public string StatsDirPath;
        public string CupuFilePath;
        public string PropsFilePath;
        public string AnnexBFilePath;
        public string YuvFilePath;
        public string DecodedFramesDirPath;
        public string CupuImagesDirPath;

        public bool CacheExists => File.Exists(PropsFilePath);
        public Func<string, string> FramesOutputFileNameBuilder => (number) => $@"{DecodedFramesDirPath}\{number}{imageExtension}";

        public int Height { get; private set; }
        public int Width { get; private set; }
        public int MaxCUHeight { get; private set; }

        private int offset;
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
            DecodedFramesDirPath = $@"{cacheDirPath}\decodedFrames";
            CupuImagesDirPath = $@"{cacheDirPath}\cupuImages";

            // AnnexB file stays at his location
            AnnexBFilePath = Path.GetExtension(filePath).ToLower() == annexBExtension ? filePath : $@"{cacheDirPath}\annexB{annexBExtension}";
            YuvFilePath = $@"{cacheDirPath}\yuvFile{yuvExtension}";
        }

        public BitmapImage GetDecodedFrameBitmap(int index)
            => DecodedFramesBitmaps[index - offset];

        public BitmapImage GetCupuImageBitmap(int index)
            => CupuImagesBitmaps[index - offset];

        public async Task CreateCache(Action<string> setAppState)
        {
            // Clear at first
            if (Directory.Exists(cacheDirPath)) Directory.Delete(cacheDirPath, true);

            InitCacheFolders();

            // Check if already annexB format and convert if not
            if (Path.GetExtension(LoadedFilePath).ToLower() != annexBExtension)
            {
                setAppState("Converting to AnnexB format");
                await FFmpegHelper.ConvertToAnnexB(this);
            }

            // Get stats data from annexB file
            setAppState("Processing AnnexB file");
            await ProcessHelper.RunProcessAsync($@".\TAppDecoder.exe", $@"-b {AnnexBFilePath} -o {YuvFilePath} -p {StatsDirPath}");

            // Extract frames 
            setAppState("Creating demo data");
            var framesLoading = FFmpegHelper.ExtractFrames(this);

            // Parse properties
            ParseProps();

            var cupuParser = new CupuParser(Width, Height, MaxCUHeight);
            await cupuParser.ParseFile(this);
            await framesLoading;
            FramesCount = GetFramesCount();
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
            Directory.CreateDirectory(DecodedFramesDirPath);
            Directory.CreateDirectory(CupuImagesDirPath);
            Directory.CreateDirectory(StatsDirPath);
        }

        public async Task LoadIntoCache(int startIndex, Action<string> setAppState, Action<string, string> handleError)
        {
            await ActionsHelper.InvokeSafelyAsync(async () =>
            {
                setAppState("Loading into cache");

                offset = (startIndex / cacheSize) * cacheSize;

                int canLoad = Math.Min(FramesCount - offset, 30);

                await LoadFramesIntoCache(canLoad);
                await LoadCupusIntoCache(canLoad);

                setAppState("Ready");
            }, "Load into cache", handleError);
        }

        public async void EnsureFrameInCache(int index, Action<string> setAppState, Action<string, string> handleError)
        {
            if (!DecodedFramesBitmaps.ContainsKey(index - offset))
            {
                await LoadIntoCache(index, setAppState, handleError);
            }
        }

        public int GetFramesCount()
        {
            return new DirectoryInfo(DecodedFramesDirPath).GetFiles().Length;
        }

        public async Task LoadFramesIntoCache(int count)
        {
            var files = new DirectoryInfo(DecodedFramesDirPath).GetFiles().ToList();
            await LoadBitmaps(DecodedFramesBitmaps, files, count);
        }

        public async Task LoadCupusIntoCache(int count)
        {
            var files = new DirectoryInfo(CupuImagesDirPath).GetFiles().ToList();
            await LoadBitmaps(CupuImagesBitmaps, files, count);
        }

        private async Task LoadBitmaps(Dictionary<int, BitmapImage> dictionary, List<FileInfo> files, int count)
        {
            await Task.Run(() =>
            {
                dictionary.Clear();
                for (int i = offset; i < offset + count; i++)
                {
                    var imageStreamSource = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = imageStreamSource;
                    bitmap.EndInit();
                    dictionary.Add(i - offset, bitmap);
                }
            });
        }

        public void SaveBitmap(BitmapSource bitmap, int number, CacheItemType type)
        {
            string name = number.ToString().PadLeft(3, '0');

            using (FileStream fs = File.Create($@"{cacheDirPath}\{type}\{name}{imageExtension}"))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fs);
            }
        }
    }
}
