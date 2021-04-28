using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEVCDemo.Helpers
{
    public class CacheProvider
    {
        private const string cachePrefix = "cache";
        private const string framesPrefix = "frames";
        private const string cupuPrefix = "cupu";

        private const string imageFormat = "bmp";
        private const string textFileFormat = "txt";

        private readonly string cachePath;

        public string CupuFilePath;

        public bool CacheExists => !string.IsNullOrEmpty(cachePath) && Directory.Exists(cachePath);
        public Func<string, string> FramesOutputFileNameBuilder => (number) => { return $@"{cachePath}\{framesPrefix}\{number}.{imageFormat}"; };

        public CacheProvider(string filePath)
        {
            cachePath = $@".\{cachePrefix}\{Path.GetFileNameWithoutExtension(filePath)}";
            CupuFilePath = $@"{cachePath}\cupu.{textFileFormat}";
        }

        public void InitCacheFolders()
        {
            Directory.CreateDirectory($@"{cachePath}\{framesPrefix}");
            Directory.CreateDirectory($@"{cachePath}\{cupuPrefix}");
        }

        public List<FileInfo> GetAllFrames()
        {
            return new DirectoryInfo($@"{cachePath}\{framesPrefix}").GetFiles().ToList();
        }

        public List<FileInfo> GetAllCupus()
        {
            return new DirectoryInfo($@"{cachePath}\{cupuPrefix}").GetFiles().ToList();
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

        public void SaveCupuBitmap(Bitmap bitmap, int number)
        {
            bitmap.Save($@"{cachePath}\{cupuPrefix}\{number}.{imageFormat}");
        }
    }
}
