using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public static class OperationsHelper
    {
        public static async Task InvokeSafelyAsync(Action action, string actionDescription, bool allowEnableViewer, string stateBefore, string stateAfter)
        {
            await InvokeSafelyAsync(async() => await Task.Run(action), actionDescription, allowEnableViewer, stateBefore, stateAfter);
        }

        public static async Task InvokeSafelyAsync(Func<Task> action, string actionDescription, bool allowEnableViewer, string stateBefore, string stateAfter)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
                GlobalActionsHelper.OnAppStateChanged(stateBefore, false, true);
                await action();
            }
            catch (Exception e)
            {
                GlobalActionsHelper.OnAppStateChanged("ErrorOccuredState,Text".Localize(), allowEnableViewer ? true : (bool?)null, false);
                MessageBox.Show($"{"ErrorOccuredTitle,Title".Localize()}\n\n{e.Message}", actionDescription);
            }
            finally
            {
                GlobalActionsHelper.OnAppStateChanged(stateAfter, true, false);
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);
            }
        }
    }
}
