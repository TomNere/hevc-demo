using HEVCDemo.Helpers;
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
        private int count = -1;
        private List<FileInfo> files;

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

        private string currentImage;
        public string CurrentImage
        {
            get { return currentImage; }
            set { SetProperty(ref currentImage, value); }
        }

        private DelegateCommand backwardClick;
        public DelegateCommand BackwardClick =>
            backwardClick ?? (backwardClick = new DelegateCommand(ExecuteBackwardClick, CanExecuteBackward));

        private void ExecuteBackwardClick()
        {
            this.CurrentImage = files[--counter].FullName;
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
            this.CurrentImage = files[++counter].FullName;
            this.ForwardClick.RaiseCanExecuteChanged();
            this.BackwardClick.RaiseCanExecuteChanged();
        }

        private bool CanExecuteForward()
        {
            return this.files?.Count > this.counter + 1;
        }

        private DelegateCommand selectVideoClick;
        public DelegateCommand SelectVideoClick =>
            selectVideoClick ?? (selectVideoClick = new DelegateCommand(ExecuteSelectVideoClick));

        async void ExecuteSelectVideoClick()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video file|*.y4m";
            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await FFmpegHelper.ExtractFrames(openFileDialog.FileName);
                Mouse.OverrideCursor = Cursors.Arrow;

                this.ForwardEnabled = true;
                this.files = (new DirectoryInfo(@".\frames\")).GetFiles().ToList();
                if (this.files?.Count > 0)
                {
                    this.CurrentImage = this.files[0].FullName;
                    this.counter = 0;
                    this.ForwardClick.RaiseCanExecuteChanged();
                }
            }
        }
    }
}
