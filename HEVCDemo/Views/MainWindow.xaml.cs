using HEVCDemo.Helpers;
using MahApps.Metro.Controls;
using Rasyidf.Localization;
using System.Windows;
using System.Windows.Threading;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Application.Current.DispatcherUnhandledException += HandleUnhandledException;
            Deactivated += GlobalActionsHelper.OnMainWindowDeactivated;
            Closing += HandleClosing;
        }

        private void HandleClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void HandleUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show($"{"UnhandledEx,Text".Localize()}\n\n{e.Exception.Message}");
        }

        private void WindowPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = ImagesViewer.ProcessPreviewKeyDown(e.Key);
        }
    }
}
