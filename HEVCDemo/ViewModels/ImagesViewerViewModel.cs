using HEVCDemo.Helpers;
using HEVCDemo.Parsers;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static HEVCDemo.Helpers.CacheProvider;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private int counter;
        private List<BitmapImage> framesBitmaps;
        private List<BitmapImage> cupuBitmaps;

        private List<FileInfo> framesFiles;
        private List<FileInfo> cupuFiles;

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

        //private BitmapSource currentCUPUImage;
        //public BitmapSource CurrentCUPUImage
        //{
        //    get { return currentCUPUImage; }
        //    set { SetProperty(ref currentCUPUImage, value); }
        //}

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
            this.CurrentFrameImage = framesBitmaps[--counter];
            this.CurrentCUPUImage = cupuBitmaps[--counter];
            this.ForwardClick.RaiseCanExecuteChanged();
            this.BackwardClick.RaiseCanExecuteChanged();
        }

        private bool CanExecuteBackward()
        {
            return this.counter > 0;
        }

        private DelegateCommand forwardClick;
        public DelegateCommand ForwardClick =>
            forwardClick ?? (forwardClick = new DelegateCommand(ExecuteForwardClick, CanExecuteForward));

        private void ExecuteForwardClick()
        {
            this.CurrentFrameImage = framesBitmaps[++counter];
            this.CurrentCUPUImage = cupuBitmaps[++counter];
            this.ForwardClick.RaiseCanExecuteChanged();
            this.BackwardClick.RaiseCanExecuteChanged();
        }

        private bool CanExecuteForward()
        {
                return this.framesBitmaps?.Count > this.counter + 1;
        }

        private DelegateCommand selectVideoClick;
        public DelegateCommand SelectVideoClick =>
            selectVideoClick ?? (selectVideoClick = new DelegateCommand(ExecuteSelectVideoClick));

        async void ExecuteSelectVideoClick()
        {
            //var openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Video file|*.h265";
            //if (openFileDialog.ShowDialog() == true)
            {

                var filePath = @"C:\out2.h265";

                this.cacheProvider = new CacheProvider(filePath);
                if (!cacheProvider.CacheExists)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    await FFmpegHelper.ExtractFrames(filePath, cacheProvider);

                    //var cupuParser = new CUPUParser();
                    //cupuParser.ParseFile(cacheProvider);

                    Mouse.OverrideCursor = Cursors.Arrow;
                }

                //var cupuParser = new CUPUParser();
                //cupuParser.ParseFile(cacheProvider);
                
                if (cupuBitmaps != null)
                {
                    //cacheProvider.SaveBitmaps(bitmaps, CacheItemType.Cupu);
                }

                framesBitmaps = cacheProvider.GetFrames(0, 30);
                cupuBitmaps = cacheProvider.GetCupus(0, 30);

                if (framesBitmaps?.Count > 0 && cupuBitmaps?.Count == framesBitmaps.Count)
                {
                    CurrentFrameImage = framesBitmaps[0];
                    CurrentCUPUImage = cupuBitmaps[0];
                    counter = 0;
                    ForwardClick.RaiseCanExecuteChanged();
                }
            }
        }
    }
}
