using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using FSendKeys = System.Windows.Forms.SendKeys;

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

        /// <summary>
        /// set the stroke width (0 for smallest, 3 for max)
        /// </summary>
        /// <param name="width">the stroke width to select</param>
        public void SetStrokeWidth(int width)
        {
            // check bounds
            if (width < 0 || width > 3)
                throw new ArgumentOutOfRangeException("width has to be between 0 and 3");

            // set stroke: ALT > R > SZ > n*DOWN > Enter
            SendKeys(Keys.Menu, Keys.R, Keys.S, Keys.Z);
            for (int i = 0; i < width; i++)
                SendKeys(Keys.Down);
            SendKeys(Keys.Enter);
        }

        /// <summary>
        /// set the paint color
        /// </summary>
        /// <param name="r">red part</param>
        /// <param name="g">green part</param>
        /// <param name="b">blue part</param>
        public void SetColor(byte r, byte g, byte b)
        {
            //edit pallet
            //ALT > R > EC
            SendKeys(Keys.Menu, Keys.R, Keys.E, Keys.C);

            // go to input field for RED
            // 7x TAB
            SendKeys(Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab);

            // send value for red
            DontFuckUpMyPC();
            FSendKeys.SendWait(r.ToString());

            // go to field for GREEN
            // 1x TAB
            SendKeys(Keys.Tab);

            // send value for green
            FSendKeys.SendWait(g.ToString());

            // go to field for BLUE
            // 1x TAB
            SendKeys(Keys.Tab);

            // send value for blue
            FSendKeys.SendWait(b.ToString());

            // exit window with OK
            // 2x TAB > ENTER
            SendKeys(Keys.Tab, Keys.Tab, Keys.Enter);
        }

        /// <summary>
        /// enter painting mode by selecting the default brush tool
        /// </summary>
        public void EnterPaintMode()
        {
            // select brush P1
            // ALT > R > P1 > ENTER
            SendKeys(Keys.Menu, Keys.R, Keys.P, Keys.D1, Keys.Enter);

        }

        /// <summary>
        /// clear everything inside the bounds
        /// </summary>
        public void ClearBounds()
        {
            // select rectangle
            // ALT > R > AU > R
            SendKeys(Keys.Menu, Keys.R, Keys.A, Keys.U, Keys.R);

            // move cursor to bounds top left and press LMB
            DontFuckUpMyPC();
            Cursor.MoveTo(Bounds.Location);
            Sleep();
            Cursor.LMBDown();
            Sleep();

            // move cursor to bounds bottom right and release LMB
            Cursor.MoveTo(new Point(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height));
            Sleep();
            Cursor.LMBUp();

            // delete 
            // ALT > R > AU > S
            SendKeys(Keys.Menu, Keys.R, Keys.A, Keys.U, Keys.S);
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
        /// send keys.
        /// </summary>
        /// <param name="keys">the keys to press</param>
        void SendKeys(params Keys[] keys)
        {
            DontFuckUpMyPC();
            Util.SendKeys(25, keys);
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
