using HEVCDemo.Helpers;
using HEVCDemo.Models;
using HEVCDemo.Views;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HEVCDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private List<LocalizationDictionary> culturePacks;

        #region Binding properties

        private bool isTerminalEnabled;
        public bool IsTerminalEnabled
        {
            get => isTerminalEnabled;
            set
            {
                SetProperty(ref isTerminalEnabled, value);
                Properties.Settings.Default.IsTerminalEnabled = value;
            }
        }

        private bool isShowTipsEnabled;
        public bool IsShowTipsEnabled
        {
            get => isShowTipsEnabled;
            set
            {
                SetProperty(ref isShowTipsEnabled, value);
                Properties.Settings.Default.IsShowTipsEnabled = value;
                GlobalActionsHelper.OnShowTipsEnabledChanged(value);
            }
        }

        private ObservableCollection<MenuItem> cultureMenuItems = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> CultureMenuItems
        {
            get => cultureMenuItems; 
            set => SetProperty(ref cultureMenuItems, value);
        }

        private LocalizationDictionary selectedPack;
        public LocalizationDictionary SelectedPack
        {
            get => selectedPack;
            set
            {
                SetProperty(ref selectedPack, value);
                LocalizationService.Current.ChangeLanguage(value);
            }
        }

        #endregion

        #region Commands

        private DelegateCommand<LocalizationDictionary> changeLanguageCommand;
        public DelegateCommand<LocalizationDictionary> ChangeLanguageCommand => changeLanguageCommand ?? (changeLanguageCommand = new DelegateCommand<LocalizationDictionary>(OnChangeLanguage));

        private DelegateCommand showHelpCommand;
        public DelegateCommand ShowHelpCommand => showHelpCommand ?? (showHelpCommand = new DelegateCommand(ExecuteShowHelp));

        private DelegateCommand showLicensesCommand;
        public DelegateCommand ShowLicensesCommand => showLicensesCommand ?? (showLicensesCommand = new DelegateCommand(ExecuteShowLicenses));

        private DelegateCommand showAboutCommand;
        public DelegateCommand ShowAboutCommand => showAboutCommand ?? (showAboutCommand = new DelegateCommand(ExecuteShowAbout));

        private DelegateCommand selectVideoCommand;
        public DelegateCommand SelectVideoCommand => selectVideoCommand ?? (selectVideoCommand = new DelegateCommand(ExecuteSelectVideo));

        private DelegateCommand exitCommand;
        public DelegateCommand ExitCommand => exitCommand ?? (exitCommand = new DelegateCommand(ExecuteExit));

        private DelegateCommand clearCacheCommand;
        public DelegateCommand ClearCacheCommand => clearCacheCommand ?? (clearCacheCommand = new DelegateCommand(ExecuteClearCache));

        #endregion

        public MainWindowViewModel()
        {
            InitializeLanguages();
            IsShowTipsEnabled = Properties.Settings.Default.IsShowTipsEnabled;
            IsTerminalEnabled = Properties.Settings.Default.IsTerminalEnabled;
        }

        private void ExecuteClearCache()
        {
            var result = MessageBox.Show("ClearCacheMsg,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _ = VideoCache.ClearCache();
            }
        }

        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }

        private void ExecuteSelectVideo()
        {
            GlobalActionsHelper.OnSelectVideoClicked();
        }

        private void ExecuteShowHelp()
        {
            var infoDialog = new InfoDialog("HelpHeader,Header".Localize(), "Help", null);
            infoDialog.ShowDialog();
        }

        private void ExecuteShowLicenses()
        {
            var infoDialog = new InfoDialog("LicensesHeader,Header".Localize(), "Licenses", null);
            infoDialog.ShowDialog();
        }

        private void ExecuteShowAbout()
        {
            var infoDialog = new InfoDialog("AppTitle,Title".Localize(), "About", null);
            infoDialog.ShowDialog();
        }

        private void InitializeLanguages()
        {
            LocalizationService.ScanLanguagesInFolder("Assets\\Translations");
            var packs = LocalizationService.RegisteredPacks;
            var packsKeys = packs.Keys;

            if (string.IsNullOrEmpty(Properties.Settings.Default.Language))
            {
                Properties.Settings.Default.Language = LocalizationDictionary.GetResources(packsKeys.First()).Culture.Name;
            }

            culturePacks = new List<LocalizationDictionary>();
            foreach (var pack in packsKeys)
            {
                var culturePack = LocalizationDictionary.GetResources(pack);
                culturePacks.Add(culturePack);
                if (culturePack.Culture.Name == Properties.Settings.Default.Language)
                {
                    LocalizationService.Current.ChangeLanguage(culturePack);
                }
            }

            CreateCultureMenuItems();
        }

        private void CreateCultureMenuItems()
        {
            CultureMenuItems.Clear();

            foreach (var pack in culturePacks)
            {
                var menuItem = new MenuItem
                {
                    Header = $"{pack.EnglishName} ({pack.CultureName})",
                    Tag = pack,
                    IsChecked = Properties.Settings.Default.Language == pack.Culture.Name,
                    IsEnabled = true
                };

                CultureMenuItems.Add(menuItem);
            }
        }

        private void OnChangeLanguage(LocalizationDictionary value)
        {
            if (value != null)
            {
                Properties.Settings.Default.Language = value.Culture.Name;
                LocalizationService.Current.ChangeLanguage(value);
                CreateCultureMenuItems();
                GlobalActionsHelper.OnLanguageChanged();
            }
        }
    }
}
