using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

        #region draw commands

        /// <summary>
        /// draw a bitmap by drawing many dots
        /// </summary>
        /// <param name="bmp">the bitmap to draw. scale this down as much as possible to speed draw up</param>
        /// <param name="dropColorBits">color bits to drop per component. 0 - 7</param>
        /// <param name="pixelStep">pixel step size. 1 for highest quality, higher to drop pixels</param>
        public void DrawBitmapWithDots(Bitmap bmp, byte dropColorBits = 0, int pixelStep = 1)
        {
            // start draw
            CW($"DrawBitmapWithDots: WxH: {bmp.Width} x {bmp.Height}");

            // get all pixels and their normalized positions, grouped by color
            Console.WriteLine(" prepare dot map");
            Dictionary<Color, List<PointF>> dots = new Dictionary<Color, List<PointF>>();
            double totalDots = 0;
            for (int x = 0; x < bmp.Width; x += pixelStep)
                for (int y = 0; y < bmp.Height; y += pixelStep)
                {
                    // get pixel to draw
                    // GetPixel is slow, but drawing is so slow it does not matter :P
                    Color px = bmp.GetPixel(x, y);

                    // drop bits
                    if (dropColorBits > 0 && dropColorBits < 8)
                    {
                        byte r = (byte)((px.R >> dropColorBits) << dropColorBits);
                        byte g = (byte)((px.G >> dropColorBits) << dropColorBits);
                        byte b = (byte)((px.B >> dropColorBits) << dropColorBits);
                        px = Color.FromArgb(r, g, b);
                    }

                    // get position normalized
                    PointF pos = new PointF((float)x / bmp.Width, (float)y / bmp.Height);

                    // insert into list
                    if (!dots.ContainsKey(px))
                        dots.Add(px, new List<PointF>());

                    dots[px].Add(pos);
                    totalDots++;
                }

            // map all colors that have less than 50 dots to the nearest color that has more than 50 dots
            // to avoid drawing lots of single dots that require their own unique color (setting the color takes time)
            Console.WriteLine($" Optimize dot map; unique colors: {dots.Values.Count}");
            const int MIN_CNT = 50;
            foreach (Color c in dots.Keys.ToList().OrderBy((cx) => dots[cx].Count))
                if (dots[c].Count < MIN_CNT)
                {
                    // this color has less than 50 dots, find the nearest color with 50+ dots
                    Color nearest = c;
                    double nearestDistance = double.MaxValue;
                    foreach (Color cN in dots.Keys)
                        if (dots[cN].Count >= MIN_CNT)
                        {
                            // calculate distance to target color
                            double dist = ColorDistanceSqrt(c, cN);
                            if (dist < nearestDistance)
                            {
                                // this color is closer, take it
                                nearestDistance = dist;
                                nearest = cN;
                            }
                        }

                    // add all dots to the nearest color's dot list 
                    dots[nearest].AddRange(dots[c]);

                    // and clear this colors list
                    dots[c].Clear();
                }

            // draw each unique color in one batch
            // sort so that the most frequent color is drawn first
            CW($" Start draw in 1s Unique Colors: {dots.Values.Where(dl => dl.Count > 0).Count()}; Total Dots: {totalDots}");
            Thread.Sleep(1000);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            double dotsDrawn = 0;
            bool firstColor = true;
            foreach (Color c in dots.Keys.ToList().OrderByDescending((cx) => dots[cx].Count))
                if (dots[c].Count > 0)
                {
                    // select the color
                    SetColor(c.R, c.G, c.B);

                    // enter draw mode
                    //DontFuckUpMyPC();
                    //EnterPaintMode();

                    // if this is the first color, use bucket tool
                    if (firstColor)
                    {
                        // bucket in the center
                        BucketAt(new PointF(0.5f, 0.5f));

                        // revert to paint mode
                        EnterPaintMode();

                        // add "drawn" pixels to total
                        dotsDrawn += dots[c].Count;

                        // to the next color
                        firstColor = false;
                        continue;
                    }

                    // draw dots
                    List<PointF> dotBatch = dots[c];
                    double dotsThisBatch = dotBatch.Count;
                    double dotsDrawnBatch = 0;
                    foreach (PointF p in dotBatch)
                    {
                        // move to position and press LMB for a moment
                        DontFuckUpMyPC();
                        Cursor.MoveTo(ToScreenSpace(p));
                        Cursor.LMBDown();
                        Cursor.LMBUp();

                        // update %
                        dotsDrawn++;
                        dotsDrawnBatch++;
                        Console.Title = $"ESC to cancel | {dotsDrawn * 100 / totalDots:0.0}% TOTAL | {dotsDrawnBatch * 100 / dotsThisBatch:0.0}% of {dotsThisBatch} in batch";
                    }
                }

            sw.Stop();
            Console.WriteLine($" done after {sw.Elapsed.TotalSeconds} seconds ({sw.Elapsed})");
        }

        /// <summary>
        /// draw a single dot
        /// </summary>
        /// <param name="pos">the position, normalized to 0.0 to 1.0</param>
        public void DrawDot(PointF pos)
        {
            // start draw
            CW($"CMD: DrawDot, at: {pos.X} / {pos.Y}");

            // move to position and press LMB for a moment
            DontFuckUpMyPC();
            Cursor.MoveTo(ToScreenSpace(pos));
            Cursor.LMBDown();
            Sleep();
            DontFuckUpMyPC();
            Cursor.LMBUp();
        }

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
        #endregion

        #region control commands
        /// <summary>
        /// set the stroke width (0 for smallest, 3 for max)
        /// </summary>
        /// <param name="width">the stroke width to select</param>
        public void SetStrokeWidth(int width)
        {
            CW("CMD: SetStrokeWidth");

            // check bounds
            if (width < 0 || width > 3)
                throw new ArgumentOutOfRangeException("width has to be between 0 and 3");

            // set stroke: ALT > R > SZ > n*DOWN > Enter
            DontFuckUpMyPC();
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
            CW("CMD: SetColor");

            //edit pallet
            //ALT > R > EC
            DontFuckUpMyPC();
            SendKeys(Keys.Menu, Keys.R, Keys.E, Keys.C);

            // go to input field for RED
            // 7x TAB
            DontFuckUpMyPC();
            SendKeys(Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab);

            // send value for red
            DontFuckUpMyPC();
            FSendKeys.SendWait(r.ToString());

            // go to field for GREEN
            // 1x TAB
            DontFuckUpMyPC();
            SendKeys(Keys.Tab);

            // send value for green
            DontFuckUpMyPC();
            FSendKeys.SendWait(g.ToString());

            // go to field for BLUE
            // 1x TAB
            DontFuckUpMyPC();
            SendKeys(Keys.Tab);

            // send value for blue
            DontFuckUpMyPC();
            FSendKeys.SendWait(b.ToString());

            // exit window with OK
            // 2x TAB > ENTER
            DontFuckUpMyPC();
            SendKeys(Keys.Tab, Keys.Tab, Keys.Enter);
        }

        /// <summary>
        /// enter painting mode by selecting the default brush tool
        /// </summary>
        public void EnterPaintMode()
        {
            CW("CMD: EnterPaintMode");

            // select brush P1
            // ALT > R > P1 > ENTER
            DontFuckUpMyPC();
            SendKeys(Keys.Menu, Keys.R, Keys.P, Keys.D1, Keys.Enter);

        }

        /// <summary>
        /// clear everything inside the bounds
        /// </summary>
        public void ClearBounds()
        {
            CW("CMD: ClearBounds");

            // select rectangle
            // ALT > R > AU > R
            DontFuckUpMyPC();
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
            DontFuckUpMyPC();
            SendKeys(Keys.Menu, Keys.R, Keys.A, Keys.U, Keys.S);
        }

        /// <summary>
        /// use the bucket tool at the given normal position
        /// </summary>
        /// <param name="pos">the position</param>
        public void BucketAt(PointF pos)
        {
            CW("CMD: BucketAt");

            // select bucket tool
            // ALT > R > FF
            DontFuckUpMyPC();
            SendKeys(Keys.Menu, Keys.R, Keys.F, Keys.F);
            Sleep();

            // move to position and press LMB for a moment
            DontFuckUpMyPC();
            Cursor.MoveTo(ToScreenSpace(pos));
            Cursor.LMBDown();
            Sleep();
            DontFuckUpMyPC();
            Cursor.LMBUp();
        }
        #endregion

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
            bool escape = Util.IsDown(Keys.Escape);
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

        double ColorDistanceSqrt(Color a, Color b)
        {
            //get deltas
            double dR = Math.Abs(a.R - b.R);
            double dG = Math.Abs(a.G - b.G);
            double dB = Math.Abs(a.B - b.B);

            // get distance squared
            return Math.Pow(dR, 2) + Math.Pow(dG, 2) + Math.Pow(dB, 2);
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
