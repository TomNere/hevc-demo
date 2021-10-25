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
