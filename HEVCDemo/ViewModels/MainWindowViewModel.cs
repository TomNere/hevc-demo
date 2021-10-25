using HEVCDemo.Helpers;
using HEVCDemo.Views;
using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace HEVCDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string title = "AppTitle,Title".Localize();
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
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

        private void ExecuteShowHelp()
        {
            var infoDialog = new InfoDialog("HelpHeader,Header".Localize(), "Help", null);
            infoDialog.Show();
        }

        public MainWindowViewModel()
        {
            PopulateLanguageComboBox();
        }

        private void PopulateLanguageComboBox()
        {
            LocalizationService.ScanLanguagesInFolder("Assets\\Translations");
            var packs = LocalizationService.RegisteredPacks;
            var cultures = packs.Keys;
            LocalizationService.Current.ChangeLanguage(LocalizationDictionary.GetResources(cultures.First()));
            foreach (var culture in cultures)
            {
                var pack = LocalizationDictionary.GetResources(culture);
                Cultures.Add(pack);
                CultureMenuItems.Add(new MenuItem() { Header = $"{pack.EnglishName} ({pack.CultureName})", Tag = pack });
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
    }
}
