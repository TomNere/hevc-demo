using HEVCDemo.ViewModels;
using MahApps.Metro.Controls;
using Rasyidf.Localization;
using System.Windows;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for SelectRangeDialog.xaml
    /// </summary>
    public partial class SelectRangeDialog : MetroWindow
    {
        private const int maxRange = 10;

        private readonly SelectRangeDialogViewModel viewModel;
        public int StartSecond { get; set; }
        public int EndSecond { get; set; }

        public SelectRangeDialog(int maximum)
        {
            InitializeComponent();
            viewModel = new SelectRangeDialogViewModel(maximum);
            DataContext = viewModel;
        }

        // For simplicity, confirm is handled in code behind
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            StartSecond = viewModel.StartSecond;
            EndSecond = viewModel.EndSecond;

            if (EndSecond - StartSecond < 1 || EndSecond - StartSecond > maxRange)
            {
                MessageBox.Show("SelectRange,Content".Localize(), "AppTitle,Title".Localize());
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
