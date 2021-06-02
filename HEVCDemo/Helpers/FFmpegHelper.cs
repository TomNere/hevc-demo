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
        public async static void InitializeFFmpeg()
        {
            // Set directory where the app will look up for FFmpeg (and download eventually)
            FFmpeg.SetExecutablesPath(@".");

            if (!Directory.Exists(@".\ffmpeg\"))
            {
                Directory.CreateDirectory(@".\ffmpeg\");
                // Download latest version
                MessageBox.Show("FFmpeg downloading...");
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full);

                MessageBox.Show("FFmpeg downloaded!");
            }
        }

        public async static Task<TimeSpan> GetDuration(string fileFullName)
        {
            IMediaInfo info = await FFmpeg.GetMediaInfo(fileFullName).ConfigureAwait(false);
            return info.Duration;
        }

        public async static Task ExtractFrames(CacheProvider cacheProvider)
        {
            IMediaInfo info = await FFmpeg.GetMediaInfo(cacheProvider.AnnexBFilePath).ConfigureAwait(false);
            IVideoStream videoStream = info.VideoStreams.First()?.SetCodec(VideoCodec.png);

            IConversionResult conversionResult = await FFmpeg.Conversions.New()
                .AddStream(videoStream)
                //.AddParameter($"-ss {TimeSpan.FromSeconds(3)}")
                //.AddParameter($"-t {TimeSpan.FromSeconds(3)}")
                .ExtractEveryNthFrame(1, cacheProvider.FramesOutputFileNameBuilder)
                //.SetOutputFormat(Format.)
                .Start();
        }

        public async static Task ConvertToAnnexB(string fileFullName, CacheProvider cacheProvider, TimeSpan duration)
        {
            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $"-ss {TimeSpan.FromSeconds(0)} -t {TimeSpan.FromSeconds(Math.Min(duration.Seconds, 10))} -i {fileFullName} -c:v copy -bsf hevc_mp4toannexb -f hevc {cacheProvider.AnnexBFilePath}");
        }
    }
}
