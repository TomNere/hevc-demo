using HEVCDemo.CustomEventArgs;
using HEVCDemo.Helpers;
using HEVCDemo.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private const double zoomStep = 0.05;
        private readonly TimeSpan playerInterval = TimeSpan.FromSeconds(2);
        private readonly Color highlightColor = Colors.DeepPink;
        private bool isPlaying;

        private VideoCache cache;

        private double zoom = 1;
        private bool isVectorsStartEnabled = true;

        #region Binding properties

        private bool isEnabled = false;
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref isEnabled, value);
        }

        private double viewerContentHeight;
        public double ViewerContentHeight
        {
            get => viewerContentHeight;
            set => SetProperty(ref viewerContentHeight, value);
        }

        private double viewerContentWidth;
        public double ViewerContentWidth
        {
            get => viewerContentWidth;
            set => SetProperty(ref viewerContentWidth, value);
        }

        private int maxSliderValue;
        public int MaxSliderValue
        {
            get => maxSliderValue;
            set => SetProperty(ref maxSliderValue, value);
        }

        private BitmapImage currentFrameImage;
        public BitmapImage CurrentFrameImage
        {
            get => currentFrameImage;
            set { SetProperty(ref currentFrameImage, value); }
        }

        private WriteableBitmap currentCodingUnitsImage;
        public WriteableBitmap CurrentCodingUnitsImage
        {
            get => currentCodingUnitsImage;
            set => SetProperty(ref currentCodingUnitsImage, value);
        }

        private WriteableBitmap currentPredictionTypeImage;
        public WriteableBitmap CurrentPredictionTypeImage
        {
            get => currentPredictionTypeImage;
            set => SetProperty(ref currentPredictionTypeImage, value);
        }

        private WriteableBitmap currentIntraPredictionImage;
        public WriteableBitmap CurrentIntraPredictionImage
        {
            get => currentIntraPredictionImage;
            set => SetProperty(ref currentIntraPredictionImage, value);
        }

        private WriteableBitmap currentInterPredictionImage;
        public WriteableBitmap CurrentInterPredictionImage
        {
            get => currentInterPredictionImage;
            set => SetProperty(ref currentInterPredictionImage, value);
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
                _ = SetProperty(ref currentFrameIndex, value);
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
                _ = SetProperty(ref decodedFramesVisibility, value);
                _ = SetCurrentFrameIndex(CurrentFrameIndex);
            }
        }

        private Visibility codingUnitsVisibility = Visibility.Visible;
        public Visibility CodingUnitsVisibility
        {
            get => codingUnitsVisibility;
            set
            {
                _ = SetProperty(ref codingUnitsVisibility, value);
                _ = SetCurrentFrameIndex(CurrentFrameIndex);
            }
        }

        private Visibility predictionTypeVisibility = Visibility.Visible;
        public Visibility PredictionTypeVisibility
        {
            get => predictionTypeVisibility;
            set
            {
                _ = SetProperty(ref predictionTypeVisibility, value);
                _ = SetCurrentFrameIndex(CurrentFrameIndex);
            }
        }

        private Visibility intraPredictionVisibility = Visibility.Visible;
        public Visibility IntraPredictionVisibility
        {
            get => intraPredictionVisibility;
            set
            {
                _ = SetProperty(ref intraPredictionVisibility, value);
                _ = SetCurrentFrameIndex(CurrentFrameIndex);
            }
        }

        private Visibility interPredictionVisibility = Visibility.Visible;
        public Visibility InterPredictionVisibility
        {
            get => interPredictionVisibility;
            set
            {
                _ = SetProperty(ref interPredictionVisibility, value);
                _ = SetCurrentFrameIndex(CurrentFrameIndex);
            }
        }

        private int scrollViewerX;
        public int ScrollViewerX
        {
            get => scrollViewerX;
            set => SetProperty(ref scrollViewerX, value);
        }

        private int scrollViewerY;
        public int ScrollViewerY
        {
            get => scrollViewerY;
            set => SetProperty(ref scrollViewerY, value);
        }

        private bool isMouseWheelPositiveDirection;
        public bool IsMouseWheelPositiveDirection
        {
            get => isMouseWheelPositiveDirection;
            set => SetProperty(ref isMouseWheelPositiveDirection, value);
        }

        private bool isPopupOpen;
        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set => SetProperty(ref isPopupOpen, value);
        }

        private InfoPopupParameters infoPopupParameters;
        public InfoPopupParameters InfoPopupParameters
        {
            get => infoPopupParameters;
            set => SetProperty(ref infoPopupParameters, value);
        }

        #endregion

        #region Commands

        private DelegateCommand stepBackwardCommand;
        public DelegateCommand StepBackwardCommand => stepBackwardCommand ?? (stepBackwardCommand = new DelegateCommand(ExecuteStepBackward, CanExecuteStepBackward));

        private DelegateCommand stepForwardCommand;
        public DelegateCommand StepForwardCommand => stepForwardCommand ?? (stepForwardCommand = new DelegateCommand(ExecuteStepForward, CanExecuteStepForward));

        private DelegateCommand stepStartCommand;
        public DelegateCommand StepStartCommand => stepStartCommand ?? (stepStartCommand = new DelegateCommand(ExecuteStepStart, CanExecuteStepBackward));

        private DelegateCommand stepEndCommand;
        public DelegateCommand StepEndCommand => stepEndCommand ?? (stepEndCommand = new DelegateCommand(ExecuteStepEnd, CanExecuteStepForward));

        private DelegateCommand zoomOutCommand;
        public DelegateCommand ZoomOutCommand => zoomOutCommand ?? (zoomOutCommand = new DelegateCommand(ExecuteZoomOut, CanExecuteZoom));

        private DelegateCommand zoomInCommand;
        public DelegateCommand ZoomInCommand => zoomInCommand ?? (zoomInCommand = new DelegateCommand(ExecuteZoomIn, CanExecuteZoom));

        private DelegateCommand selectVideoCommand;
        public DelegateCommand SelectVideoCommand => selectVideoCommand ?? (selectVideoCommand = new DelegateCommand(ExecuteSelectVideo));

        private DelegateCommand mouseScrolledCommand;
        public DelegateCommand MouseScrolledCommand => mouseScrolledCommand ?? (mouseScrolledCommand = new DelegateCommand(ExecuteMouseScrolled));

        private DelegateCommand<object> scrollViewerRightClickCommand;
        public DelegateCommand<object> ScrollViewerRightClickCommand
            => scrollViewerRightClickCommand ?? (scrollViewerRightClickCommand = new DelegateCommand<object>(ExecuteScrollViewerRightClick));

        private DelegateCommand closePopupCommand;
        public DelegateCommand ClosePopupCommand => closePopupCommand ?? (closePopupCommand = new DelegateCommand(ExecuteClosePopup));

        private DelegateCommand playBackwardCommand;
        public DelegateCommand PlayBackwardCommand => playBackwardCommand ?? (playBackwardCommand = new DelegateCommand(ExecutePlayBackward, CanExecutePlay));

        private DelegateCommand playForwardCommand;
        public DelegateCommand PlayForwardCommand => playForwardCommand ?? (playForwardCommand = new DelegateCommand(ExecutePlayForward, CanExecutePlay));

        private DelegateCommand pauseCommand;
        public DelegateCommand PauseCommand => pauseCommand ?? (pauseCommand = new DelegateCommand(ExecutePause, CanExecutePause));

        #endregion

        public ImagesViewerViewModel()
        {
            BindEventHandlers();
        }

        #region Event handlers

        private void BindEventHandlers()
        {
            GlobalActionsHelper.SelectVideoClicked += SelectVideoClicked;
            GlobalActionsHelper.DecodedFramesVisibilityChanged += DecodedFramesVisibilityChanged;
            GlobalActionsHelper.CodingUnitsVisibilityChanged += CodingUnitsVisibilityChanged;
            GlobalActionsHelper.PredictionTypeVisibilityChanged += PredictionTypeVisibilityChanged;
            GlobalActionsHelper.IntraPredictionVisibilityChanged += IntraPredictionVisibilityChanged;
            GlobalActionsHelper.InterPredictionVisibilityChanged += InterPredictionVisibilityChanged;
            GlobalActionsHelper.VectorsStartVisibilityChanged += VectorsStartVisibilityChanged;
            GlobalActionsHelper.AppStateChanged += AppStateChanged;
            GlobalActionsHelper.MainWindowDeactivated += MainWindowDeactivated;
            GlobalActionsHelper.CacheCleared += CacheCleared;
        }

        private void CacheCleared(object sender, EventArgs e)
        {
            Clear();
            StartButtonVisibility = Visibility.Visible;
        }

        private void MainWindowDeactivated(object sender, EventArgs e)
        {
            ClosePopup();
        }

        private void AppStateChanged(object sender, AppStateChangedEventArgs e)
        {
            if (e.IsViewerEnabled != null)
            {
                IsEnabled = (bool)e.IsViewerEnabled;
            }
        }

        private void SelectVideoClicked(object sender, EventArgs e)
        {
            _ = SelectVideo();
        }

        private void DecodedFramesVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            DecodedFramesVisibility = ConvertBoolToVisibility(e.IsVisible);
        }

        private void CodingUnitsVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            CodingUnitsVisibility = ConvertBoolToVisibility(e.IsVisible);
        }

        private void PredictionTypeVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            PredictionTypeVisibility = ConvertBoolToVisibility(e.IsVisible);
        }

        private void IntraPredictionVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            IntraPredictionVisibility = ConvertBoolToVisibility(e.IsVisible);
        }

        private void InterPredictionVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            InterPredictionVisibility = ConvertBoolToVisibility(e.IsVisible);
        }

        private void VectorsStartVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            isVectorsStartEnabled = e.IsVisible;
            _ = SetCurrentFrameIndex(CurrentFrameIndex);
        }

        #endregion

        #region Backward and Forward

        private void ExecuteStepBackward()
        {
            _ = SetCurrentFrameIndex(CurrentFrameIndex - 1);
        }

        private bool CanExecuteStepBackward()
        {
            return CurrentFrameIndex > 0;
        }

        private void ExecuteStepForward()
        {
            _ = SetCurrentFrameIndex(CurrentFrameIndex + 1);
        }

        private bool CanExecuteStepForward()
        {
            return cache?.VideoSequence.FramesCount > CurrentFrameIndex + 1;
        }

        private void ExecuteStepStart()
        {
            _ = SetCurrentFrameIndex(0);
        }

        private void ExecuteStepEnd()
        {
            _ = SetCurrentFrameIndex(cache.VideoSequence.FramesCount - 1);
        }

        #endregion

        #region Zoom

        private void ExecuteZoomOut()
        {
            if (zoom - zoomStep < 0) return;

            zoom -= zoomStep;

            ViewerContentHeight = cache.VideoSequence.Height * zoom;
            ViewerContentWidth = cache.VideoSequence.Width * zoom;
        }
        
        private void ExecuteZoomIn()
        {
            zoom += zoomStep;

            ViewerContentHeight = cache.VideoSequence.Height * zoom;
            ViewerContentWidth = cache.VideoSequence.Width * zoom;
        }

        private bool CanExecuteZoom()
        {
            return cache?.VideoSequence != null;
        }

        private void ExecuteMouseScrolled()
        {
            if (IsMouseWheelPositiveDirection)
            {
                ExecuteZoomIn();
            }
            else
            {
                ExecuteZoomOut();
            }
        }

        #endregion

        #region Select Video

        private void ExecuteSelectVideo()
        {
            _ = SelectVideo();
        }

        private async Task SelectVideo()
        {
            await FFmpegHelper.EnsureFFmpegIsDownloaded();
            if (!FFmpegHelper.IsFFmpegDownloaded) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "h.265 video file|*.mp4|h.265 annexB binary file|*.bin;*.h265"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                StartButtonVisibility = Visibility.Hidden;

                await OperationsHelper.InvokeSafelyAsync(async () =>
                {
                    GlobalActionsHelper.OnAppStateChanged("OpeningFile,Text".Localize(), false);
                    string filePath = openFileDialog.FileName;

                    if (cache?.LoadedFilePath == filePath)
                    {
                        MessageBox.Show("FileAlreadyLoadedMsg,Text".Localize(), "AppTitle,Title".Localize());
                        GlobalActionsHelper.OnAppStateChanged("ReadyState,Text".Localize(), true);
                        return;
                    }

                    Clear();

                    cache = new VideoCache(filePath);
                    if (cache.CacheExists)
                    {
                        var result = MessageBox.Show("CacheExistsMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            await cache.CreateCache();
                        }
                        else
                        {
                            GlobalActionsHelper.OnAppStateChanged("LoadingDemoData,Text".Localize(), false);
                            cache.ParseProps();
                            await cache.ProcessFiles();
                            cache.CheckFramesCount();
                        }
                    }
                    else
                    {
                        await cache.CreateCache();
                    }

                    await cache.LoadIntoCache(0);

                    zoom = 1;
                    ViewerContentHeight = cache.VideoSequence.Height;
                    ViewerContentWidth = cache.VideoSequence.Width;
                    Resolution = $"{cache.VideoSequence.Width} x {cache.VideoSequence.Height}";
                    FileSize = FormattingHelper.GetFileSize(new FileInfo(openFileDialog.FileName).Length);

                    MaxSliderValue = cache.VideoSequence.FramesCount - 1;
                    await SetCurrentFrameIndex(0);
                    GlobalActionsHelper.OnAppStateChanged("ReadyState,Text".Localize(), true);
                }, "CreatingCache,Text".Localize(), false);
            }
        }

        #endregion Select video

        #region Tooltip popup

        private void ExecuteScrollViewerRightClick(object scrollViewer)
        {
            if ((scrollViewer as ScrollViewer)?.Content is Grid grid)
            {
                // Reset position
                IsPopupOpen = false;

                double horizontalOffset = 0;
                double verticalOffset = 0;

                if (ViewerContentWidth < grid.ActualWidth)
                {
                    horizontalOffset = (grid.ActualWidth - ViewerContentWidth) / 2;
                }

                if (ViewerContentHeight < grid.ActualHeight)
                {
                    verticalOffset = (grid.ActualHeight - ViewerContentHeight) / 2;
                }

                // Get parameters
                InfoPopupParameters = InfoPopupHelper.GetInfo(cache.VideoSequence, currentFrameIndex, new Point(ScrollViewerX , ScrollViewerY), horizontalOffset, verticalOffset, grid, zoom);

                if (infoPopupParameters == null) return;

                IsPopupOpen = true;

                // Highlight unit
                var highlightImage = BitmapFactory.New((int)ViewerContentWidth, (int)ViewerContentHeight);

                int x1 = InfoPopupParameters.Pu.X;
                int y1 = infoPopupParameters.Pu.Y;
                int x2 = x1 + InfoPopupParameters.Pu.Width;
                int y2 = y1 + InfoPopupParameters.Pu.Height;

                highlightImage.FillRectangle((int)(x1 * zoom), (int)(y1 * zoom), (int)(x2 * zoom), (int)(y2 * zoom), highlightColor);
                CurrentHighlightImage = highlightImage;
                HighlightVisibility = Visibility.Visible;
            }
        }

        private void ExecuteClosePopup()
        {
            ClosePopup();   
        }

        private void ClosePopup()
        {
            IsPopupOpen = false;
            HighlightVisibility = Visibility.Hidden;
        }

        #endregion

        #region Player

        private bool CanExecutePlay()
        {
            return cache?.VideoSequence != null && !isPlaying;
        }

        private bool CanExecutePause()
        {
            return isPlaying;
        }

        private void ExecutePlayBackward()
        {
            StartPlayer();

            _ = Task.Run(async () =>
            {
                while (isPlaying)
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;

                    if (CurrentFrameIndex > 0)
                    {
                        await SetCurrentFrameIndex(CurrentFrameIndex - 1);
                        await Task.Delay(playerInterval);
                    }
                    else
                    {
                        StopPlayer();
                    }
                }
            });
        }

        private void ExecutePlayForward()
        {
            StartPlayer();

            _ = Task.Run(async () =>
            {
                while(isPlaying)
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;

                    if (cache?.VideoSequence.FramesCount > CurrentFrameIndex + 1)
                    {
                        await SetCurrentFrameIndex(CurrentFrameIndex + 1);
                        await Task.Delay(playerInterval);
                    }
                    else
                    {
                        StopPlayer();
                    }
                }
            });
        }

        private void ExecutePause()
        {
            StopPlayer();
        }

        private void StartPlayer()
        {
            GlobalActionsHelper.OnAppStateChanged("PlayingState,Text".Localize(), null);
            isPlaying = true;
            RaisePlayerControlsExecuteChanged();
        }

        private void StopPlayer()
        {
            GlobalActionsHelper.OnAppStateChanged("ReadyState,Text".Localize(), null);
            isPlaying = false;
            RaisePlayerControlsExecuteChanged();
        }

        private void RaisePlayerControlsExecuteChanged()
        {
            PlayBackwardCommand.RaiseCanExecuteChanged();
            PlayForwardCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Helpers

        private async Task SetCurrentFrameIndex(int index)
        {
            await SetCurrentFrame(index);
            CurrentFrameIndex = index;
        }

        private async Task SetCurrentFrame(int index)
        {
            ClosePopup();

            if (cache == null) return;

            if (DecodedFramesVisibility == Visibility.Visible)
            {
                CurrentFrameImage = await cache.GetYuvFrame(index, $"{(isPlaying ? "Playing" : "Ready")}State,Text".Localize());
            }
            if (CodingUnitsVisibility == Visibility.Visible)
            {
                CurrentCodingUnitsImage = cache.GetCodingUnitsFrame(index);
            }
            if (PredictionTypeVisibility == Visibility.Visible)
            {
                CurrentPredictionTypeImage = cache.GetPredictionTypeFrame(index);
            }
            if (IntraPredictionVisibility == Visibility.Visible)
            {
                CurrentIntraPredictionImage = cache.GetIntraPredictionFrame(index);
            }
            if (InterPredictionVisibility == Visibility.Visible)
            {
                CurrentInterPredictionImage = cache.GetInterPredictionFrame(index, isVectorsStartEnabled);
            }

            StepForwardCommand.RaiseCanExecuteChanged();
            StepBackwardCommand.RaiseCanExecuteChanged();
            StepStartCommand.RaiseCanExecuteChanged();
            StepEndCommand.RaiseCanExecuteChanged();
            ZoomInCommand.RaiseCanExecuteChanged();
            ZoomOutCommand.RaiseCanExecuteChanged();
            RaisePlayerControlsExecuteChanged();
        }

        private Visibility ConvertBoolToVisibility(bool isVisible)
        {
            return isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void Clear()
        {
            ClosePopup();
            IsEnabled = false;
            CurrentFrameImage = null;
            CurrentCodingUnitsImage = null;
            CurrentPredictionTypeImage = null;
            CurrentIntraPredictionImage = null;
            CurrentInterPredictionImage = null;
            cache = null;
        }

        #endregion
    }
}
