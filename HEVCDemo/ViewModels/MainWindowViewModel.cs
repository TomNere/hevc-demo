using Prism.Commands;
using Prism.Mvvm;
using Rasyidf.Localization;
using System.Collections.ObjectModel;
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

        public MainWindowViewModel()
        {
            PopulateLanguageComboBox();
        }

        private void PopulateLanguageComboBox()
        {
            LocalizationService.ScanLanguagesInFolder("Assets");
            var packs = LocalizationService.RegisteredPacks;
            var cultures = packs.Keys;
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
    }
}
