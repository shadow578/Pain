using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

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

        // region SVG Path

        public void DrawPath(string path, int width, int height)
        {
            /* CUSTOM FUNCTION -> HARD
            // these chars are all valid SVG commands. whenever we find one, a new command starts.
            string separators = @"(?=[MZLHVCSQTAmzlhvcsqta])";

            // split path string into command tokens
            SVGToken[] pathTokens = Regex.Split(path, separators)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => SVGToken.Parse(t))
                .ToArray();

            // run each command
            SVGToken lastToken;
            foreach (SVGToken token in pathTokens)
            {
                // commands
                switch (token.Command)
                {
                    case 'm':
                        // move relative
                        break;
                    case 'M':
                        // move absolute
                        break;
                    case 'z':
                    case 'Z':
                        // close path
                        break;

                    default:
                        // not supported
                        Console.WriteLine("Unsupported Token: " + token.Command);
                        break;
                }
            }
            */

            // parse using WPF
            Geometry geometry = Geometry.Parse(path);
            PathGeometry pathGeometry = PathGeometry.CreateFromGeometry(geometry);

            // every figure
            foreach (PathFigure fig in pathGeometry.Figures)
            {
                // every segement
                foreach (PathSegment seg in fig.Segments)
                {


                    Console.WriteLine("wat");
                }
            }
        }

        class SVGToken
        {
            /// <summary>
            /// command
            /// </summary>
            public char Command { get; private set; }

            /// <summary>
            /// command args
            /// </summary>
            public float[] Args { get; private set; }

            public SVGToken(char cmd, params float[] args)
            {
                Command = cmd;
                Args = args;
            }

            public static SVGToken Parse(string tokenStr)
            {
                //get command
                char cmd = tokenStr.Take(1).Single();

                // get the rest
                string argsStr = tokenStr.Substring(1);

                // split the args string and parse to floats
                float[] args = Regex.Split(argsStr, @"[\s,]|(?=-)")
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(arg => float.Parse(arg))
                    .ToArray();

                // create token
                return new SVGToken(cmd, args);
            }
        }
        //endregion

        /// <summary>
        /// draw a polygon
        /// </summary>
        /// <param name="verticies">the verticies</param>
        public void DrawPolygon(params PointF[] verticies)
        {
            // mouse down
            DontFuckUpMyPC();
            Cursor.LMBDown();

            // move to each verticy
            foreach (PointF vert in verticies)
            {
                DontFuckUpMyPC();
                Cursor.MoveTo(ToScreenSpace(vert));
                Sleep();
            }

            // move back to the first vert
            DontFuckUpMyPC();
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
        //endregion
    }
}
