using HEVCDemo.Helpers;
using HEVCDemo.Models;
using HEVCDemo.Views;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HEVCDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
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

        private ObservableCollection<LocalizationDictionary> cultures = new ObservableCollection<LocalizationDictionary>();
        public ObservableCollection<LocalizationDictionary> Cultures
        {
            get => cultures;
            set
            {
                SetProperty(ref cultures, value);
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

        private DelegateCommand<LocalizationDictionary> changeLanguageCommand;
        public DelegateCommand<LocalizationDictionary> ChangeLanguageCommand
            => changeLanguageCommand ?? (changeLanguageCommand = new DelegateCommand<LocalizationDictionary>(OnChangeLanguage));

        private DelegateCommand showHelpCommand;
        public DelegateCommand ShowHelpCommand
            => showHelpCommand ?? (showHelpCommand = new DelegateCommand(ExecuteShowHelp));

        private DelegateCommand showLicensesCommand;
        public DelegateCommand ShowLicensesCommand
            => showLicensesCommand ?? (showLicensesCommand = new DelegateCommand(ExecuteShowLicenses));

        private DelegateCommand showAboutCommand;
        public DelegateCommand ShowAboutCommand
            => showAboutCommand ?? (showAboutCommand = new DelegateCommand(ExecuteShowAbout));

        private void ExecuteShowHelp()
        {
            var infoDialog = new InfoDialog("HelpHeader,Header".Localize(), "Help", null);
            infoDialog.Show();
        }

        private void ExecuteShowLicenses()
        {
            var infoDialog = new InfoDialog("LicensesHeader,Header".Localize(), "Licenses", null);
            infoDialog.Show();
        }

        private void ExecuteShowAbout()
        {
            var infoDialog = new InfoDialog("AppTitle,Title".Localize(), "About", null);
            infoDialog.Show();
        }

        public MainWindowViewModel()
        {
            InitializeLanguages();
            IsShowTipsEnabled = Properties.Settings.Default.IsShowTipsEnabled;
            IsTerminalEnabled = Properties.Settings.Default.IsTerminalEnabled;
        }

        private void InitializeLanguages()
        {
            LocalizationService.ScanLanguagesInFolder("Assets\\Translations");
            var packs = LocalizationService.RegisteredPacks;
            var cultures = packs.Keys;
            LocalizationService.Current.ChangeLanguage(LocalizationDictionary.GetResources(cultures.First()));
            foreach (var culture in cultures)
            {
                var pack = LocalizationDictionary.GetResources(culture);
                Cultures.Add(pack);

                var menuItem = new MenuItem
                {
                    Header = $"{pack.EnglishName} ({pack.CultureName})",
                    Tag = pack,
                    IsChecked = cultures.First() == culture, // TODO,
                    IsEnabled = cultures.First() == culture // TODO,
                };

                CultureMenuItems.Add(menuItem);
            }
        }

        private void OnChangeLanguage(LocalizationDictionary value)
        {
            if (value != null)
            {
                LocalizationService.Current.ChangeLanguage(value);
            }
        }

        private DelegateCommand selectVideoCommand;
        public DelegateCommand SelectVideoCommand => selectVideoCommand ?? (selectVideoCommand = new DelegateCommand(ExecuteSelectVideo));
        private void ExecuteSelectVideo()
        {
            GlobalActionsHelper.OnSelectVideoClicked();
        }

        private DelegateCommand exitCommand;
        public DelegateCommand ExitCommand => exitCommand ?? (exitCommand = new DelegateCommand(ExecuteExit));
        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }

        private DelegateCommand clearCacheCommand;
        public DelegateCommand ClearCacheCommand => clearCacheCommand ?? (clearCacheCommand = new DelegateCommand(ExecuteClearCache));
        private void ExecuteClearCache()
        {
            var result = MessageBox.Show("ClearCacheMessage,Text".Localize(), "AppTitle,Title".Localize(), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _ = VideoCache.ClearCache();
            }
        }
    }
}
