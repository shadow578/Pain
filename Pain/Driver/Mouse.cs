using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Pain.Driver
{
    /// <summary>
    /// mouse input simulation funtions
    /// </summary>
    public static class Mouse
    {
        /// <summary>
        /// enable command logging to console
        /// </summary>
        public static bool LogCommands { get; set; } = false;

        /// <summary>
        /// press the left mouse button. stays pressed until ReleaseLeftButton is called
        /// </summary>
        public static void PressLeftButton()
        {
            SendMouseEvent(LowLevel.MouseEventFlags.LeftDown);
            Log("LMB Down");
        }

        /// <summary>
        /// release the left mouse button
        /// </summary>
        public static void ReleaseLeftButton()
        {
            SendMouseEvent(LowLevel.MouseEventFlags.LeftUp);
            Log("LMB Up");
        }

        /// <summary>
        /// press the right mouse button. stays pressed until ReleaseLeftButton is called
        /// </summary>
        public static void PressRightButton()
        {
            SendMouseEvent(LowLevel.MouseEventFlags.RightDown);
            Log("RMB Down");
        }

        /// <summary>
        /// release the right mouse button
        /// </summary>
        public static void ReleaseRightButton()
        {
            SendMouseEvent(LowLevel.MouseEventFlags.RightUp);
            Log("RMB Up");
        }

        /// <summary>
        /// move the cursor
        /// </summary>
        /// <param name="p">the point on screen to move to</param>
        public static void MoveTo(Point p)
        {
            bool ok = LowLevel.SetCursorPos(p.X, p.Y);
            Log($"MOVE {p.X} / {p.Y} OK: {ok}");
        }

        /// <summary>
        /// get the current cursor position
        /// </summary>
        /// <returns>the cursor position</returns>
        public static Point GetPosition()
        {
            LowLevel.GetCursorPos(out LowLevel.MousePoint p);
            return new Point(p.X, p.Y);
        }

        /// <summary>
        /// send a mouse event on the current cursor position
        /// </summary>
        /// <param name="flags">the event flags</param>
        static void SendMouseEvent(LowLevel.MouseEventFlags flags)
        {
            // get cursor position for event
            LowLevel.GetCursorPos(out LowLevel.MousePoint pos);

            // send event
            LowLevel.SendMouseEvent((int)flags, pos.X, pos.Y, 0, 0);
        }

        /// <summary>
        /// log a message
        /// </summary>
        /// <param name="msg">message to write</param>
        static void Log(string msg)
        {
            if (LogCommands)
                Console.WriteLine("[Mouse] " + msg);
        }

        /// <summary>
        /// low level API calls
        /// </summary>
        private static class LowLevel
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct MousePoint
            {
                public int X;
                public int Y;

                public MousePoint(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }

            [Flags]
            public enum MouseEventFlags
            {
                LeftDown = 0x00000002,
                LeftUp = 0x00000004,
                MiddleDown = 0x00000020,
                MiddleUp = 0x00000040,
                Move = 0x00000001,
                Absolute = 0x00008000,
                RightDown = 0x00000008,
                RightUp = 0x00000010
            }

            [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int x, int y);

            [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out MousePoint lpMousePoint);

            [DllImport("user32.dll", EntryPoint = "mouse_event")]
            public static extern void SendMouseEvent(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        }
    }
}
