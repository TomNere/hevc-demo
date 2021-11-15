using HEVCDemo.Models;
using Rasyidf.Localization;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace HEVCDemo.Helpers
{
    public static class FFmpegHelper
    {
        public static bool IsFFmpegDownloaded => File.Exists(@"ffmpeg.exe");

        public static async Task EnsureFFmpegIsDownloaded()
        {
            await OperationsHelper.InvokeSafelyAsync(async () =>
            {
                GlobalActionsHelper.OnAppStateChanged("CheckingFFmpegState,Text".Localize(), false);

                if (!IsFFmpegDownloaded)
                {
                    var result = MessageBox.Show("FFmpegNotFoundMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        GlobalActionsHelper.OnAppStateChanged("DownloadingFFmpegState,Text".Localize(), false);
                        await DownloadFFmpeg();
                        MessageBox.Show("FFmpegDownloadedMsg,Text".Localize());
                    }
                    else
                    {
                        GlobalActionsHelper.OnAppStateChanged("FFmpegMissingState,Text".Localize(), false);
                        return;
                    }
                }

                SetReadyState();
            }, "DownloadFFmpegTitle,Title".Localize(), false);

            void SetReadyState()
            {
                // Enable to allow invoking download by clicking on "Select video"
                GlobalActionsHelper.OnAppStateChanged("ReadyState,Text".Localize(), true);
            }
        }

        private async static Task DownloadFFmpeg()
        {
            if (!IsFFmpegDownloaded)
            {
                // By default, FFmpeg executable (and download) path is "."
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full);
            }
        }

        public async static Task<TimeSpan> GetDuration(string fileFullName)
        {
            IMediaInfo info = await FFmpeg.GetMediaInfo(fileFullName).ConfigureAwait(false);
            return info.Duration;
        }

        public async static Task ExtractFrames(VideoCache cacheProvider)
        {
            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $@"-s {cacheProvider.VideoSequence.Width}x{cacheProvider.VideoSequence.Height} -i {cacheProvider.YuvFilePath} -preset fast {cacheProvider.YuvFramesDirPath}\%03d.bmp");
        }

        public async static Task ConvertToAnnexB(VideoCache cacheProvider)
        {
            var duration = await GetDuration(cacheProvider.LoadedFilePath);

            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $"-ss {TimeSpan.FromSeconds(0)} -t {TimeSpan.FromSeconds(Math.Min(duration.Seconds, 10))} -i {cacheProvider.LoadedFilePath} -c:v copy -bsf hevc_mp4toannexb -f hevc {cacheProvider.AnnexBFilePath}");
        }
    }
}
