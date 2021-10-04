using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace HEVCDemo.ViewModels
{
    public class InfoDialogViewModel : BindableBase
    {
        private string title;
        public string Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
            }
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                SetProperty(ref text, value);
            }
        }

        private BitmapImage image;
        public BitmapImage Image
        {
            get => image;
            set { SetProperty(ref image, value); }
        }
    }
}
