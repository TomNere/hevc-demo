using HEVCDemo.Helpers;
using HEVCDemo.Parsers;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private int counter;
        private int offset;
        private readonly Dictionary<int, BitmapImage> framesBitmaps = new Dictionary<int, BitmapImage>();
        private readonly Dictionary<int, BitmapImage> cupuBitmaps = new Dictionary<int, BitmapImage>();

        private CacheProvider cacheProvider;

        public ImagesViewerViewModel()
        {
        }

        private bool backwardEnabled = true;
        public bool BackwardEnabled
        {
            get { return backwardEnabled; }
            set { SetProperty(ref backwardEnabled, value); }
        }

        private bool forwardEnabled = true;
        public bool ForwardEnabled
        {
            get { return forwardEnabled; }
            set { SetProperty(ref forwardEnabled, value); }
        }

        private BitmapImage  currentFrameImage;
        public  BitmapImage  CurrentFrameImage
        {
            get { return currentFrameImage; }
            set { SetProperty(ref currentFrameImage, value); }
        }

        public int Counter
        {
            get { return counter; }
            set
            {
                if (!framesBitmaps.ContainsKey(value - offset))
                {
                    LoadIntoCache(value);
                }
                SetProperty(ref counter, value);
                this.CurrentFrameImage = framesBitmaps[value - offset];
                this.CurrentCUPUImage = cupuBitmaps[value - offset];
                this.ForwardClick.RaiseCanExecuteChanged();
                this.BackwardClick.RaiseCanExecuteChanged();
            }
        }

        private void LoadIntoCache(int value)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            offset = (value / 30) * 30;

            var canLoad = Math.Min(framesCount - offset, 30);

            var frames = cacheProvider.GetFrames(offset, canLoad);
            var cupus = cacheProvider.GetCupus(offset, canLoad);

            framesBitmaps.Clear();
            cupuBitmaps.Clear();

            for (int i = 0; i < canLoad; i++)
            {
                framesBitmaps.Add(i, frames[i]);
                cupuBitmaps.Add(i, cupus[i]);
            }
            BackwardClick.RaiseCanExecuteChanged();
            ForwardClick.RaiseCanExecuteChanged();
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private int framesCount;

        private int maxSliderValue;
        public int MaxSliderValue
        {
            get { return maxSliderValue; }
            set { SetProperty(ref maxSliderValue, value); }
        }

        private BitmapImage currentCUPUImage;
        public  BitmapImage CurrentCUPUImage
        {
            get { return currentCUPUImage; }
            set { SetProperty(ref currentCUPUImage, value); }
        }

        private DelegateCommand backwardClick;
        public DelegateCommand BackwardClick =>
            backwardClick ?? (backwardClick = new DelegateCommand(ExecuteBackwardClick, CanExecuteBackward));

        private void ExecuteBackwardClick()
        {
            this.CurrentFrameImage = framesBitmaps[--Counter - offset];
            this.CurrentCUPUImage = cupuBitmaps[Counter - offset];
            this.ForwardClick.RaiseCanExecuteChanged();
            this.BackwardClick.RaiseCanExecuteChanged();
        }

        private bool CanExecuteBackward()
        {
            return this.Counter > 0;
        }

        private DelegateCommand forwardClick;
        public DelegateCommand ForwardClick =>
            forwardClick ?? (forwardClick = new DelegateCommand(ExecuteForwardClick, CanExecuteForward));

        private void ExecuteForwardClick()
        {
            this.CurrentFrameImage = framesBitmaps[++Counter - offset];
            this.CurrentCUPUImage = cupuBitmaps[Counter - offset];
            this.ForwardClick.RaiseCanExecuteChanged();
            this.BackwardClick.RaiseCanExecuteChanged();
        }

        private bool CanExecuteForward()
        {
            return this.framesCount > this.Counter + 1;
        }

        private DelegateCommand selectVideoClick;
        public DelegateCommand SelectVideoClick =>
            selectVideoClick ?? (selectVideoClick = new DelegateCommand(ExecuteSelectVideoClick));

        async void ExecuteSelectVideoClick()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "h.265 video file|*.mp4|h.265 annexB binary file|*.bin";
            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var filePath = openFileDialog.FileName;

                this.cacheProvider = new CacheProvider(filePath);
                if (!cacheProvider.CacheExists)
                {
                    // Extract frames

                    var duration = await FFmpegHelper.GetDuration(filePath);

                    // Already annexB format
                    if (Path.GetExtension(filePath) == "bin")
                    {
                        cacheProvider.CreateAnnexBCopy(filePath);
                    }
                    else
                    {
                        await FFmpegHelper.ConvertToAnnexB(filePath, cacheProvider, duration);
                    }

                    await FFmpegHelper.ExtractFrames(cacheProvider);

                    // Get cupu data
                    await ProcessHelper.RunProcessAsync($@".\TAppDecoder.exe", $@"-b {cacheProvider.AnnexBFilePath} -o c:\out.yuv -p {cacheProvider.CupuFilePath}");
                    File.Delete(@"c:\out.yuv");

                    var cupuParser = new CUPUParser();
                    cupuParser.ParseFile(cacheProvider);
                }

                framesCount = cacheProvider.GetFramesCount();

                LoadIntoCache(0);

                if (framesBitmaps?.Count > 0 && cupuBitmaps?.Count == framesBitmaps.Count)
                {
                    CurrentFrameImage = framesBitmaps[0];
                    CurrentCUPUImage = cupuBitmaps[0];
                    MaxSliderValue = framesCount - 1;
                    offset = 0;
                    Counter = 0;
                }
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
