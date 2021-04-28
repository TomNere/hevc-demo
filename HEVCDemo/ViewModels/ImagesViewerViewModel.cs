using HEVCDemo.Helpers;
using HEVCDemo.Parsers;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private int counter;
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

        private string currentFrameImage;
        public string CurrentFrameImage
        {
            get { return currentFrameImage; }
            set { SetProperty(ref currentFrameImage, value); }
        }

        private string currentCUPUImage;
        public string CurrentCUPUImage
        {
            get { return currentCUPUImage; }
            set { SetProperty(ref currentCUPUImage, value); }
        }

        private DelegateCommand backwardClick;
        public DelegateCommand BackwardClick =>
            backwardClick ?? (backwardClick = new DelegateCommand(ExecuteBackwardClick, CanExecuteBackward));

        private void ExecuteBackwardClick()
        {
            this.CurrentFrameImage = framesFiles[--counter].FullName;
            this.CurrentCUPUImage = cupuFiles[--counter].FullName;
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
            this.CurrentFrameImage = framesFiles[++counter].FullName;
            this.CurrentCUPUImage = cupuFiles[++counter].FullName;
            this.ForwardClick.RaiseCanExecuteChanged();
            this.BackwardClick.RaiseCanExecuteChanged();
        }

        private bool CanExecuteForward()
        {
                return this.framesFiles?.Count > this.counter + 1;
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

                    var cupuParser = new CUPUParser();
                    cupuParser.ParseFile(cacheProvider);

                    Mouse.OverrideCursor = Cursors.Arrow;
                }

                //var cupuParser = new CUPUParser();
                //cupuParser.ParseFile(cacheProvider);

                framesFiles = cacheProvider.GetAllFrames();
                cupuFiles = cacheProvider.GetAllCupus();

                if (framesFiles?.Count > 0 && cupuFiles.Count == framesFiles.Count)
                {
                    CurrentFrameImage = framesFiles[0].FullName;
                    CurrentCUPUImage = cupuFiles[0].FullName;
                    counter = 0;
                    ForwardClick.RaiseCanExecuteChanged();
                }
            }
        }
    }
}
