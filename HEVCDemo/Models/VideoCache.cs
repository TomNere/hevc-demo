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
        private const int precachedBitmapsRange = 5;
        private const string cachePrefix = "cache";

        private const string textExtension = ".txt";
        private const string annexBExtension = ".bin";
        private const string yuvExtension = ".yuv";
        private const string mp4Extension = ".mp4";

        private readonly string cacheDirPath;
        private readonly Dictionary<int, HevcBitmaps> PrecachedHevcBitmaps = new Dictionary<int, HevcBitmaps>();

        private List<FileInfo> orderedYuvFramesFiles;

        public string LoadedFilePath;

        public bool CacheExists => File.Exists(PropsFilePath);
        public bool IsMp4 => Path.GetExtension(LoadedFilePath) == mp4Extension;
        public string StatsDirPath;
        public string CodingUnitsFilePath;
        public string PropsFilePath;
        public string PredictionTypeFilePath;
        public string IntraPredictionFilePath;
        public string InterPredictionFilePath;
        public string AnnexBFilePath;
        public string HevcFilePath;
        public string YuvFilePath;
        public string YuvFramesDirPath;

        public TimeSpan Duration { get; set; }
        public double FileSize { get; set; }
        public double Framerate { get; set; }
        public int StartSecond { get; set; }
        public int EndSecond { get; set; }
        public VideoSequence VideoSequence;

        public static bool CacheDirectoryExists => Directory.Exists(cachePrefix);

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
            HevcFilePath = $@"{cacheDirPath}\hevcConverted{mp4Extension}";
            YuvFilePath = $@"{cacheDirPath}\yuvFile{yuvExtension}";
        }

        public async Task CreateCache(int startSecond, int endSecond, bool convert)
        {
            StartSecond = startSecond;
            EndSecond = endSecond;

            // Clear at first
            if (Directory.Exists(cacheDirPath))
            {
                Directory.Delete(cacheDirPath, true);
            }

            InitCacheFolders();

            // Try to convert to hevc
            if (convert)
            {
                GlobalActionsHelper.OnAppStateChanged("ConvertingToHevcState,Text".Localize(), false, true);
                await FFmpegHelper.ConvertToHevc(this);
                LoadedFilePath = HevcFilePath;
            }

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
            GlobalActionsHelper.OnAppStateChanged("CreatingDemoDataState,Text".Localize(), false, true);
            _ = await ProcessHelper.RunProcessAsync($@".\TAppDecoder.exe", $@"-b {AnnexBFilePath} -o {YuvFilePath} -p {StatsDirPath}");

            VideoSequence = new VideoSequence();

            // Parse properties
            ParseProps();

            // Extract frames
            GlobalActionsHelper.OnAppStateChanged("LoadingDemoDataState,Text".Localize(), false, true);
            var framesLoading = FFmpegHelper.ExtractFrames(this);

            await ProcessFiles();
            await framesLoading;
            InitializeYuvFramesFiles();
            File.Delete(AnnexBFilePath);
            File.Delete(YuvFilePath);
            if (LoadedFilePath == HevcFilePath)
            {
                File.Delete(HevcFilePath);
            }
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
                throw new Exception("ErrorCreatingDemoDataEx,Text".Localize());
            }
        }

        /// <summary>
        /// Precache HevcBitmaps by given range and return hevc bitmaps on index
        /// </summary>
        private async Task<HevcBitmaps> PrecacheHevcBitmaps(int index, int range, ViewConfiguration configuration)
        {
            return await Task.Run(() =>
            {
                lock (PrecachedHevcBitmaps)
                {
                    try
                    {
                        var bitmapsToPrecache = new List<(int, HevcBitmaps)>();

                        for (int i = Math.Max(0, index - range); i < Math.Min(VideoSequence.FramesCount, index + range); i++)
                        {
                            // Frame already precached, skip...
                            if (PrecachedHevcBitmaps.ContainsKey(i)) continue;

                            WriteableBitmap allOthers = null;
                            WriteableBitmap predictionType = null;
                            bool isValid = true;

                            // Get YUV frame
                            if (configuration.IsYuvFrameVisible)
                            {
                                var yuvFrame = new BitmapImage();
                                using (var imageStreamSource = new FileStream(orderedYuvFramesFiles[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    yuvFrame.BeginInit();
                                    yuvFrame.StreamSource = imageStreamSource;
                                    yuvFrame.CacheOption = BitmapCacheOption.OnLoad;
                                    yuvFrame.EndInit();
                                }

                                allOthers = new WriteableBitmap(yuvFrame);
                            }

                            // Init if YUV frame not enabled
                            if (allOthers == null)
                            {
                                allOthers = BitmapFactory.New(VideoSequence.Width, VideoSequence.Height);
                            }

                            // Prediction types
                            if (configuration.IsPredictionTypeVisible)
                            {
                                predictionType = BitmapFactory.New(VideoSequence.Width, VideoSequence.Height);
                                if (!GetPredictionTypeBitmap(i, predictionType))
                                {
                                    isValid = false;
                                }
                            }

                            // Intra prediction
                            if (configuration.IsIntraPredictionVisible)
                            {
                                if (!GetIntraPredictionBitmap(i, allOthers))
                                {
                                    isValid = false;
                                }
                            }

                            // Inter prediction
                            if (configuration.IsInterPredictionVisible)
                            {
                                if (!GetInterPredictionBitmap(i, allOthers, configuration.IsMotionVectorsStartEnabled))
                                {
                                    isValid = false;
                                }
                            }

                            // Coding and prediction units
                            if (configuration.IsCodingPredictionUnitsVisible)
                            {
                                if (!GetCodingPredictionUnitsBitmap(i, allOthers))
                                {
                                    isValid = false;
                                }
                            }

                            allOthers.Freeze();
                            predictionType?.Freeze();

                            bitmapsToPrecache.Add((i, new HevcBitmaps(allOthers, predictionType, isValid)));
                        }

                        // Remove precached frames outside of predefined range
                        foreach (var item in PrecachedHevcBitmaps.Where(item => item.Key < index - precachedBitmapsRange || item.Key > index + precachedBitmapsRange).ToList())
                        {
                            PrecachedHevcBitmaps.Remove(item.Key);
                        }
                        GC.Collect();

                        // Add to dictionary
                        foreach (var item in bitmapsToPrecache)
                        {
                            PrecachedHevcBitmaps.Add(item.Item1, item.Item2);
                        }

                        return PrecachedHevcBitmaps[index];
                    }
                    catch (Exception ex)
                    {
                        if (ex is OutOfMemoryException || ex is InsufficientMemoryException)
                        {
                            MessageBox.Show($"{"InsufficientMemoryEx,Text".Localize()}\n\n{ex.Message}", "AppTitle,Title".Localize());
                            return null;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            });
        }

        public async Task<HevcBitmaps> GetFrameBitmaps(int index, string afterStateText, ViewConfiguration configuration)
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
                toReturn = await PrecacheHevcBitmaps(index, 1, configuration);
                },
                "LoadingIntoCacheState,Text".Localize(),
                true,
                "LoadingIntoCacheState,Text".Localize(),
                afterStateText);
            }

            // Precache in background for later use
            _ = PrecacheHevcBitmaps(index, precachedBitmapsRange, configuration);

            return toReturn;
        }

        public bool GetIntraPredictionBitmap(int index, WriteableBitmap writeableBitmap)
        {
            var frame = VideoSequence.GetFrameByPoc(index);
            if (frame == null) return false;

            foreach (var cu in frame.CodingUnits)
            {
                IntraParser.WriteBitmaps(cu, writeableBitmap);
            }
            return true;
        }

        public bool GetPredictionTypeBitmap(int index, WriteableBitmap writeableBitmap)
        {
            var frame = VideoSequence.GetFrameByPoc(index);
            if (frame == null) return false;

            foreach (var cu in frame.CodingUnits)
            {
                PredictionParser.WriteBitmaps(cu, writeableBitmap);
            }
            return true;
        }

        public bool GetCodingPredictionUnitsBitmap(int index, WriteableBitmap writeableBitmap)
        {
            var frame = VideoSequence.GetFrameByPoc(index);
            if (frame == null) return false;

            foreach (var cu in frame.CodingUnits)
            {
                CupuParser.WriteBitmaps(cu, writeableBitmap);
            }
            return true;
        }

        public bool GetInterPredictionBitmap(int index, WriteableBitmap writeableBitmap, bool isStartEnabled)
        {
            var frame = VideoSequence.GetFrameByPoc(index);
            if (frame == null) return false;

            foreach (var cu in frame.CodingUnits)
            {
                MotionVectorsParser.WriteBitmaps(cu, writeableBitmap, isStartEnabled);
            }
            return true;
        }

        public void ClearPrecachedHevcBitmaps()
        {
            lock (PrecachedHevcBitmaps)
            {
                PrecachedHevcBitmaps.Clear();
            }
        }

        public static async Task ClearCache()
        {
            if (!Directory.Exists(cachePrefix)) return;

            await OperationsHelper.InvokeSafelyAsync(
                () => Directory.Delete(cachePrefix, true),
                "ClearingCacheState,Text".Localize(),
                true,
                "ClearingCacheState,Text".Localize(),
                "ReadyState,Text".Localize());

            GlobalActionsHelper.OnCacheCleared();
        }
    }
}
