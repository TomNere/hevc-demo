using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public static class ActionsHelper
    {
        public static async Task InvokeSafelyAsync(Func<Task> action, string actionDescription, bool allowEnableViewer)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await action();
            }
            catch (Exception e)
            {
                GlobalActionsHelper.OnAppStateChanged("ErrorOccuredState,Text".Localize(), allowEnableViewer ? true : (bool?) null);
                MessageBox.Show($"{actionDescription}\n\n{"ErrorMessageMsg,Text".Localize()}\n{e.Message}", "ErrorOccuredTitle,Title".Localize());
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
