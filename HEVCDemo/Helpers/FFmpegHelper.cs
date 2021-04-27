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


        public async static Task ExtractFrames(string fileFullName)
        {
            Directory.CreateDirectory(@".\frames\");

            Func<string, string> outputFileNameBuilder = (number) => { return @".\frames\frame" + number + ".bmp"; };
            IMediaInfo info = await FFmpeg.GetMediaInfo(fileFullName).ConfigureAwait(false);
            IVideoStream videoStream = info.VideoStreams.First()?.SetCodec(VideoCodec.bmp);

            IConversionResult conversionResult = await FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .AddParameter($"-ss {TimeSpan.FromSeconds(3)}")
                .AddParameter($"-t {TimeSpan.FromSeconds(3)}")
                .ExtractEveryNthFrame(1, outputFileNameBuilder)
                .Start();
        }
    }
}
