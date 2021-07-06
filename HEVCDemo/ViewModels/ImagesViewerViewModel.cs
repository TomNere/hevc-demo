using HEVCDemo.Helpers;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private CacheProvider cacheProvider;

        #region Binding properties

        private bool enabled = false;
        public bool Enabled
        {
            get => enabled;
            set => SetProperty(ref enabled, value);
        }

        private string appState;
        public string AppState
        {
            get => appState;
            set => SetProperty(ref appState, value);
        }

        private double height;
        public double Height
        {
            get => height;
            set => SetProperty(ref height, value);
        }

        private double width;
        public double Width
        {
            get => width;
            set => SetProperty(ref width, value);
        }

        private int maxSliderValue;
        public int MaxSliderValue
        {
            get { return maxSliderValue; }
            set => SetProperty(ref maxSliderValue, value);
        }

        private BitmapImage currentFrameImage;
        public BitmapImage CurrentFrameImage
        {
            get => currentFrameImage;
            set { SetProperty(ref currentFrameImage, value); }
        }

        private BitmapImage currentCupuImage;
        public BitmapImage CurrentCupuImage
        {
            get => currentCupuImage;
            set => SetProperty(ref currentCupuImage, value);
        }

        private int currentFrameIndex;
        public int CurrentFrameIndex
        {
            get => currentFrameIndex;
            set
            {
                SetCurrentFrame(value);
                SetProperty(ref currentFrameIndex, value);
            }
        }

        private Visibility startButtonVisibility = Visibility.Visible;
        public Visibility StartButtonVisibility
        {
            get => startButtonVisibility;
            set => SetProperty(ref startButtonVisibility, value);
        }

        private string resolution;
        public string Resolution
        {
            get => resolution;
            set => SetProperty(ref resolution, value);
        }

        private string fileSize;
        public string FileSize
        {
            get => fileSize;
            set => SetProperty(ref fileSize, value);
        }
        #endregion

        public ImagesViewerViewModel()
        {
            CheckFFmpeg(true);
        }

        private void HandleError(string actionDescription, string message)
        {
            Enabled = true;
            AppState = "Error occured";
            MessageBox.Show($"{actionDescription}\n\nError message:\n{message}", "Error occured");
        }

        private void SetAppState(string state, bool enabled)
        {
            AppState = state;
            Enabled = enabled;
        }

        private async void CheckFFmpeg(bool changeState)
        {
            await ActionsHelper.InvokeSafelyAsync(async () =>
            {
                if (changeState)
                {
                    AppState = "Checking FFmpeg";
                }

                if (!FFmpegHelper.FFmpegExists)
                {
                    var result = MessageBox.Show("FFmpeg not found. Do you wish to automatically download it?", "HEVC demo app", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        AppState = "Downloading FFmpeg";
                        await FFmpegHelper.DownloadFFmpeg();
                        MessageBox.Show("FFmpeg sucessfully downloaded.");
                        AppState = "Ready";
                    }
                    else
                    {
                        AppState = "FFmpeg missing";
                    }
                }
                else if (changeState)
                {
                    AppState = "Ready";
                }

                // Enable to allow invoking download by clicking on "Select video"
                Enabled = true;

            }, "Download FFmpeg helper", HandleError);
        }

        #region Backward Forward controls

        private DelegateCommand backwardClick;
        public DelegateCommand BackwardClick =>
            backwardClick ?? (backwardClick = new DelegateCommand(ExecuteBackwardClick, CanExecuteBackward));

        private void ExecuteBackwardClick()
        {
            CurrentFrameIndex--;
        }

        private bool CanExecuteBackward()
        {
            return this.CurrentFrameIndex > 0;
        }

        private DelegateCommand forwardClick;
        public DelegateCommand ForwardClick =>
            forwardClick ?? (forwardClick = new DelegateCommand(ExecuteForwardClick, CanExecuteForward));

        private void ExecuteForwardClick()
        {
            CurrentFrameIndex++;
        }

        private bool CanExecuteForward()
        {
            return cacheProvider?.FramesCount > CurrentFrameIndex + 1;
        }

        private async void SetCurrentFrame(int index)
        {
            await cacheProvider.EnsureFrameInCache(index, SetAppState, HandleError);
            Dispatcher.CurrentDispatcher.Invoke(() => CurrentFrameImage = cacheProvider.YuvFramesBitmaps[index]);
            Dispatcher.CurrentDispatcher.Invoke(() => CurrentCupuImage = cacheProvider.CupuFramesBitmaps[index]);
            ForwardClick.RaiseCanExecuteChanged();
            BackwardClick.RaiseCanExecuteChanged();
        }

        #endregion

        #region Zoom controls

        private DelegateCommand zoomOutClick;
        public DelegateCommand ZoomOutClick => zoomOutClick ?? (zoomOutClick = new DelegateCommand(ExecuteZoomOutClick));

        private void ExecuteZoomOutClick()
        {
            Height /= 1.05;
            Width /= 1.05;
        }

        private DelegateCommand zoomInClick;
        public DelegateCommand ZoomInClick => zoomInClick ?? (zoomInClick = new DelegateCommand(ExecuteZoomInClick));
        private void ExecuteZoomInClick()
        {
            Height *= 1.05;
            Width *= 1.05;
        }

        #endregion

        #region Select Video

        private DelegateCommand selectVideoClick;
        public DelegateCommand SelectVideoClick =>
            selectVideoClick ?? (selectVideoClick = new DelegateCommand(ExecuteSelectVideoClick));

        private async void ExecuteSelectVideoClick()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "h.265 video file|*.mp4|h.265 annexB binary file|*.bin";

            if (openFileDialog.ShowDialog() == true)
            {
                await ActionsHelper.InvokeSafelyAsync(async () =>
                {
                    string filePath = openFileDialog.FileName;

                    cacheProvider = new CacheProvider(filePath);
                    if (cacheProvider.CacheExists)
                    {
                        var result = MessageBox.Show("Cache exists. Do you wish to overwrite?", "HEVC demo app", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            await cacheProvider.CreateCache(SetAppState);
                        }
                        else
                        {
                            cacheProvider.InitFramesCount();
                            cacheProvider.ParseProps();
                        }
                    }
                    else
                    {
                        await cacheProvider.CreateCache(SetAppState);
                    }

                    await cacheProvider.LoadIntoCache(0, SetAppState);

                    if (cacheProvider.YuvFramesBitmaps.Count > 0 &&
                        cacheProvider.YuvFramesBitmaps.Count == cacheProvider.CupuFramesBitmaps.Count)
                    {
                        Height = cacheProvider.Height;
                        Width = cacheProvider.Width;
                        Resolution = $"{cacheProvider.Width} x {cacheProvider.Height}";
                        double length = new FileInfo(openFileDialog.FileName).Length / 1024d;
                        FileSize = length < 1000 ? $"{length:0.000} KB" : $"{length / 1024:0.000} MB";

                        Dispatcher.CurrentDispatcher.Invoke(() => CurrentFrameImage = cacheProvider.YuvFramesBitmaps[0]);
                        Dispatcher.CurrentDispatcher.Invoke(() => CurrentCupuImage = cacheProvider.CupuFramesBitmaps[0]);
                        MaxSliderValue = cacheProvider.FramesCount - 1;
                        CurrentFrameIndex = 0;
                        StartButtonVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        AppState = "Error - mismatch in count of frames and cupu images";
                    }
                }, "Create cache", HandleError);
            }
        }

        #endregion Select video
    }
}
