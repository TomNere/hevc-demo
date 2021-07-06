﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace HEVCDemo.Helpers
{
    public static class FFmpegHelper
    {
        public static bool FFmpegExists => File.Exists(@"ffmpeg.exe");

        public async static Task DownloadFFmpeg()
        {
            if (!FFmpegExists)
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

        public async static Task ExtractFrames(CacheProvider cacheProvider)
        {
            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $@"-s {cacheProvider.Width}x{cacheProvider.Height} -i {cacheProvider.YuvFilePath} -preset fast {cacheProvider.YuvFramesDirPath}\%03d.bmp");
        }

        public async static Task ConvertToAnnexB(CacheProvider cacheProvider)
        {
            var duration = await GetDuration(cacheProvider.LoadedFilePath);

            await ProcessHelper.RunProcessAsync("ffmpeg.exe", $"-ss {TimeSpan.FromSeconds(0)} -t {TimeSpan.FromSeconds(Math.Min(duration.Seconds, 10))} -i {cacheProvider.LoadedFilePath} -c:v copy -bsf hevc_mp4toannexb -f hevc {cacheProvider.AnnexBFilePath}");
        }
    }
}
