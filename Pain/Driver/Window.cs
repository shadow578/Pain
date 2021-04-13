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
        /// Force the foreground window to repaint
        /// </summary>
        public static void ForceRepaintForegroundWindow()
        {
            // get foreground process pointer
            IntPtr hWndForeground = LowLevel.GetForegroundWindow();

            // force the repaint
            LowLevel.SendMessage(hWndForeground, LowLevel.WmPaint, 0, 0);
        }

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
            public const int WmPaint = 0x000F;

            [DllImport("User32.dll")]
            public static extern long SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

            [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        }
    }
}
