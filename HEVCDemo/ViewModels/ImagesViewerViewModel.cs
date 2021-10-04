using GalaSoft.MvvmLight.CommandWpf;
using HEVCDemo.Helpers;
using HEVCDemo.Types;
using HEVCDemo.Views;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private bool viewOptionsEnabled = false;
        public bool ViewOptionsEnabled
        {
            get => viewOptionsEnabled;
            set => SetProperty(ref viewOptionsEnabled, value);
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

        private WriteableBitmap currentCupuImage;
        public WriteableBitmap CurrentCupuImage
        {
            get => currentCupuImage;
            set => SetProperty(ref currentCupuImage, value);
        }

        private WriteableBitmap currentPredictionImage;
        public WriteableBitmap CurrentPredictionImage
        {
            get => currentPredictionImage;
            set => SetProperty(ref currentPredictionImage, value);
        }

        private WriteableBitmap currentIntraImage;
        public WriteableBitmap CurrentIntraImage
        {
            get => currentIntraImage;
            set => SetProperty(ref currentIntraImage, value);
        }

        private WriteableBitmap currentMotionVectorsImage;
        public WriteableBitmap CurrentMotionVectorsImage
        {
            get => currentMotionVectorsImage;
            set => SetProperty(ref currentMotionVectorsImage, value);
        }

        private int currentFrameIndex;
        public int CurrentFrameIndex
        {
            get => currentFrameIndex;
            set
            {
                _ = SetCurrentFrame(value);
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

        public Visibility DecodedFramesVisibility => IsDecodedFramesEnabled ? Visibility.Visible : Visibility.Hidden;
        private bool isDecodedFramesEnabled = true;
        public bool IsDecodedFramesEnabled
        {
            get => isDecodedFramesEnabled;
            set
            {
                SetProperty(ref isDecodedFramesEnabled, value);
                RaisePropertyChanged(nameof(DecodedFramesVisibility));
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        public Visibility CupuVisibility => IsCupuEnabled ? Visibility.Visible : Visibility.Hidden;
        private bool isCupuEnabled = true;
        public bool IsCupuEnabled
        {
            get => isCupuEnabled;
            set
            {
                SetProperty(ref isCupuEnabled, value);
                RaisePropertyChanged(nameof(CupuVisibility));
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        public Visibility PredictionVisibility => IsPredictionEnabled ? Visibility.Visible : Visibility.Hidden;
        private bool isPredictionEnabled = true;
        public bool IsPredictionEnabled
        {
            get => isPredictionEnabled;
            set
            {
                SetProperty(ref isPredictionEnabled, value);
                RaisePropertyChanged(nameof(PredictionVisibility));
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        public Visibility IntraVisibility => IsIntraEnabled ? Visibility.Visible : Visibility.Hidden;
        private bool isIntraEnabled = true;
        public bool IsIntraEnabled
        {
            get => isIntraEnabled;
            set
            {
                SetProperty(ref isIntraEnabled, value);
                RaisePropertyChanged(nameof(IntraVisibility));
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        public Visibility MotionVectorsVisibility => IsMotionVectorsEnabled ? Visibility.Visible : Visibility.Hidden;
        private bool isMotionVectorsEnabled = true;
        public bool IsMotionVectorsEnabled
        {
            get => isMotionVectorsEnabled;
            set
            {
                SetProperty(ref isMotionVectorsEnabled, value);
                RaisePropertyChanged(nameof(MotionVectorsVisibility));
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        private bool isVectorsStartEnabled = true;
        public bool IsVectorsStartEnabled
        {
            get => isVectorsStartEnabled;
            set
            {
                SetProperty(ref isVectorsStartEnabled, value);
                _ = SetCurrentFrame(CurrentFrameIndex);
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
            AppState = "ErrorOccuredState,Text".Localize();
            MessageBox.Show($"{actionDescription}\n\n{"ErrorMessageMsg,Text".Localize()}\n{message}", "ErrorOccuredTitle,Title".Localize());
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
                    AppState = "CheckingFFmpegState,Text".Localize();
                }

                if (!FFmpegHelper.FFmpegExists)
                {
                    var result = MessageBox.Show("FFmpegNotFoundMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        AppState = "DownloadingFFmpegState,Text".Localize();
                        await FFmpegHelper.DownloadFFmpeg();
                        MessageBox.Show("FFmpegDownloadedMsg,Text".Localize());
                        AppState = "ReadyState,Text".Localize();
                    }
                    else
                    {
                        AppState = "FFmpegMissingState,Text".Localize();
                    }
                }
                else if (changeState)
                {
                    AppState = "ReadyState,Text".Localize();
                }

                // Enable to allow invoking download by clicking on "Select video"
                Enabled = true;

            }, "DownloadFFmpegTitle,Title".Localize(), HandleError);
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
            return cacheProvider?.videoSequence.FramesCount > CurrentFrameIndex + 1;
        }

        private async Task SetCurrentFrame(int index)
        {
            SetAppState("LoadingIntoCacheState,Text".Localize(), false);

            if (DecodedFramesVisibility == Visibility.Visible)
            {
                await Dispatcher.CurrentDispatcher.Invoke(async() => CurrentFrameImage = await cacheProvider.GetYuvFrame(index, HandleError));
            }
            if (CupuVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentCupuImage = cacheProvider.GetCuPuFrame(index));
            }
            if (PredictionVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentPredictionImage = cacheProvider.GetPredictionFrame(index));
            }
            if (IntraVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentIntraImage = cacheProvider.GetIntraFrame(index));
            }
            if (MotionVectorsVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentMotionVectorsImage = cacheProvider.GetMotionVectorsFrame(index, isVectorsStartEnabled));
            }

            ForwardClick.RaiseCanExecuteChanged();
            BackwardClick.RaiseCanExecuteChanged();

            SetAppState("ReadyState,Text".Localize(), true);
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
            ViewOptionsEnabled = false;

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "h.265 video file|*.mp4|h.265 annexB binary file|*.bin";

            if (openFileDialog.ShowDialog() == true)
            {
                await ActionsHelper.InvokeSafelyAsync(async () =>
                {
                    string filePath = openFileDialog.FileName;

                    if (cacheProvider?.LoadedFilePath == filePath)
                    {
                        MessageBox.Show("FileAlreadyLoadedMsg,Text".Localize(), "AppTitle,Title".Localize());
                        ViewOptionsEnabled = true;
                        return;
                    }

                    cacheProvider = new CacheProvider(filePath);
                    if (cacheProvider.CacheExists)
                    {
                        var result = MessageBox.Show("CacheExistsMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            await cacheProvider.CreateCache(SetAppState);
                        }
                        else
                        {
                            SetAppState("CreatingDemoState,Text".Localize(), false);
                            cacheProvider.InitFramesCount();
                            cacheProvider.ParseProps();
                            await cacheProvider.ProcessFiles();
                        }
                    }
                    else
                    {
                        await cacheProvider.CreateCache(SetAppState);
                    }
                    await cacheProvider.LoadIntoCache(0);

                    Height = cacheProvider.videoSequence.Height;
                    Width = cacheProvider.videoSequence.Width;
                    Resolution = $"{cacheProvider.videoSequence.Width} x {cacheProvider.videoSequence.Height}";
                    double length = new FileInfo(openFileDialog.FileName).Length / 1024d;
                    FileSize = length < 1000 ? $"{length:0.000} KB" : $"{length / 1024:0.000} MB";

                    MaxSliderValue = cacheProvider.videoSequence.FramesCount - 1;
                    CurrentFrameIndex = 0;
                    StartButtonVisibility = Visibility.Hidden;
                    ViewOptionsEnabled = true;
                }, "CreateCacheTitle,Title".Localize(), HandleError);
            }
        }

        #endregion Select video

        #region Info

        private DelegateCommand resolutionClick;
        public DelegateCommand ResolutionClick => resolutionClick ?? (resolutionClick = new DelegateCommand(ExecuteResolutionClick));
        private void ExecuteResolutionClick()
        {
            var infoDialog = new InfoDialog("VideoResolutionTitle,Title".Localize(), "VideoResolution");
            infoDialog.Show();
        }

        private DelegateCommand fileSizeClick;
        public DelegateCommand FileSizeClick => fileSizeClick ?? (fileSizeClick = new DelegateCommand(ExecuteFileSizeClick));
        private void ExecuteFileSizeClick()
        {
            var infoDialog = new InfoDialog("FileSizeTitle,Title".Localize(), "FileSize");
            infoDialog.Show();
        }

        private DelegateCommand decodedFramesInfoClick;
        public DelegateCommand DecodedFramesInfoClick => decodedFramesInfoClick ?? (decodedFramesInfoClick = new DelegateCommand(ExecuteDecodedFramesInfoClick));
        private void ExecuteDecodedFramesInfoClick()
        {
            var infoDialog = new InfoDialog("DecodedFramesLabel,Content".Localize(), "DecodedFrames");
            infoDialog.Show();
        }

        private DelegateCommand codingUnitsInfoClick;
        public DelegateCommand CodingUnitsInfoClick => codingUnitsInfoClick ?? (codingUnitsInfoClick = new DelegateCommand(ExecuteCodingUnitsInfoClick));
        private void ExecuteCodingUnitsInfoClick()
        {
            var infoDialog = new InfoDialog("CodingUnitsLabel,Content".Localize(), "CodingUnits");
            infoDialog.Show();
        }

        private DelegateCommand predictionTypeInfoClick;
        public DelegateCommand PredictionTypeInfoClick => predictionTypeInfoClick ?? (predictionTypeInfoClick = new DelegateCommand(ExecutePredictionTypeInfoClick));
        private void ExecutePredictionTypeInfoClick()
        {
            var infoDialog = new InfoDialog("PredictionLabel,Content".Localize(), "PredictionType");
            infoDialog.Show();
        }

        private DelegateCommand intraPredictionInfoClick;
        public DelegateCommand IntraPredictionInfoClick => intraPredictionInfoClick ?? (intraPredictionInfoClick = new DelegateCommand(ExecuteIntraPredictionInfoClick));
        private void ExecuteIntraPredictionInfoClick()
        {
            var infoDialog = new InfoDialog("IntraLabel,Content".Localize(), "IntraPrediction");
            infoDialog.Show();
        }

        #endregion

        #region Tooltips

        private int imageViewerX;
        public int ScrollViewerX
        {
            get => imageViewerX;
            set 
            {
                SetProperty(ref imageViewerX, value);
            }
        }

        private int imageViewerY;
        public int ScrollViewerY
        {
            get => imageViewerY;
            set
            {
                SetProperty(ref imageViewerY, value);
            }
        }

        private bool isPopupOpen;
        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                SetProperty(ref isPopupOpen, value);
            }
        }

        private InfoPopupParameters popupContent;
        public InfoPopupParameters PopupContent
        {
            get => popupContent;
            set
            {
                SetProperty(ref popupContent, value);
            }
        }

        private DelegateCommand<object> scrollViewerRightClick;
        public DelegateCommand<object> ScrollViewerRightClick => scrollViewerRightClick ?? (scrollViewerRightClick = new DelegateCommand<object>(ExecuteImageRightClick));
        private void ExecuteImageRightClick(object scrollV)
        {
            if (scrollV is ScrollViewer scrollViewer)
            {
                var grid = scrollViewer.Content as Grid;

                // Reset position
                IsPopupOpen = false;
                IsPopupOpen = true;

                PopupContent = cacheProvider.GetUnitDescriptionByLocation(currentFrameIndex, new Point(ScrollViewerX, ScrollViewerY), grid);
            }
        }

        private DelegateCommand closePopupClick;
        public DelegateCommand ClosePopupClick => closePopupClick ?? (closePopupClick = new DelegateCommand(ExecuteClosePopupClick));
        private void ExecuteClosePopupClick()
        {
            IsPopupOpen = false;
        }

        #endregion
    }
}
