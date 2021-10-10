using HEVCDemo.Helpers;
using HEVCDemo.Types;
using HEVCDemo.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.Generic;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml
    /// </summary>
    public partial class InfoDialog : MetroWindow
    {
        public InfoDialog(string title, string textKey, List<InfoImage> images)
        {
            InitializeComponent();
            var viewModel = new InfoDialogViewModel
            {
                Title = title,
                Text = MarkdownLoader.GetInfoText(textKey),
                Images = images
            };
            DataContext = viewModel;
        }
    }
}
