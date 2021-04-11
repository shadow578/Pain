using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Media;

using WPoint = System.Windows.Point;

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
 
        // region SVG Path

        /// <summary>
        /// attempt to draw a svg path (uses WPF geometry under the hood)
        /// </summary>
        /// <param name="path">the path to draw</param>
        /// <param name="width">the viewport width</param>
        /// <param name="height">the viewport height</param>
        public void DrawPath(string path, int width, int height)
        {
            CW("CMD: DrawPath");

            // parse using WPF
            Geometry geometry = Geometry.Parse(path);
            PathGeometry pathGeometry = PathGeometry.CreateFromGeometry(geometry);

            // every figure
            foreach (PathFigure fig in pathGeometry.Figures)
            {
                // every segement
                foreach (PathSegment seg in fig.Segments)
                {
                    CW($"Segment: {seg.GetType().Name}");
                    DrawPolyBezier(seg as PolyBezierSegment, width, height);
                }
            }
        }

        /// <summary>
        /// draw a poly bezier (badly, that is)
        /// </summary>
        /// <param name="seg">the segment to draw</param>
        /// <param name="width">the viewport width</param>
        /// <param name="height">the viewport height</param>
        void DrawPolyBezier(PolyBezierSegment seg, int width, int height)
        {
            // check the segment is valid
            if (seg == null)
                return;

            // prepare polygon to draw
            PointF[] poly = seg.Points.Select(p =>
            {
                // normalize point
                p.X /= width;
                p.Y /= height;

                // convert to PointF
                return ToPoint(p);
            }).ToArray();

            // draw polygon closed
            DrawPolygon(verticies: poly);
        }

        //endregion

        /// <summary>
        /// draw a polygon
        /// </summary>
        /// <param name="verticies">the verticies</param>
        /// <param name="close">close the polygon after the draw?</param>
        public void DrawPolygon(bool close = true, params PointF[] verticies)
        {
            // start draw
            CW("CMD: DrawPoly");

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

        //region util
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
        /// convert a WPF point to a Forms point
        /// </summary>
        /// <param name="point">the wpf point</param>
        /// <returns>the forms point</returns>
        PointF ToPoint(WPoint point)
        {
            return new PointF((float)point.X, (float)point.Y);
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
            if(EnableCW)
                Console.WriteLine(msg);
        }
        //endregion
    }
}
