using HEVCDemo.Helpers;
using HEVCDemo.Hevc;
using HEVCDemo.Parsers;
using Rasyidf.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Models
{
    public class VideoCache
    {
        public readonly string LoadedFilePath;

        private const int precachedBitmapsRange = 5;
        private const string cachePrefix = "cache";

        private const string textExtension = ".txt";
        private const string annexBExtension = ".bin";
        private const string yuvExtension = ".yuv";

        private readonly string cacheDirPath;

        private List<FileInfo> orderedYuvFramesFiles;
        private readonly Dictionary<int, HevcBitmaps> PrecachedHevcBitmaps = new Dictionary<int, HevcBitmaps>();

        public bool CacheExists => File.Exists(PropsFilePath);
        public bool IsMp4 => Path.GetExtension(LoadedFilePath) == ".mp4";
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

        public VideoCache(string filePath)
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

            // Set paths
            YuvFramesDirPath = $@"{cacheDirPath}\yuvFrames";
            AnnexBFilePath = $@"{cacheDirPath}\annexBFile{annexBExtension}";
            YuvFilePath = $@"{cacheDirPath}\yuvFile{yuvExtension}";
        }

        public async Task CreateCache(int startSecond, int endSecond)
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
                GlobalActionsHelper.OnAppStateChanged("ConvertingAnnexBState,Text".Localize(), false, true);
                await FFmpegHelper.ConvertToAnnexB(this, startSecond, endSecond);
            }
            // Only copy
            else
            {
                File.Copy(LoadedFilePath, AnnexBFilePath);
            }

            // Get stats data from annexB file
            GlobalActionsHelper.OnAppStateChanged("CreatingDemoData,Text".Localize(), false, true);
            _ = await ProcessHelper.RunProcessAsync($@".\TAppDecoder.exe", $@"-b {AnnexBFilePath} -o {YuvFilePath} -p {StatsDirPath}");

            // Parse properties
            ParseProps();

            // Extract frames
            GlobalActionsHelper.OnAppStateChanged("LoadingDemoData,Text".Localize(), false, true);
            var framesLoading = FFmpegHelper.ExtractFrames(this);

            await ProcessFiles();
            await framesLoading;
            InitializeYuvFramesFiles();
            File.Delete(AnnexBFilePath);
            File.Delete(YuvFilePath);
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

        public void InitializeYuvFramesFiles()
        {
            var files = new DirectoryInfo(YuvFramesDirPath).GetFiles();
            orderedYuvFramesFiles = files.OrderBy(file => int.Parse(Path.GetFileNameWithoutExtension(file.FullName))).ToList();

            if (files.Length != VideoSequence.FramesCount)
            {
                throw new Exception("ErrorCreatingDemoData,Text".Localize());
            }
        }

        /// <summary>
        /// Precache HevcBitmaps by given range and return hevc bitmaps on index
        /// </summary>
        private async Task<HevcBitmaps> PrecacheHevcBitmaps(int index, int range, bool isMotionVectorStartEnabled)
        {
            return await Task.Run(() =>
            {
                lock (PrecachedHevcBitmaps)
                {
                    // Remove precached frames outside of predefined range
                    foreach (var item in PrecachedHevcBitmaps.Where(item => item.Key < index - precachedBitmapsRange || item.Key > index + precachedBitmapsRange).ToList())
                    {
                        PrecachedHevcBitmaps.Remove(item.Key);
                    }
                    GC.Collect();

                    for (int i = Math.Max(0, index - range); i < Math.Min(VideoSequence.FramesCount, index + range); i++)
                    {
                        // Frame already precached, skip...
                        if (PrecachedHevcBitmaps.ContainsKey(i)) continue;

                        try
                        {
                            var yuvFrame = new BitmapImage();
                            using (var imageStreamSource = new FileStream(orderedYuvFramesFiles[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                yuvFrame.BeginInit();
                                yuvFrame.StreamSource = imageStreamSource;
                                yuvFrame.CacheOption = BitmapCacheOption.OnLoad;
                                yuvFrame.EndInit();
                            }

                            yuvFrame.Freeze();
                            var hevcBitmaps = new HevcBitmaps
                            {
                                YuvFrame = yuvFrame,
                                CodingPredictionUnits = GetCodingPredictionUnitsBitmap(i),
                                PredictionType = GetPredictionTypeBitmap(i),
                                IntraPrediction = GetIntraPredictionBitmap(i),
                                InterPrediction = GetInterPredictionBitmap(i, isMotionVectorStartEnabled)
                            };

                            PrecachedHevcBitmaps.Add(i, hevcBitmaps);
                        }
                        catch (Exception ex)
                        {
                            if (ex is OutOfMemoryException || ex is InsufficientMemoryException)
                            {
                                MessageBox.Show($"{"InsufficientMemory,Text".Localize()}\n\n{ex.Message}", "AppTitle,Title".Localize());
                                return null;
                            }
                            else
                            {
                                throw ex;
                            }
                        }

                    }

                    return PrecachedHevcBitmaps[index];
                }
            });
        }

        public async Task<HevcBitmaps> GetFrameBitmaps(int index, string afterStateText, bool isMotionVectorStartEnabled)
        {
            HevcBitmaps toReturn = null;

            // Check if precached
            lock (PrecachedHevcBitmaps)
            {
                if (PrecachedHevcBitmaps.ContainsKey(index))
                {
                    toReturn = PrecachedHevcBitmaps[index];
                }
            }

            // Not precached
            if (toReturn == null)
            {
                await OperationsHelper.InvokeSafelyAsync(async () =>
                {
                    // Load only 3 frames to save time
                    toReturn = await PrecacheHevcBitmaps(index, 1, isMotionVectorStartEnabled);
                },
                "LoadingIntoCache,Text".Localize(),
                true,
                "LoadingIntoCache,Text".Localize(),
                afterStateText);
            }

            // Precache in background for later use
            _ = PrecacheHevcBitmaps(index, precachedBitmapsRange, isMotionVectorStartEnabled);

            return toReturn;
        }

        public WriteableBitmap GetIntraPredictionBitmap(int index)
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

        public WriteableBitmap GetPredictionTypeBitmap(int index)
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

        public WriteableBitmap GetCodingPredictionUnitsBitmap(int index)
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

        public WriteableBitmap GetInterPredictionBitmap(int index, bool isStartEnabled)
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

        public void ClearPrecachedHevcBitmaps()
        {
            PrecachedHevcBitmaps.Clear();
        }

        public static async Task ClearCache()
        {
            if (!Directory.Exists(cachePrefix)) return;

            await OperationsHelper.InvokeSafelyAsync(
                () => Directory.Delete(cachePrefix, true),
                "ClearingCache,Text".Localize(),
                true,
                "ClearingCache,Text".Localize(),
                "ReadyState,Text".Localize());

            GlobalActionsHelper.OnCacheCleared();
        }
    }
}
