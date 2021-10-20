using HEVCDemo.Helpers;
using HEVCDemo.Types;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private CacheProvider cacheProvider;
        private double zoom = 1;
        private int realHeight;
        private int realWidth;

        private bool isVectorsStartEnabled = true;

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

        private WriteableBitmap currentHighlightImage;
        public WriteableBitmap CurrentHighlightImage
        {
            get => currentHighlightImage;
            set => SetProperty(ref currentHighlightImage, value);
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

        private Visibility highlightVisibility = Visibility.Hidden;
        public Visibility HighlightVisibility
        {
            get => highlightVisibility;
            set => SetProperty(ref highlightVisibility, value);
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


        private Visibility decodedFramesVisibility = Visibility.Visible;
        public Visibility DecodedFramesVisibility
        {
            get => decodedFramesVisibility;
            set
            {
                SetProperty(ref decodedFramesVisibility, value);
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }


        private Visibility codingUnitsVisibility = Visibility.Visible;
        public Visibility CodingUnitsVisibility
        {
            get => codingUnitsVisibility;
            set
            {
                SetProperty(ref codingUnitsVisibility, value);
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }


        private Visibility predictionTypeVisibility = Visibility.Visible;
        public Visibility PredictionTypeVisibility
        {
            get => predictionTypeVisibility;
            set
            {
                SetProperty(ref predictionTypeVisibility, value);
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }


        private Visibility intraPredictionVisibility = Visibility.Visible;
        public Visibility IntraPredictionVisibility
        {
            get => intraPredictionVisibility;
            set
            {
                SetProperty(ref intraPredictionVisibility, value);
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }


        private Visibility interPredictionVisibility = Visibility.Visible;
        public Visibility InterPredictionVisibility
        {
            get => interPredictionVisibility;
            set
            {
                SetProperty(ref interPredictionVisibility, value);
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        #endregion

        public ImagesViewerViewModel()
        {
            CheckFFmpeg(true);
            BindEventHandlers();
        }

        #region Event handlers

        private void BindEventHandlers()
        {
            ViewOptionsHelper.DecodedFramesVisibilityChanged += DecodedFramesVisibilityChanged;
            ViewOptionsHelper.CodingUnitsVisibilityChanged += CodingUnitsVisibilityChanged;
            ViewOptionsHelper.PredictionTypeVisibilityChanged += PredictionTypeVisibilityChanged;
            ViewOptionsHelper.IntraPredictionVisibilityChanged += IntraPredictionVisibilityChanged;
            ViewOptionsHelper.InterPredictionVisibilityChanged += InterPredictionVisibilityChanged;
            ViewOptionsHelper.VectorsStartVisibilityChanged += VectorsStartVisibilityChanged;
        }

        private void DecodedFramesVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            DecodedFramesVisibility = ViewOptionsHelper.ConvertBoolToVisibility(e.IsVisible);
        }

        private void CodingUnitsVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            CodingUnitsVisibility = ViewOptionsHelper.ConvertBoolToVisibility(e.IsVisible);
        }

        private void PredictionTypeVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            PredictionTypeVisibility = ViewOptionsHelper.ConvertBoolToVisibility(e.IsVisible);
        }

        private void IntraPredictionVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            IntraPredictionVisibility = ViewOptionsHelper.ConvertBoolToVisibility(e.IsVisible);
        }

        private void InterPredictionVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            InterPredictionVisibility = ViewOptionsHelper.ConvertBoolToVisibility(e.IsVisible);
        }

        private void VectorsStartVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            isVectorsStartEnabled = e.IsVisible;
            _ = SetCurrentFrame(CurrentFrameIndex);
        }

        #endregion

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
            if (cacheProvider == null) return;

            SetAppState("LoadingIntoCacheState,Text".Localize(), false);

            if (DecodedFramesVisibility == Visibility.Visible)
            {
                await Dispatcher.CurrentDispatcher.Invoke(async() => CurrentFrameImage = await cacheProvider.GetYuvFrame(index, HandleError));
            }
            if (CodingUnitsVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentCupuImage = cacheProvider.GetCuPuFrame(index));
            }
            if (PredictionTypeVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentPredictionImage = cacheProvider.GetPredictionFrame(index));
            }
            if (IntraPredictionVisibility == Visibility.Visible)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => CurrentIntraImage = cacheProvider.GetIntraFrame(index));
            }
            if (InterPredictionVisibility == Visibility.Visible)
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
            zoom -= 0.05;

            Height = (int) realHeight * zoom;
            Width = (int) realWidth * zoom;
        }

        private DelegateCommand zoomInClick;
        public DelegateCommand ZoomInClick => zoomInClick ?? (zoomInClick = new DelegateCommand(ExecuteZoomInClick));
        private void ExecuteZoomInClick()
        {
            zoom += 0.05;

            Height = (int)realHeight * zoom;
            Width = (int)realWidth * zoom;
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

                    if (cacheProvider?.LoadedFilePath == filePath)
                    {
                        MessageBox.Show("FileAlreadyLoadedMsg,Text".Localize(), "AppTitle,Title".Localize());
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

                    Height = realHeight = cacheProvider.videoSequence.Height;
                    Width = realWidth = cacheProvider.videoSequence.Width;
                    Resolution = $"{cacheProvider.videoSequence.Width} x {cacheProvider.videoSequence.Height}";
                    double length = new FileInfo(openFileDialog.FileName).Length / 1024d;
                    FileSize = length < 1000 ? $"{length:0.000} KB" : $"{length / 1024:0.000} MB";

                    MaxSliderValue = cacheProvider.videoSequence.FramesCount - 1;
                    CurrentFrameIndex = 0;
                    StartButtonVisibility = Visibility.Hidden;
                }, "CreateCacheTitle,Title".Localize(), HandleError);
            }
        }

        #endregion Select video

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

        private bool mouseWheelDirection;
        public bool MouseWheelDirection
        {
            get => mouseWheelDirection;
            set
            {
                SetProperty(ref mouseWheelDirection, value);
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

        private DelegateCommand mouseScrolled;
        public DelegateCommand MouseScrolled => mouseScrolled ?? (mouseScrolled = new DelegateCommand(ExecuteMouseScrolled));
        private void ExecuteMouseScrolled()
        {
            if (MouseWheelDirection)
            {
                ExecuteZoomInClick();
            }
            else
            {
                ExecuteZoomOutClick();
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

                PopupContent = cacheProvider.GetUnitDescriptionByLocation(currentFrameIndex, new Point(ScrollViewerX, ScrollViewerY), scrollViewer, zoom);

                var highlight = BitmapFactory.New((int)Width, (int)Height);

                int x1 = PopupContent.Pu.X;
                int y1 = popupContent.Pu.Y;
                int x2 = x1 + PopupContent.Pu.Width;
                int y2 = y1 + PopupContent.Pu.Height;
                highlight.FillRectangle((int)(x1 * zoom), (int)(y1 * zoom), (int)(x2 * zoom), (int)(y2 * zoom), Colors.DeepPink);
                CurrentHighlightImage = highlight;
                HighlightVisibility = Visibility.Visible;
            }
        }

        private DelegateCommand closePopupClick;
        public DelegateCommand ClosePopupClick => closePopupClick ?? (closePopupClick = new DelegateCommand(ExecuteClosePopupClick));
        private void ExecuteClosePopupClick()
        {
            IsPopupOpen = false;
            HighlightVisibility = Visibility.Hidden;
        }

        #endregion
    }
}
