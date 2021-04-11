using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintTestFX
{
    static class Util
    {
        // region get key state
        [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
        static extern short _GetAsyncKeyState(Keys vKey);

        /// <summary>
        /// check if a key is currently down
        /// </summary>
        /// <param name="key">the key to check</param>
        /// <returns>is the key down?</returns>
        public static bool IsDown(Keys key)
        {
            return (_GetAsyncKeyState(key) & 0x8000) != 0;
        }

        //endregion

        // region check FG process
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        static extern IntPtr _GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        static extern uint _GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static Process GetForegroundWindowProcess()
        {
            // get foreground process pointer
            IntPtr hWndForeground = _GetForegroundWindow();

            // get PID for that pointer
            _GetWindowThreadProcessId(hWndForeground, out uint fgPid);

            // get process by pid
            return Process.GetProcessById((int)fgPid);
        }
        //endregion
    }
}
