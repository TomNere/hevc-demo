using HEVCDemo.Types;
using Prism.Mvvm;
using System.Collections.Generic;

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

        private List<InfoImage> images;
        public List<InfoImage> Images
        {
            get => images;
            set { SetProperty(ref images, value); }
        }
    }
}
