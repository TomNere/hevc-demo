using HEVCDemo.Helpers;
using HEVCDemo.Types;
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
            InitializeHelpPopup();
            GlobalActionsHelper.MainWindowDeactivated += MainWindowDeactivated;
            GlobalActionsHelper.AppStateChanged += AppStateChanged;
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

        private readonly TimeSpan helpPopupInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan helpPopupTimeout = TimeSpan.FromSeconds(15);
        private readonly TimeSpan helpPopupInitialDelay = TimeSpan.FromSeconds(5);
        private readonly TimeSpan tryLaterDelay = TimeSpan.FromSeconds(30);
        private Timer helpPopupTimer;
        private int helpPopupIndex;
        private List<string> helpPopupTexts;

        private void InitializeHelpPopup()
        {
            helpPopupTimer = new Timer(DoHelpPopupTimerTick, null, helpPopupInitialDelay, helpPopupInterval);
            helpPopupIndex = 0;
            helpPopupTexts = new List<string>
            {
                "PopupHelp1,Content".Localize(),
                "PopupHelp2,Content".Localize(),
                "PopupHelp3,Content".Localize(),
                "PopupHelp4,Content".Localize(),
                "PopupHelp5,Content".Localize(),
                "PopupHelp6,Content".Localize(),
            };
        }

        private async void DoHelpPopupTimerTick(object state)
        {
            if (!WindowHelper.GetApplicationIsActivated())
            {
                // Try later
                helpPopupTimer.Change(tryLaterDelay, helpPopupInterval);
                return;
            }

            HelpPopupText = helpPopupTexts[helpPopupIndex];
            IsHelpPopupOpen = true;
            helpPopupIndex = helpPopupIndex + 1 >= helpPopupTexts.Count ? 0 : helpPopupIndex + 1;

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
        public DelegateCommand CloseHelpPopupCommand => closeHelpPopupCommand ?? (closeHelpPopupCommand = new DelegateCommand(ExecuteCloseHelpPopupClick));
        private void ExecuteCloseHelpPopupClick()
        {
            IsHelpPopupOpen = false;
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
        }

        private void SetAppState(string stateText)
        {
            AppState = stateText;
        }

        #endregion
    }
}
