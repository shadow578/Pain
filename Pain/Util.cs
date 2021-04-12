using Pain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PaintTestFX
{
    [Obsolete("Window and Keyboard")]
    static class Util
    {
        /// <summary>
        /// enable command logging to console
        /// </summary>
        public static bool ENABLE_CW { get; set; } = false;

        #region send key 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "keybd_event")]
        static extern void _SendKeyboardEvent(VK bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        /// <summary>
        /// simulate keypresses.
        /// instead of ALT, use MENU
        /// </summary>
        /// <param name="delay">delay between keys, ms</param>
        /// <param name="keys">keys to press</param>
        public static void SendKeys(int delay, params VK[] keys)
        {
            foreach (VK key in keys)
            {
                CW($"SendKey: {key}");
                _SendKeyboardEvent(key, 0, 0, 0);
                Thread.Sleep(delay);
                _SendKeyboardEvent(key, 0, 0x0002, 0);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// types a string using VKeys. Only supports letters, digits, whitespaces, newline, dot and comma.
        /// directly use SendKeys for everything else
        /// </summary>
        /// <param name="delay">delay between keys, ms</param>
        /// <param name="str">the string to type</param>
        public static void SendKeys(int delay, string str)
        {
            List<VK> keys = new List<VK>();
            foreach (char c in str)
            {
                VK key;
                if (char.IsLetter(c))
                {
                    if (!Enum.TryParse(c.ToString(), true, out key))
                        continue;
                }
                else if (char.IsDigit(c))
                {
                    if (!Enum.TryParse("N" + c, true, out key))
                        continue;
                }
                else if (char.IsWhiteSpace(c))
                    key = VK.Space;
                else if (c == ',')
                    key = VK.OEMComma;
                else if (c == '.')
                    key = VK.OEMPeriod;
                else if (c == '\n')
                    key = VK.Return;
                else
                    continue;

                keys.Add(key);
            }

            SendKeys(delay, keys.ToArray());
        }
        #endregion

        #region get key state
        [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
        static extern short _GetAsyncKeyState(VK vKey);

        /// <summary>
        /// check if a key is currently down
        /// </summary>
        /// <param name="key">the key to check</param>
        /// <returns>is the key down?</returns>
        public static bool IsDown(VK key)
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
