﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HEVCDemo.Helpers
{
    public static class WindowHelper
    {
        // Returns true if the current application has focus, false otherwise
        public static bool GetApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) return false;

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}
