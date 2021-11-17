using HEVCDemo.CustomEventArgs;
using HEVCDemo.Helpers;
using HEVCDemo.Models;
using HEVCDemo.Views;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.ViewModels
{
    public class ImagesViewerViewModel : BindableBase
    {
        private const double zoomStep = 0.05;
        private readonly TimeSpan playerInterval = TimeSpan.FromSeconds(1);
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

        private HevcBitmaps currentFrame;
        public HevcBitmaps CurrentFrame
        {
            get => currentFrame;
            set => SetProperty(ref currentFrame, value);
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

        private Visibility spinnerVisibility = Visibility.Visible;
        public Visibility SpinnerVisibility
        {
            get => spinnerVisibility;
            set => SetProperty(ref spinnerVisibility, value);
        }

        private Visibility selectVideoVisibility = Visibility.Visible;
        public Visibility SelectVideoVisibility
        {
            get => selectVideoVisibility;
            set => SetProperty(ref selectVideoVisibility, value);
        }

        private Visibility highlightVisibility = Visibility.Hidden;
        public Visibility HighlightVisibility
        {
            get => highlightVisibility;
            set => SetProperty(ref highlightVisibility, value);
        }

        private Visibility decodedFramesVisibility = Visibility.Visible;
        public Visibility DecodedFramesVisibility
        {
            get => decodedFramesVisibility;
            set => _ = SetProperty(ref decodedFramesVisibility, value);
        }

        private Visibility codingUnitsVisibility = Visibility.Visible;
        public Visibility CodingUnitsVisibility
        {
            get => codingUnitsVisibility;
            set => _ = SetProperty(ref codingUnitsVisibility, value);
        }

        private Visibility predictionTypeVisibility = Visibility.Visible;
        public Visibility PredictionTypeVisibility
        {
            get => predictionTypeVisibility;
            set => _ = SetProperty(ref predictionTypeVisibility, value);
        }

        private Visibility intraPredictionVisibility = Visibility.Visible;
        public Visibility IntraPredictionVisibility
        {
            get => intraPredictionVisibility;
            set => _ = SetProperty(ref intraPredictionVisibility, value);
        }

        private Visibility interPredictionVisibility = Visibility.Visible;
        public Visibility InterPredictionVisibility
        {
            get => interPredictionVisibility;
            set => _ = SetProperty(ref interPredictionVisibility, value);
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
            GlobalActionsHelper.KeyDown += KeyDown;
        }

        private void KeyDown(object sender, KeyDownEventArgs e)
        {
            var key = e.Key;
            if (key == Key.Left)
            {
                if (StepBackwardCommand.CanExecute())
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        StepStartCommand.Execute();
                    }
                    else
                    {
                        StepBackwardCommand.Execute();
                    }
                }
            }
            else if (key == Key.Right)
            {
                if (StepForwardCommand.CanExecute())
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        StepEndCommand.Execute();
                    }
                    else
                    {
                        StepForwardCommand.Execute();
                    }
                }
            }
            else if (key == Key.Space)
            {
                if (!isPlaying)
                {
                    PlayForwardCommand.Execute();
                }
                else
                {
                    PauseCommand.Execute();
                }
            }
            else if (key == Key.Add)
            {
                if (ZoomInCommand.CanExecute())
                {
                    ZoomInCommand.Execute();
                }
            }
            else if (key == Key.Subtract)
            {
                if (ZoomOutCommand.CanExecute())
                {
                    ZoomOutCommand.Execute();
                }
            }
        }

        private void CacheCleared(object sender, EventArgs e)
        {
            Clear();
            SelectVideoVisibility = Visibility.Visible;
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

            SpinnerVisibility = e.IsBusy ? Visibility.Visible : Visibility.Hidden;
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
            if (cache != null)
            {
                cache.ClearPrecachedHevcBitmaps();
                _ = SetCurrentFrame(CurrentFrameIndex);
            }
        }

        #endregion

        #region Backward and Forward

        private void ExecuteStepBackward()
        {
            CurrentFrameIndex--;
        }

        private bool CanExecuteStepBackward()
        {
            return CurrentFrameIndex > 0;
        }

        private void ExecuteStepForward()
        {
            CurrentFrameIndex++;
        }

        private bool CanExecuteStepForward()
        {
            return cache?.VideoSequence.FramesCount > CurrentFrameIndex + 1;
        }

        private void ExecuteStepStart()
        {
            CurrentFrameIndex = 0;
        }

        private void ExecuteStepEnd()
        {
            CurrentFrameIndex = cache.VideoSequence.FramesCount - 1;
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
                SelectVideoVisibility = Visibility.Hidden;

                await OperationsHelper.InvokeSafelyAsync(async () =>
                {
                    string filePath = openFileDialog.FileName;

                    if (cache?.LoadedFilePath == filePath)
                    {
                        MessageBox.Show("FileAlreadyLoadedMsg,Text".Localize(), "AppTitle,Title".Localize());
                        GlobalActionsHelper.OnAppStateChanged("ReadyState,Text".Localize(), true, false);
                        return;
                    }

                    Clear();

                    var cacheToCreate = new VideoCache(filePath);
                    if (cacheToCreate.CacheExists)
                    {
                        var result = MessageBox.Show("CacheExistsMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            if (!await CreateCache(cacheToCreate))
                            {
                                return;
                            }
                        }
                        else
                        {
                            GlobalActionsHelper.OnAppStateChanged("LoadingDemoData,Text".Localize(), false, true);
                            cacheToCreate.ParseProps();
                            await cacheToCreate.ProcessFiles();
                            cacheToCreate.InitializeYuvFramesFiles();
                        }
                    }
                    else if (!await CreateCache(cacheToCreate))
                    {
                        return;
                    }

                    cache = cacheToCreate;
                    zoom = 1;
                    ViewerContentHeight = cache.VideoSequence.Height;
                    ViewerContentWidth = cache.VideoSequence.Width;
                    string resolution = $"{cache.VideoSequence.Width} x {cache.VideoSequence.Height}";
                    string fileSize = FormattingHelper.GetFileSize(new FileInfo(openFileDialog.FileName).Length);
                    GlobalActionsHelper.OnVideoLoaded(resolution, fileSize);

                    MaxSliderValue = cache.VideoSequence.FramesCount - 1;
                    CurrentFrameIndex = 0;
                },
                "CreatingCache,Text".Localize(),
                false,
                "OpeningFile,Text".Localize(),
                "ReadyState,Text".Localize()
                );
            }
        }

        private async Task<bool> CreateCache(VideoCache cache)
        {
            if (cache.IsMp4)
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);

                var duration = await FFmpegHelper.GetDuration(cache.LoadedFilePath);
                var dialog = new SelectRangeDialog((int)duration.TotalSeconds);
                var dialogResult = dialog.ShowDialog();

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

                if (dialogResult ?? false)
                {
                    await cache.CreateCache(dialog.StartSecond, dialog.EndSecond);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("CantCrop,Text".Localize(), "AppTitle,Title".Localize());
                await cache.CreateCache(0, 0);
                return true;
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
                        CurrentFrameIndex--;
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
                        CurrentFrameIndex++;
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
            GlobalActionsHelper.OnAppStateChanged("PlayingState,Text".Localize(), null, false);
            isPlaying = true;
            RaisePlayerControlsExecuteChanged();
        }

        private void StopPlayer()
        {
            GlobalActionsHelper.OnAppStateChanged("ReadyState,Text".Localize(), null, false);
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

        private async Task SetCurrentFrame(int index)
        {
            ClosePopup();

            if (cache == null) return;

            if (DecodedFramesVisibility == Visibility.Visible)
            {
                CurrentFrame = await cache.GetFrameBitmaps(index, $"{(isPlaying ? "Playing" : "Ready")}State,Text".Localize(), isVectorsStartEnabled);
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
            CurrentFrame = null;
            cache = null;
        }

        #endregion
    }
}
