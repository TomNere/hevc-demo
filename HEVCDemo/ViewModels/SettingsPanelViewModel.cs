using HEVCDemo.Helpers;
using HEVCDemo.Types;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HEVCDemo.ViewModels
{
    public class SettingsPanelViewModel : BindableBase
    {
        private const string ChevronUp = "ChevronUp";
        private const string ChevronDown = "ChevronDown";

        private readonly GridLength smallSectionHeight = new GridLength(120);
        private readonly GridLength bigSectionHeight = new GridLength(150);
        private readonly GridLength hiddenSectionHeight = new GridLength(0);

        public SettingsPanelViewModel()
        {
            Section1ButtonKind = ChevronUp;
            Section1Height = smallSectionHeight;

            Section2ButtonKind = ChevronUp;
            Section2Height = smallSectionHeight;

            Section3ButtonKind = ChevronDown;
            Section3Height = hiddenSectionHeight;

            Section4ButtonKind = ChevronDown;
            Section4Height = hiddenSectionHeight;

            InitializeHelpPopup();
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
                ViewOptionsHelper.OnDecodedFramesVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isCodingUnitsEnabled = true;
        public bool IsCodingUnitsEnabled
        {
            get => isCodingUnitsEnabled;
            set
            {
                SetProperty(ref isCodingUnitsEnabled, value);
                ViewOptionsHelper.OnCodingUnitsVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isPredictionTypeEnabled = true;
        public bool IsPredictionTypeEnabled
        {
            get => isPredictionTypeEnabled;
            set
            {
                SetProperty(ref isPredictionTypeEnabled, value);
                ViewOptionsHelper.OnPredictionTypeVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isIntraPredictionEnabled = true;
        public bool IsIntraPredictionEnabled
        {
            get => isIntraPredictionEnabled;
            set
            {
                SetProperty(ref isIntraPredictionEnabled, value);
                ViewOptionsHelper.OnIntraPredictionVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isInterPredictionEnabled = true;
        public bool IsInterPredictionEnabled
        {
            get => isInterPredictionEnabled;
            set
            {
                SetProperty(ref isInterPredictionEnabled, value);
                ViewOptionsHelper.OnInterPredictionVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        private bool isVectorsStartEnabled = true;
        public bool IsVectorsStartEnabled
        {
            get => isVectorsStartEnabled;
            set
            {
                SetProperty(ref isVectorsStartEnabled, value);
                ViewOptionsHelper.OnVectorsStartVisibilityChanged(new VisibilityChangedEventArgs { IsVisible = value });
            }
        }

        #endregion

        #region Section 1

        private DelegateCommand showHideSection1Command;
        public DelegateCommand ShowHideSection1Command => showHideSection1Command ?? (showHideSection1Command = new DelegateCommand(ExecuteShowHideSection1));
        private void ExecuteShowHideSection1()
        {
            bool isHidden = Section1Height == hiddenSectionHeight;
            Section1Height = isHidden ? smallSectionHeight : hiddenSectionHeight;
            Section1ButtonKind = isHidden ? ChevronUp : ChevronDown;
        }

        private GridLength section1Height;
        public GridLength Section1Height
        {
            get => section1Height;
            set => SetProperty(ref section1Height, value);
        }

        private string section1ButtonKind;
        public string Section1ButtonKind
        {
            get => section1ButtonKind;
            set => SetProperty(ref section1ButtonKind, value);
        }

        #endregion

        #region Section 2

        private DelegateCommand showHideSection2Command;
        public DelegateCommand ShowHideSection2Command => showHideSection2Command ?? (showHideSection2Command = new DelegateCommand(ExecuteShowHideSection2));
        private void ExecuteShowHideSection2()
        {
            bool isHidden = Section2Height == hiddenSectionHeight;
            Section2Height = isHidden ? smallSectionHeight : hiddenSectionHeight;
            Section2ButtonKind = isHidden ? ChevronUp : ChevronDown;
        }

        private GridLength section2Height;
        public GridLength Section2Height
        {
            get => section2Height;
            set => SetProperty(ref section2Height, value);
        }

        private string section2ButtonKind;
        public string Section2ButtonKind
        {
            get => section2ButtonKind;
            set => SetProperty(ref section2ButtonKind, value);
        }

        #endregion

        #region Section 3

        private DelegateCommand showHideSection3Command;
        public DelegateCommand ShowHideSection3Command => showHideSection3Command ?? (showHideSection3Command = new DelegateCommand(ExecuteShowHideSection3));
        private void ExecuteShowHideSection3()
        {
            bool isHidden = Section3Height == hiddenSectionHeight;
            Section3Height = isHidden ? smallSectionHeight : hiddenSectionHeight;
            Section3ButtonKind = isHidden ? ChevronUp : ChevronDown;
        }

        private GridLength section3Height;
        public GridLength Section3Height
        {
            get => section3Height;
            set => SetProperty(ref section3Height, value);
        }

        private string section3ButtonKind;
        public string Section3ButtonKind
        {
            get => section3ButtonKind;
            set => SetProperty(ref section3ButtonKind, value);
        }

        #endregion

        #region Section 4

        private DelegateCommand showHideSection4Command;
        public DelegateCommand ShowHideSection4Command => showHideSection4Command ?? (showHideSection4Command = new DelegateCommand(ExecuteShowHideSection4));
        private void ExecuteShowHideSection4()
        {
            bool isHidden = Section4Height == hiddenSectionHeight;
            Section4Height = isHidden ? bigSectionHeight : hiddenSectionHeight;
            Section4ButtonKind = isHidden ? ChevronUp : ChevronDown;
        }

        private GridLength section4Height;
        public GridLength Section4Height
        {
            get => section4Height;
            set => SetProperty(ref section4Height, value);
        }

        private string section4ButtonKind;
        public string Section4ButtonKind
        {
            get => section4ButtonKind;
            set => SetProperty(ref section4ButtonKind, value);
        }

        #endregion

        #region HelpPopup

        private readonly TimeSpan helpPopupInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan helpPopupTimeout = TimeSpan.FromSeconds(20);
        private readonly TimeSpan helpPopupInitialDelay = TimeSpan.FromSeconds(5);
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

        #endregion
    }
}
