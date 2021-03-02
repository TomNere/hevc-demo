using Prism.Mvvm;

namespace HEVCDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "HEVC Demo app";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
