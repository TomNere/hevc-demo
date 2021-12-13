using HEVCDemo.CustomEventArgs;
using HEVCDemo.Helpers;
using HEVCDemo.Models;
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
        private readonly ViewConfiguration viewConfiguration = new ViewConfiguration();

        public SettingsPanelViewModel()
        {
            GlobalActionsHelper.MainWindowDeactivated += MainWindowDeactivated;
            GlobalActionsHelper.AppStateChanged += AppStateChanged;
            GlobalActionsHelper.ShowTipsEnabledChanged += SetTipsIsEnabledChanged;
            GlobalActionsHelper.VideoLoaded += VideoLoaded;
            GlobalActionsHelper.LanguageChanged += LanguageChanged;
            InitializeTipsTexts();
            InitializeTipsPopup();
            _ = FFmpegHelper.EnsureFFmpegIsDownloaded();

            IsDecodedFrameEnabled = viewConfiguration.IsDecodedFrameVisible;
            IsCodingUnitsEnabled = viewConfiguration.IsCodingPredictionUnitsVisible;
            IsPredictionTypeEnabled = viewConfiguration.IsPredictionTypeVisible;
            IsIntraPredictionEnabled = viewConfiguration.IsIntraPredictionVisible;
            IsInterPredictionEnabled = viewConfiguration.IsInterPredictionVisible;
            IsVectorsStartEnabled = viewConfiguration.IsMotionVectorsStartEnabled;
        }

        #region Event handlers

        // Reinitialize tips popup texts and close tips popup because it has wrong language
        private void LanguageChanged(object sender, EventArgs e)
        {
            InitializeTipsTexts();
            ExecuteCloseTipsPopup();
        }

        // Set resolution and file size
        private void VideoLoaded(object sender, VideoLoadedEventArgs e)
        {
            Resolution = e.Resolution;
            FileSize = e.FileSize;
        }

        // Enable or hide tips popup
        private void SetTipsIsEnabledChanged(object sender, ShowTipsEventArgs e)
        {
            if (e.IsEnabled)
            {
                tipsPopupTimer.Change(TimeSpan.Zero, tipsPopupInterval);
            }
            else
            {
                IsTipsPopupOpen = false;
            }
        }

        private void MainWindowDeactivated(object sender, EventArgs e)
        {
            IsTipsPopupOpen = false;
        }

        private void AppStateChanged(object sender, AppStateChangedEventArgs e)
        {
            SetAppState(e.StateText);
            SettingsEnabled = !e.IsBusy;
        }

        #endregion

        #region Binding properties

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

        private bool settingsEnabled;
        public bool SettingsEnabled
        {
            get => settingsEnabled;
            set => SetProperty(ref settingsEnabled, value);
        }


        private bool isDecodedFrameEnabled = true;
        public bool IsDecodedFrameEnabled
        {
            get => isDecodedFrameEnabled;
            set
            {
                SetProperty(ref isDecodedFrameEnabled, value);
                viewConfiguration.IsDecodedFrameVisible = value;
                GlobalActionsHelper.OnViewConfigurationChanged(viewConfiguration);
            }
        }

        private bool isCodingUnitsEnabled = true;
        public bool IsCodingUnitsEnabled
        {
            get => isCodingUnitsEnabled;
            set
            {
                SetProperty(ref isCodingUnitsEnabled, value);
                viewConfiguration.IsCodingPredictionUnitsVisible = value;
                GlobalActionsHelper.OnViewConfigurationChanged(viewConfiguration);
            }
        }

        private bool isPredictionTypeEnabled = true;
        public bool IsPredictionTypeEnabled
        {
            get => isPredictionTypeEnabled;
            set
            {
                SetProperty(ref isPredictionTypeEnabled, value);
                viewConfiguration.IsPredictionTypeVisible = value;
                GlobalActionsHelper.OnViewConfigurationChanged(viewConfiguration);
            }
        }

        private bool isIntraPredictionEnabled = true;
        public bool IsIntraPredictionEnabled
        {
            get => isIntraPredictionEnabled;
            set
            {
                SetProperty(ref isIntraPredictionEnabled, value);
                viewConfiguration.IsIntraPredictionVisible = value;
                GlobalActionsHelper.OnViewConfigurationChanged(viewConfiguration);
            }
        }

        private bool isInterPredictionEnabled = true;
        public bool IsInterPredictionEnabled
        {
            get => isInterPredictionEnabled;
            set
            {
                SetProperty(ref isInterPredictionEnabled, value);
                viewConfiguration.IsInterPredictionVisible = value;
                GlobalActionsHelper.OnViewConfigurationChanged(viewConfiguration);
            }
        }

        private bool isVectorsStartEnabled = true;
        public bool IsVectorsStartEnabled
        {
            get => isVectorsStartEnabled;
            set
            {
                SetProperty(ref isVectorsStartEnabled, value);
                viewConfiguration.IsMotionVectorsStartEnabled = value;
                GlobalActionsHelper.OnViewConfigurationChanged(viewConfiguration);
            }
        }

        private bool isTipsPopupOpen;
        public bool IsTipsPopupOpen
        {
            get => isTipsPopupOpen;
            set => SetProperty(ref isTipsPopupOpen, value);
        }

        private string tipsPopupText;
        public string TipsPopupText
        {
            get => tipsPopupText;
            set => SetProperty(ref tipsPopupText, value);
        }

        private string appState;
        public string AppState
        {
            get => appState;
            set => SetProperty(ref appState, value);
        }

        #endregion

        #region Commands

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

        private DelegateCommand showWhatIsHevcCommand;
        public DelegateCommand ShowWhatIsHevcCommand => showWhatIsHevcCommand ?? (showWhatIsHevcCommand = new DelegateCommand(ExecuteShowWhatIsHevc));
        private void ExecuteShowWhatIsHevc()
        {
            InfoDialogHelper.ShowWhatIsHevcInfoDialog();
        }

        private DelegateCommand closeTipsPopupCommand;
        public DelegateCommand CloseTipsPopupCommand => closeTipsPopupCommand ?? (closeTipsPopupCommand = new DelegateCommand(ExecuteCloseTipsPopup));
        private void ExecuteCloseTipsPopup()
        {
            IsTipsPopupOpen = false;
        }

        private DelegateCommand showHelpCommand;
        public DelegateCommand ShowHelpCommand => showHelpCommand ?? (showHelpCommand = new DelegateCommand(ExecuteShowHelp));
        private void ExecuteShowHelp()
        {
            var infoDialog = new InfoDialog("HelpHeader,Header".Localize(), "Help", null);
            infoDialog.ShowDialog();
        }

        #endregion

        #region Tips popup

        private readonly TimeSpan tipsPopupInterval = TimeSpan.FromMinutes(3);
        private readonly TimeSpan tipsPopupTimeout = TimeSpan.FromSeconds(15);
        private readonly TimeSpan tipsPopupInitialDelay = TimeSpan.FromSeconds(15);
        private readonly TimeSpan tryLaterDelay = TimeSpan.FromSeconds(30);
        private readonly List<string> tipsPopupTexts = new List<string>();

        private Timer tipsPopupTimer;

        private void InitializeTipsTexts()
        {
            tipsPopupTexts.Clear();
            tipsPopupTexts.AddRange(new List<string>
            {
                "PopupTip1,Content".Localize(),
                "PopupTip2,Content".Localize(),
                "PopupTip3,Content".Localize(),
                "PopupTip4,Content".Localize(),
                "PopupTip5,Content".Localize(),
                "PopupTip6,Content".Localize(),
                "PopupTip7,Content".Localize(),
                "PopupTip8,Content".Localize(),
                "PopupTip9,Content".Localize(),
                "PopupTip10,Content".Localize(),
                "PopupTip11,Content".Localize(),
            });
        }

        private void InitializeTipsPopup()
        {
            tipsPopupTimer = new Timer(DoTipsPopupTimerTick, null, tipsPopupInitialDelay, tipsPopupInterval);
        }

        private async void DoTipsPopupTimerTick(object state)
        {
            if (!Properties.Settings.Default.IsShowTipsEnabled) return;

            if (!WindowHelper.GetApplicationIsActivated())
            {
                // Try later
                tipsPopupTimer.Change(tryLaterDelay, tipsPopupInterval);
                return;
            }

            var index = Properties.Settings.Default.ShowTipsCounter;

            TipsPopupText = tipsPopupTexts[index];
            IsTipsPopupOpen = true;
            Properties.Settings.Default.ShowTipsCounter = index + 1 >= tipsPopupTexts.Count ? 0 : index + 1;

            await Task.Delay(tipsPopupTimeout);
            IsTipsPopupOpen = false;
        }

        #endregion

        private void SetAppState(string stateText)
        {
            AppState = stateText;
        }

    }
}
