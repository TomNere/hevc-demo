using HEVCDemo.CustomEventArgs;
using HEVCDemo.Helpers;
using HEVCDemo.Views;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEVCDemo.ViewModels
{
    public class SettingsPanelViewModel : BindableBase
    {
        public SettingsPanelViewModel()
        {
            GlobalActionsHelper.MainWindowDeactivated += MainWindowDeactivated;
            GlobalActionsHelper.AppStateChanged += AppStateChanged;
            GlobalActionsHelper.ShowTipsEnabledChanged += SetTipsIsEnabled;
            GlobalActionsHelper.VideoLoaded += VideoLoaded;
            InitializeHelpPopup();
            _ = FFmpegHelper.EnsureFFmpegIsDownloaded();
        }

        private void VideoLoaded(object sender, VideoLoadedEventArgs e)
        {
            Resolution = e.Resolution;
            FileSize = e.FileSize;
        }

        private void SetTipsIsEnabled(object sender, ShowTipsEventArgs e)
        {
            Properties.Settings.Default.IsShowTipsEnabled = e.IsEnabled;

            if (e.IsEnabled)
            {
                helpPopupTimer.Change(TimeSpan.Zero, helpPopupInterval);
            }
            else
            {
                IsHelpPopupOpen = false;
            }
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

        #region Info dialogs

        private DelegateCommand showResolutionInfoCommand;
        public DelegateCommand ShowResolutionInfoCommand => showResolutionInfoCommand ?? (showResolutionInfoCommand = new DelegateCommand(ExecuteShowResolutionInfo));
        private void ExecuteShowResolutionInfo()
        {
            InfoDialogHelper.ShowResolutionInfoDialog();
        }

        private DelegateCommand showFileSizeInfoCommand;
        public DelegateCommand ShowFileSizeInfoCommand => showFileSizeInfoCommand ?? (showFileSizeInfoCommand = new DelegateCommand(ExecuteShowFileSizeInfo));
        private void ExecuteShowFileSizeInfo()
        {
            InfoDialogHelper.ShowFileSizeInfoDialog();
        }

        private DelegateCommand showDecodedFramesInfoCommand;
        public DelegateCommand ShowDecodedFramesInfoCommand => showDecodedFramesInfoCommand ?? (showDecodedFramesInfoCommand = new DelegateCommand(ExecuteShowDecodedFramesInfo));
        private void ExecuteShowDecodedFramesInfo()
        {
            InfoDialogHelper.ShowDecodedFramesInfoDialog();
        }

        private DelegateCommand showCodingUnitsInfoCommand;
        public DelegateCommand ShowCodingUnitsInfoCommand => showCodingUnitsInfoCommand ?? (showCodingUnitsInfoCommand = new DelegateCommand(ExecuteShowCodingUnitsInfo));
        private void ExecuteShowCodingUnitsInfo()
        {
            InfoDialogHelper.ShowCodingUnitsInfoDialog();
        }

        private DelegateCommand showPredictionTypeInfoCommand;
        public DelegateCommand ShowPredictionTypeInfoCommand => showPredictionTypeInfoCommand ?? (showPredictionTypeInfoCommand = new DelegateCommand(ExecuteShowPredictionTypeInfo));
        private void ExecuteShowPredictionTypeInfo()
        {
            InfoDialogHelper.ShowPredictionTypeInfoDialog();
        }

        private DelegateCommand showIntraPredictionInfoCommand;
        public DelegateCommand ShowIntraPredictionInfoCommand => showIntraPredictionInfoCommand ?? (showIntraPredictionInfoCommand = new DelegateCommand(ExecutShowIntraPredictionInfo));
        private void ExecutShowIntraPredictionInfo()
        {
            InfoDialogHelper.ShowIntraPredictionInfoDialog();
        }

        private DelegateCommand showInterPredictionInfoCommand;
        public DelegateCommand ShowInterPredictionInfoCommand => showInterPredictionInfoCommand ?? (showInterPredictionInfoCommand = new DelegateCommand(ExecuteShowInterPredictionInfo));
        private void ExecuteShowInterPredictionInfo()
        {
            InfoDialogHelper.ShowInterPredictionInfoDialog();
        }

        #endregion

        #region View options

        private bool settingsEnabled;

        public bool SettingsEnabled
        {
            get => settingsEnabled;
            set => SetProperty(ref settingsEnabled, value);
        }


        private bool isDecodedFramesEnabled = true;
        public bool IsDecodedFramesEnabled
        {
            get => isDecodedFramesEnabled;
            set
            {
                SetProperty(ref isDecodedFramesEnabled, value);
                GlobalActionsHelper.OnDecodedFramesVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isCodingUnitsEnabled = true;
        public bool IsCodingUnitsEnabled
        {
            get => isCodingUnitsEnabled;
            set
            {
                SetProperty(ref isCodingUnitsEnabled, value);
                GlobalActionsHelper.OnCodingUnitsVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isPredictionTypeEnabled = true;
        public bool IsPredictionTypeEnabled
        {
            get => isPredictionTypeEnabled;
            set
            {
                SetProperty(ref isPredictionTypeEnabled, value);
                GlobalActionsHelper.OnPredictionTypeVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isIntraPredictionEnabled = true;
        public bool IsIntraPredictionEnabled
        {
            get => isIntraPredictionEnabled;
            set
            {
                SetProperty(ref isIntraPredictionEnabled, value);
                GlobalActionsHelper.OnIntraPredictionVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isInterPredictionEnabled = true;
        public bool IsInterPredictionEnabled
        {
            get => isInterPredictionEnabled;
            set
            {
                SetProperty(ref isInterPredictionEnabled, value);
                GlobalActionsHelper.OnInterPredictionVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isVectorsStartEnabled = true;
        public bool IsVectorsStartEnabled
        {
            get => isVectorsStartEnabled;
            set
            {
                SetProperty(ref isVectorsStartEnabled, value);
                GlobalActionsHelper.OnVectorsStartVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        #endregion

        #region HelpPopup

        private readonly TimeSpan helpPopupInterval = TimeSpan.FromMinutes(3);
        private readonly TimeSpan helpPopupTimeout = TimeSpan.FromSeconds(15);
        private readonly TimeSpan helpPopupInitialDelay = TimeSpan.FromSeconds(15);
        private readonly TimeSpan tryLaterDelay = TimeSpan.FromSeconds(30);
        private Timer helpPopupTimer;
        private List<string> helpPopupTexts;

        private void InitializeHelpPopup()
        {
            helpPopupTimer = new Timer(DoHelpPopupTimerTick, null, helpPopupInitialDelay, helpPopupInterval);
            helpPopupTexts = new List<string>
            {
                "PopupHelp1,Content".Localize(),
                "PopupHelp2,Content".Localize(),
                "PopupHelp3,Content".Localize(),
                "PopupHelp4,Content".Localize(),
                "PopupHelp5,Content".Localize(),
                "PopupHelp6,Content".Localize(),
                "PopupHelp7,Content".Localize(),
                "PopupHelp8,Content".Localize(),
                "PopupHelp9,Content".Localize(),
            };
        }

        private async void DoHelpPopupTimerTick(object state)
        {
            if (!Properties.Settings.Default.IsShowTipsEnabled) return;

            if (!WindowHelper.GetApplicationIsActivated())
            {
                // Try later
                helpPopupTimer.Change(tryLaterDelay, helpPopupInterval);
                return;
            }

            var index = Properties.Settings.Default.ShowTipsCounter;

            HelpPopupText = helpPopupTexts[index];
            IsHelpPopupOpen = true;
            Properties.Settings.Default.ShowTipsCounter = index + 1 >= helpPopupTexts.Count ? 0 : index + 1;

            await Task.Delay(helpPopupTimeout);
            IsHelpPopupOpen = false;
        }

        private bool isHelpPopupOpen;
        public bool IsHelpPopupOpen
        {
            get => isHelpPopupOpen;
            set => SetProperty(ref isHelpPopupOpen, value);
        }

        private string helpPopupText;
        public string HelpPopupText
        {
            get => helpPopupText;
            set => SetProperty(ref helpPopupText, value);
        }

        private DelegateCommand closeHelpPopupCommand;
        public DelegateCommand CloseHelpPopupCommand => closeHelpPopupCommand ?? (closeHelpPopupCommand = new DelegateCommand(ExecuteCloseHelp));
        private void ExecuteCloseHelp()
        {
            IsHelpPopupOpen = false;
        }

        private DelegateCommand showHelpCommand;
        public DelegateCommand ShowHelpCommand => showHelpCommand ?? (showHelpCommand = new DelegateCommand(ExecuteShowHelp));
        private void ExecuteShowHelp()
        {
            var infoDialog = new InfoDialog("HelpHeader,Header".Localize(), "Help", null);
            infoDialog.Show();
        }

        private void MainWindowDeactivated(object sender, EventArgs e)
        {
            IsHelpPopupOpen = false;
        }

        #endregion

        #region App state

        private string appState;
        public string AppState
        {
            get => appState;
            set => SetProperty(ref appState, value);
        }

        private void AppStateChanged(object sender, AppStateChangedEventArgs e)
        {
            SetAppState(e.StateText);
            SettingsEnabled = !e.IsBusy;
        }

        private void SetAppState(string stateText)
        {
            AppState = stateText;
        }

        #endregion
    }
}
