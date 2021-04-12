using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pain.Driver
{
    /// <summary>
    /// (foreground) window functions
    /// </summary>
    public static class Window
    {
        /// <summary>
        /// get the process that is the current foreground window
        /// </summary>
        /// <returns>the process of the foreground window</returns>
        public static Process GetForegroundProcess()
        {
            // get foreground process pointer
            IntPtr hWndForeground = LowLevel.GetForegroundWindow();

            // get PID for that pointer
            LowLevel.GetWindowThreadProcessId(hWndForeground, out uint fgPid);

            // get process by pid
            return Process.GetProcessById((int)fgPid);
        }

        /// <summary>
        /// Low Level API Calls
        /// </summary>
        private static class LowLevel
        {
            [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        }
    }
}
