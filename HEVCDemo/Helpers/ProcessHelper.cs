using System.Diagnostics;
using System.Threading.Tasks;

namespace HEVCDemo.Helpers
{
    public static class ProcessHelper
    {
        public static Task<int> RunProcessAsync(string fileName, string arguments)
        {
            var tcs = new TaskCompletionSource<int>();

            var startInfoArgs = new ProcessStartInfo
            {
                FileName = fileName,     // App to run
                Arguments = arguments,   // Command line arguments
                UseShellExecute = false, // Run directly from exe
                // Show or don't show terminal
                CreateNoWindow = !Properties.Settings.Default.IsTerminalEnabled
            };

            var process = new Process
            {
                StartInfo = startInfoArgs,
                EnableRaisingEvents = true
            };

            process.Exited += (s, e) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
