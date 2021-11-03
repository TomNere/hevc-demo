using HEVCDemo.Helpers;
using HEVCDemo.Models;
using HEVCDemo.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

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
            AddNavigationCommandBinding();

            var viewModel = new InfoDialogViewModel
            {
                Title = title,
                Text = MarkdownLoader.GetInfoText(textKey),
                Images = images
            };
            DataContext = viewModel;
        }

        // Handles navigation in markdown
        private void AddNavigationCommandBinding()
        {
            CommandBindings.Add(new CommandBinding(
                NavigationCommands.GoToPage,
                    (s, e) =>
                    {
                        var proc = new Process();
                        proc.StartInfo.UseShellExecute = true;
                        proc.StartInfo.FileName = (string)e.Parameter;
                        proc.Start();
                    }));
        }
    }
}
