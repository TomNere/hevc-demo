using HEVCDemo.Models;
using Rasyidf.Localization;
using System;
using System.IO;
using System.Linq;
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
                if (!IsFFmpegDownloaded)
                {
                    var result = MessageBox.Show("FFmpegNotFoundMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        GlobalActionsHelper.OnAppStateChanged("DownloadingFFmpegState,Text".Localize(), false, true);
                        await DownloadFFmpeg();
                        MessageBox.Show("FFmpegDownloadedMsg,Text".Localize(), "AppTitle,Title".Localize());
                    }
                    else
                    {
                        GlobalActionsHelper.OnAppStateChanged("FFmpegMissingState,Text".Localize(), false, false);
                        return;
                    }
                }
            }, 
            "DownloadFFmpegTitle,Title".Localize(),
            false,
            "CheckingFFmpegState,Text".Localize(),
            "ReadyState,Text".Localize()
            );
        }

        private async static Task DownloadFFmpeg()
        {
            if (!IsFFmpegDownloaded)
            {
                // By default, FFmpeg executable (and download) path is "."
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full);
            }
        }

        public async static Task InitProperties(VideoCache cache)
        {
            var info = await FFmpeg.GetMediaInfo(cache.LoadedFilePath).ConfigureAwait(false);
            cache.Duration = info.Duration;
            cache.FileSize = info.Size;

            foreach(var videoStream in info.VideoStreams)
            {
                if (videoStream.Framerate > 0)
                {
                    cache.Framerate = videoStream.Framerate;
                }
            }
        }

        public async static Task ExtractFrames(VideoCache cache)
        {
            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $@"-s {cache.VideoSequence.Width}x{cache.VideoSequence.Height} -i {cache.YuvFilePath} -preset fast {cache.YuvFramesDirPath}\%03d.bmp");
        }

        public async static Task ConvertToAnnexB(VideoCache cacheProvider, int startSecond, int endSecond)
        {
            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $"-ss {TimeSpan.FromSeconds(startSecond)} -t {endSecond} -i {cacheProvider.LoadedFilePath} -c:v copy -bsf hevc_mp4toannexb -f hevc {cacheProvider.AnnexBFilePath}");
        }

        public async static Task<bool> ConvertToHevc(VideoCache cache)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(cache.LoadedFilePath);
            var videoStream = mediaInfo?.VideoStreams.FirstOrDefault()?.SetCodec(VideoCodec.hevc);

            if (videoStream == null) return false;

            var framerate = videoStream.Framerate;
            var bitrate = videoStream.Bitrate;

            await FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .SetOutput(cache.HevcFilePath)
                .SetFrameRate(framerate)
                .SetVideoBitrate(bitrate)
                .Start();

            return true;
        }
    }
}
