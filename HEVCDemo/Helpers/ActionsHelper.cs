using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public static class ActionsHelper
    {
        public static async Task InvokeSafelyAsync(Func<Task> action, string actionDescription, Action<string, string> errorHandler)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await action();
            }
            catch (Exception e)
            {
                errorHandler(actionDescription, e.Message);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
