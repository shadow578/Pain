using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PaintTestFX
{
    static class Util
    {
        /// <summary>
        /// enable command logging to console
        /// </summary>
        public static bool ENABLE_CW { get; set; } = false;

        #region send key 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "keybd_event")]
        static extern void _SendKeyboardEvent(Keys bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        /// <summary>
        /// simulate keypresses.
        /// instead of ALT, use MENU
        /// </summary>
        /// <param name="delay">delay between keys, ms</param>
        /// <param name="keys">keys to press</param>
        public static void SendKeys(int delay, params Keys[] keys)
        {
            foreach(Keys key in keys)
            {
                CW($"SendKey: {key}");
                _SendKeyboardEvent(key, 0, 0, 0);
                Thread.Sleep(delay);
                _SendKeyboardEvent(key, 0, 0x0002, 0);
                Thread.Sleep(delay);
            }
        }
        #endregion

        #region get key state
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

        #endregion

        #region check FG process
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
        #endregion

        /// <summary>
        /// console write
        /// </summary>
        /// <param name="msg">message to write</param>
        static void CW(string msg)
        {
            if (ENABLE_CW)
                Console.WriteLine(msg);
        }
    }
}
