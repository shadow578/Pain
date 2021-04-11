using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace PaintTestFX
{
    public class PainInterface
    {
        const string PAINT_EXE = "mspaint.exe";

        /// <summary>
        /// bounds in which the cursor may move
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// movement delay, ms. if this is too low, paint does not recognize the movement correctly
        /// </summary>
        public int MoveDelay { get; set; } = 50;

        /// <summary>
        /// enable command logging to console
        /// </summary>
        public bool EnableCW { get; set; } = false;

        /// <summary>
        /// draw a polygon
        /// </summary>
        /// <param name="verticies">the verticies</param>
        /// <param name="close">close the polygon after the draw?</param>
        public void DrawPolygon(bool close = true, params PointF[] verticies)
        {
            // start draw
            CW($"CMD: DrawPoly, close: {close}, vertCoutn: {verticies.Length}");

            // move to first vert and mouse down
            DontFuckUpMyPC();
            Cursor.MoveTo(ToScreenSpace(verticies[0]));
            Cursor.LMBDown();
            Sleep();

            // move to each verticy
            foreach (PointF vert in verticies)
            {
                DontFuckUpMyPC();
                Cursor.MoveTo(ToScreenSpace(vert));
                Sleep();
            }

            // move back to the first vert
            DontFuckUpMyPC();
            if (close)
                Cursor.MoveTo(ToScreenSpace(verticies[0]));

            // mouse up
            Cursor.LMBUp();
        }

        #region util
        /// <summary>
        /// movement delay
        /// </summary>
        void Sleep()
        {
            Thread.Sleep(MoveDelay);
        }

        /// <summary>
        /// convert a normalized point 0.0 - 1.0 to screen (bounds) space
        /// </summary>
        /// <param name="point">the normalized point</param>
        /// <returns>the screen point</returns>
        Point ToScreenSpace(PointF point)
        {
            // check x and y are in range 0-1
            if (point.X < 0 || point.X > 1 || point.Y < 0 || point.Y > 1)
                throw new ArgumentOutOfRangeException("input point has to be normalized 0.0-1.0");

            // multiply with bounds
            int x = (int)Math.Floor(point.X * Bounds.Size.Width);
            int y = (int)Math.Floor(point.Y * Bounds.Size.Height);

            // add top left
            x += Bounds.X;
            y += Bounds.Y;

            return new Point(x, y);
        }

        /// <summary>
        /// keep the host pc safe... call this before doing anything with the cursor
        /// </summary>
        /// <param name="throwIfBad">if true, the function throws a InvalidOperationExeption if something is bad</param>
        /// <returns>is everything ok?</returns>
        bool DontFuckUpMyPC(bool throwIfBad = true)
        {
            // get foreground process
            Process fg = Util.GetForegroundWindowProcess();

            // check the foreground executable is mspaint.exe and still open
            bool isMsPaint = !fg.HasExited && fg.MainModule.FileName.EndsWith(PAINT_EXE);
            if (!isMsPaint && throwIfBad)
                throw new InvalidOperationException("Foreground process is not mspaint! aborting");


            // check if abort (ESC) key was pressed
            bool escape = Util.IsDown(System.Windows.Forms.Keys.Escape);
            if (escape)
                throw new InvalidOperationException("Escape ESC pressed");

            if (isMsPaint)
            {
                // get cursor position
                Point cursorPos = Cursor.GetCursorPos();

                // check cursor is inside allowed bounds
                if (!Bounds.Contains(cursorPos))
                {
                    // uhm... move it inside bounds
                    Cursor.MoveTo(Bounds.Location);
                }
            }

            return isMsPaint;
        }

        /// <summary>
        /// console write
        /// </summary>
        /// <param name="msg">message to write</param>
        void CW(string msg)
        {
            if (EnableCW)
                Console.WriteLine(msg);
        }
        #endregion
    }
}
