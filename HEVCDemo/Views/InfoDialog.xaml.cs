using HEVCDemo.Helpers;
using HEVCDemo.ViewModels;
using MahApps.Metro.Controls;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml
    /// </summary>
    public partial class InfoDialog : MetroWindow
    {
        public InfoDialog(string title, string textKey)
        {
            InitializeComponent();
            var viewModel = new InfoDialogViewModel
            {
                Title = title,
                Text = MarkdownLoader.GetInfoText(textKey)
            };
            DataContext = viewModel;
        }
    }
}
