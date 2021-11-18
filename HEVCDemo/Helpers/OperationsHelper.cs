using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public static class OperationsHelper
    {
        public static async Task<bool> InvokeSafelyAsync(Action action, string actionDescription, bool allowEnableViewer, string stateBefore, string stateAfter)
        {
            return await InvokeSafelyAsync(async() => await Task.Run(action), actionDescription, allowEnableViewer, stateBefore, stateAfter);
        }

        public static async Task<bool> InvokeSafelyAsync(Func<Task> action, string actionDescription, bool allowEnableViewer, string stateBefore, string stateAfter)
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
                MessageBox.Show($"{"ErrorOccuredTitle,Title".Localize()} - {actionDescription}\n\n{e.Message}", "AppTitle,Title".Localize());
                return false;
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);
            }

            GlobalActionsHelper.OnAppStateChanged(stateAfter, true, false);
            return true;
        }
    }
}
