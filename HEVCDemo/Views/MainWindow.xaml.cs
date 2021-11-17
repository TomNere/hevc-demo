using HEVCDemo.Helpers;
using MahApps.Metro.Controls;
using Rasyidf.Localization;
using System;
using System.Windows;
using System.Windows.Input;
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
            GlobalActionsHelper.VideoLoaded += VideoLoaded;
            Closing += HandleClosing;
        }

        /// <summary>
        /// VideoLoaded handler - focus main window to ensure shortcuts functionality
        /// </summary>
        private void VideoLoaded(object sender, CustomEventArgs.VideoLoadedEventArgs e)
        {
            Focus();
        }

        private void HandleClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void HandleUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            if (e.Exception is OutOfMemoryException || e.Exception is InsufficientMemoryException)
            {
                MessageBox.Show($"{"InsufficientMemory,Text".Localize()}\n\n{e.Exception.Message}", "AppTitle,Title".Localize());
            }
            else
            {
                MessageBox.Show($"{"UnhandledEx,Text".Localize()}\n\n{e.Exception.Message}", "AppTitle,Title".Localize());
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            GlobalActionsHelper.OnKeyDown(key);
            e.Handled = key == Key.Left ||
                        key == Key.Right ||
                        key == Key.Space ||
                        key == Key.Add ||
                        key == Key.Subtract;
        }
    }
}
