using Pain;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PaintTestFX
{
    public static class Cursor
    {
        /// <summary>
        /// enable command logging to console
        /// </summary>
        public static bool ENABLE_CW { get; set; } = false;

        [StructLayout(LayoutKind.Sequential)]
        struct MousePoint
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
        enum MouseEventFlags
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
        private static extern bool _SetCursorPos(int x, int y);

        [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        private static extern void _SendMouseEvent(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        /// <summary>
        /// press the left mouse button. pressed until LMBUp is called
        /// </summary>
        public static void LMBDown()
        {
            //SendMouseEvent(MouseEventFlags.LeftDown);
            CW("LMBDown");
        }

        /// <summary>
        /// release the left mouse button
        /// </summary>
        public static void LMBUp()
        {
            SendMouseEvent(MouseEventFlags.LeftUp);
            CW("LMBUp");
        }

        /// <summary>
        /// press the right mouse button. pressed until LMBUp is called
        /// </summary>
        public static void RMBDown()
        {
            SendMouseEvent(MouseEventFlags.RightDown);
            CW("RMBDown");
        }

        /// <summary>
        /// release the right mouse button
        /// </summary>
        public static void RMBUp()
        {
            SendMouseEvent(MouseEventFlags.RightUp);
            CW("RMBUp");
        }

        /// <summary>
        /// move the cursor
        /// </summary>
        /// <param name="p">the point on screen to move to</param>
        public static void MoveTo(Point p)
        {
            bool ok = _SetCursorPos(p.X, p.Y);
            CW($"MOV {p.X} / {p.Y}  OK: {ok}");
        }

        /// <summary>
        /// get the current cursor position
        /// </summary>
        /// <returns>the cursor position</returns>
        public static Point GetCursorPos()
        {
            _GetCursorPos(out MousePoint p);
            return new Point(p.X, p.Y);
        }

        /// <summary>
        /// send a mouse event on the current cursor position
        /// </summary>
        /// <param name="flags">the event flags</param>
        static void SendMouseEvent(MouseEventFlags flags)
        {
            // get cursor position for event
            _GetCursorPos(out MousePoint p);

            // send event
            _SendMouseEvent((int)flags, p.X, p.Y, 0, 0);
        }

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
