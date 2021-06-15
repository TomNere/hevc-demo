using HEVCDemo.Helpers;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media.Imaging;

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
            set { SetProperty(ref enabled, value); }
        }

        private string appState;
        public string AppState
        {
            get => appState;
            set { SetProperty(ref appState, value); }
        }

        private int height;
        public int Height
        {
            get => height;
            set { SetProperty(ref height, value); }
        }

        private int width;
        public int Width
        {
            get => width;
            set { SetProperty(ref width, value); }
        }

        private int maxSliderValue;
        public int MaxSliderValue
        {
            get { return maxSliderValue; }
            set { SetProperty(ref maxSliderValue, value); }
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
            set { SetProperty(ref currentCupuImage, value); }
        }

        private int currentFrameCounter;
        public int CurrentFrameIndex
        {
            get => currentFrameCounter;
            set
            {
                cacheProvider.EnsureFrameInCache(value, SetAppState, HandleError);
                SetProperty(ref currentFrameCounter, value);
                SetCurrentFrame();
            }
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
            MessageBox.Show("Error occured", $"{actionDescription}\n\nError message:\n{message}");
        }

        private void SetAppState(string state)
        {
            AppState = state;
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
            SetCurrentFrame();
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
            SetCurrentFrame();
        }

        private bool CanExecuteForward()
        {
            return cacheProvider?.FramesCount > CurrentFrameIndex + 1;
        }

        private void SetCurrentFrame()
        {
            CurrentFrameImage = cacheProvider.GetDecodedFrameBitmap(CurrentFrameIndex);
            CurrentCupuImage = cacheProvider.GetCupuImageBitmap(CurrentFrameIndex);
            ForwardClick.RaiseCanExecuteChanged();
            BackwardClick.RaiseCanExecuteChanged();
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
                        var result = MessageBox.Show("HEVC demo app", "Cache exists. Do you with to overwrite?", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            await cacheProvider.CreateCache(SetAppState);
                        }
                    }
                    else
                    {
                        cacheProvider.ParseProps();
                    }

                    await cacheProvider.LoadIntoCache(0, SetAppState, HandleError);

                    if (cacheProvider.DecodedFramesBitmaps.Count > 0 &&
                        cacheProvider.DecodedFramesBitmaps.Count == cacheProvider.CupuImagesBitmaps.Count)
                    {
                        CurrentFrameImage = cacheProvider.DecodedFramesBitmaps[0];
                        CurrentCupuImage = cacheProvider.CupuImagesBitmaps[0];
                        MaxSliderValue = cacheProvider.DecodedFramesBitmaps.Count - 1;
                        CurrentFrameIndex = 0;
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
