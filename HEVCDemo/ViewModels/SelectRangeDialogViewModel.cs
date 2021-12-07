using Prism.Mvvm;
using System;

namespace HEVCDemo.ViewModels
{
    public class SelectRangeDialogViewModel : BindableBase
    {
        private int maximum;
        public int Maximum
        {
            get => maximum;
            set => SetProperty(ref maximum, value);
        }

        private int startSecond;
        public int StartSecond
        {
            get => startSecond;
            set => SetProperty(ref startSecond, value);
        }

        private int endSecond;
        public int EndSecond
        {
            get => endSecond;
            set => SetProperty(ref endSecond, value);
        }

        public SelectRangeDialogViewModel(int maximum)
        {
            Maximum = maximum;

            // 3s - default value
            EndSecond = Math.Min(3, Maximum);
        }
    }
}
