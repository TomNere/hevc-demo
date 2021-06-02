using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Helpers
{
    public class CacheProvider
    {
        public enum CacheItemType
        {
            Cupu
        }

        private const string cachePrefix = "cache";
        private const string framesPrefix = "frames";
        private const string cupuPrefix = "cupu";

        private const string imageFormat = "png";
        private const string textFileFormat = "txt";
        private const string annexBFormat = "bin";

        private readonly string cachePath;

        public string CupuFilePath;
        public string AnnexBFilePath;

        public bool CacheExists => !string.IsNullOrEmpty(cachePath) && File.Exists(AnnexBFilePath);
        public Func<string, string> FramesOutputFileNameBuilder => (number) => { return $@"{cachePath}\{framesPrefix}\{number}.{imageFormat}"; };

        public CacheProvider(string filePath)
        {
            cachePath = $@".\{cachePrefix}\{Path.GetFileNameWithoutExtension(filePath)}";
            CupuFilePath = $@"{cachePath}\cupu.{textFileFormat}";
            AnnexBFilePath = $@"{cachePath}\annexB.{annexBFormat}";

            InitCacheFolders();
        }

        public void InitCacheFolders()
        {
            Directory.CreateDirectory($@"{cachePath}\{framesPrefix}");
            Directory.CreateDirectory($@"{cachePath}\{cupuPrefix}");
        }

        public List<FileInfo> GetFrames(int end)
        {
            return new DirectoryInfo($@"{cachePath}\{framesPrefix}").GetFiles().Take(end).ToList();
        }

        public int GetFramesCount()
        {
            return new DirectoryInfo($@"{cachePath}\{framesPrefix}").GetFiles().Length;
        }

        public List<BitmapImage> GetFrames(int start, int count)
        {
            var files = new DirectoryInfo($@"{cachePath}\{framesPrefix}").GetFiles().ToList();
            return GetBitmapsInRange(files, start, start + count).ToList();
        }

        public List<BitmapImage> GetCupus(int start, int count)
        {
            var files = new DirectoryInfo($@"{cachePath}\{CacheItemType.Cupu}").GetFiles().ToList();
            return GetBitmapsInRange(files, start, start + count).ToList();
        }

        public void CreateAnnexBCopy(string filePath)
        {
            File.Copy(filePath, AnnexBFilePath);
        }

        private IEnumerable<BitmapImage> GetBitmapsInRange(List<FileInfo> files, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                {
                    var imageStreamSource = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    //yield return Image.FromStream(imageStreamSource);
                    //var decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    //yield return decoder.Frames[0];
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = imageStreamSource;
                    bitmap.EndInit();
                    yield return bitmap;
                }
            }
        }

        public string TryGetFramePath(int index)
        {
            string filePath = $@"{cachePath}\{framesPrefix}\{index}.{imageFormat}";
            return File.Exists(filePath) ? filePath : null;
        }

        public string TryGetCUPUPath(int index)
        {
            string filePath = $@"{cachePath}\{cupuPrefix}\{index}.{textFileFormat}";
            return File.Exists(filePath) ? filePath : null;
        }

        public void SaveBitmaps(Dictionary<int, BitmapSource> bitmaps, CacheItemType type)
        {
            foreach(var bitmap in bitmaps)
            {
                string name = bitmap.Key.ToString().PadLeft(3, '0');

                using (FileStream fs = File.Create($@"{cachePath}\{type}\{name}.{imageFormat}"))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap.Value));
                    encoder.Save(fs);
                }
            }
        }

        public void SaveBitmap(BitmapSource bitmap, int number, CacheItemType type)
        {
            string name = number.ToString().PadLeft(3, '0');

            using (FileStream fs = File.Create($@"{cachePath}\{type}\{name}.{imageFormat}"))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fs);
            }
        }
    }
}
