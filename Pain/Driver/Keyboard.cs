using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pain.Driver
{
    /// <summary>
    /// keyboard functions
    /// </summary>
    public static class Keyboard
    {
        /// <summary>
        /// enable command logging to console
        /// </summary>
        public static bool LogCommands { get; set; } = false;

        /// <summary>
        /// how many milliseconds keys stay pressed
        /// </summary>
        public static int KeyDelay { get; set; } = 50;

        /// <summary>
        /// check if a key is down
        /// </summary>
        /// <param name="key">the key to check</param>
        /// <returns>is the key down</returns>
        public static bool IsDown(VK key)
        {
            return (LowLevel.GetAsyncKeyState(key) & 0x8000) != 0;
        }

        /// <summary>
        /// type strings and virtual keys.
        /// for strings, supports everything that <see cref="Type(string)"/> supports.
        /// all objects other than VK and string are converted using .ToString()
        /// 
        /// Use like so: Type("this is", VK.Enter, "string");
        /// </summary>
        /// <param name="p">the parameters</param>
        public static void Type(params object[] p)
        {
            // every parameter
            foreach (object o in p)
            {
                // strings are handled by type
                if (o is string str)
                    Type(str);
                else if (o is VK key)
                    SendVirtualKey(key);
                else
                    Type(o.ToString());
            }
        }

        /// <summary>
        /// type a string. 
        /// supports letters (upper and lower), digits, whitespaces, newline, dot and comma.
        /// also supports typing of virtual keys by escaping them with %{VK.}, like this: "Hello %{VK.Return} World"
        /// </summary>
        /// <param name="s">the string</param>
        public static void Type(string s)
        {
            // every char
            for (int i = 0; i < s.Length; i++)
            {
                // get current char
                char c = s[i];

                // check for escaped sequence if current char is %
                VK vk;
                if (c == '%')
                {
                    //get escaped end
                    int escEnd = s.IndexOf('}', i);
                    if (escEnd == -1)
                        continue;

                    // parse vk name
                    string escVk = s.Substring(i, escEnd).Trim('{', '}', ' ');
                    if (string.IsNullOrWhiteSpace(escVk)
                        || !Enum.TryParse<VK>(escVk, out vk))
                        continue;
                }
                else
                    if (!CharToVK(c, out vk))
                    continue;

                // type the key
                SendVirtualKey(vk);
            }

        }

        /// <summary>
        /// send multiple single virtual key event
        /// </summary>
        /// <param name="keys">the keys to send</param>
        public static void SendVirtualKey(params VK[] keys)
        {
            foreach (VK key in keys)
                SendVirtualKey(key);
        }

        /// <summary>
        /// send a single virtual key event
        /// </summary>
        /// <param name="key">the key to send</param>
        public static void SendVirtualKey(VK key)
        {
            LOG(key.ToString());
            KeyDown(key);
            Thread.Sleep(KeyDelay);
            KeyUp(key);
        }

        /// <summary>
        /// send a key down event for a key
        /// </summary>
        /// <param name="key">the key to send the event for</param>
        public static void KeyDown(VK key)
        {
            LowLevel.SendKeyboardEvent(key, 0, LowLevel.FLAG_KEY_DOWN, 0);
        }

        /// <summary>
        /// send a key up event for a key
        /// </summary>
        /// <param name="key">the key to send the event for</param>
        public static void KeyUp(VK key)
        {
            LowLevel.SendKeyboardEvent(key, 0, LowLevel.FLAG_KEY_UP, 0);
        }

        /// <summary>
        /// get the virtual key for a char.
        /// Only supports letters, digits, whitespaces, newline, dot and comma.
        /// </summary>
        /// <param name="c">the char to convert</param>
        /// <param name="key">the virtual key code</param>
        /// <returns>was parse ok</returns>
        static bool CharToVK(char c, out VK key)
        {
            key = VK.A;

            if (char.IsLetter(c))
            {
                // parse letter
                if (!Enum.TryParse(c.ToString(), true, out key))
                    return false;

                // add shift if capitalized
                if (char.IsUpper(c))
                    key |= VK.Shift | VK.LeftShift;
            }
            else if (char.IsDigit(c))
            {
                //digits are Nx in the enum
                if (!Enum.TryParse("N" + c, true, out key))
                    return false;
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
                return false;

            return true;
        }

        /// <summary>
        /// log a message
        /// </summary>
        /// <param name="msg">message to write</param>
        static void LOG(string msg)
        {
            if (LogCommands)
                Console.WriteLine("[Keyboard] " + msg);
        }

        /// <summary>
        /// low level API calls
        /// </summary>
        private static class LowLevel
        {
            public const int FLAG_KEY_DOWN = 0;
            public const int FLAG_KEY_UP = 0x0002;

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "keybd_event")]
            public static extern void SendKeyboardEvent(VK bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

            [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
            public static extern short GetAsyncKeyState(VK vKey);
        }
    }
}
