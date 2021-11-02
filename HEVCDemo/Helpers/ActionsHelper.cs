using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public static class ActionsHelper
    {
        public static async Task InvokeSafelyAsync(Action action, string actionDescription, bool allowEnableViewer)
        {
            await InvokeSafelyAsync(async() => await Task.Run(action), actionDescription, allowEnableViewer);
        }

        public static async Task InvokeSafelyAsync(Func<Task> action, string actionDescription, bool allowEnableViewer)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
                await action();
            }
            catch (Exception e)
            {
                GlobalActionsHelper.OnAppStateChanged("ErrorOccuredState,Text".Localize(), allowEnableViewer ? true : (bool?)null);
                MessageBox.Show($"{"ErrorOccuredTitle,Title".Localize()}\n\n{e.Message}", actionDescription);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);
            }
        }
    }
}
