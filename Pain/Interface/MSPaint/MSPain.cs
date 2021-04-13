using Pain.Driver;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Pain.Interface.MSPaint
{
    /// <summary>
    /// MS Paint abstraction layer
    /// </summary>
    public class MSPain : IDrawTarget
    {
        #region Init

        /// <summary>
        /// create a new instance with safezone from user input
        /// </summary>
        /// <param name="cmd">paint command abstractions instance</param>
        /// <returns>the instance</returns>
        public static MSPain Create(IPaintControlCommands cmd)
        {
            return Create(GetSafeZone(), cmd);
        }

        /// <summary>
        /// create a new instance
        /// </summary>
        /// <param name="safeZone">the screen bounds</param>
        /// <param name="cmd">paint command abstractions instance</param>
        /// <returns>the instance</returns>
        public static MSPain Create(Rectangle safeZone, IPaintControlCommands cmd)
        {
            return new MSPain
            {
                Bounds = safeZone,
                Commands = cmd
            };
        }

        /// <summary>
        /// Get the safe zone on screen from user input
        /// </summary>
        /// <returns>the safe zone</returns>
        static Rectangle GetSafeZone()
        {
            // wait for user
            Console.WriteLine("Press <SPACE> to start setting up the safe zone");
            while (!Keyboard.IsDown(VK.Space))
                Thread.Sleep(50);

            Console.WriteLine("OK, release now");
            while (Keyboard.IsDown(VK.Space))
                Thread.Sleep(50);

            // get top left
            Console.WriteLine("Move the cursor to the TOP LEFT corner of your safe zone and click LMB");
            while (!Keyboard.IsDown(VK.LeftButton))
            {
                Point p = Mouse.GetPosition();
                Console.Write($"Position: {p.X} / {p.Y}      ");
                Console.CursorLeft = 0;
                Thread.Sleep(50);
            }

            // get pos and make user release button
            Point topLeft = Mouse.GetPosition();
            //Console.WriteLine("\nOk, release now");
            //while (Keyboard.IsDown(VK.LeftButton))
            //    Thread.Sleep(50);

            // get bottom right
            Console.WriteLine("\nOk, Move the cursor to the BOTTOM RIGHT corner of your safe zone and release LMB");
            while (Keyboard.IsDown(VK.LeftButton))
            {
                // get second point
                Point p = Mouse.GetPosition();

                // calculate zone
                Rectangle zonePre = new Rectangle(topLeft.X, 
                    topLeft.Y, 
                    Math.Clamp(p.X - topLeft.X, 0, int.MaxValue),
                    Math.Clamp(p.Y - topLeft.Y, 0, int.MaxValue));
                Console.Write($"Position: {p.X} / {p.Y} \t Zone: X: {zonePre.X} Y: {zonePre.Y} W: {zonePre.Width} H: {zonePre.Height}    ");
                Console.CursorLeft = 0;

                Thread.Sleep(50);
            }

            // get pos and make user release button
            Point bottomRight = Mouse.GetPosition();
            Console.WriteLine("\nOk, release now");
            while (Keyboard.IsDown(VK.LeftButton))
                Thread.Sleep(50);

            // create and return rectangle
            Rectangle zone = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            Console.WriteLine($"Safe zone is: X: {zone.X} Y: {zone.Y} W: {zone.Width} H: {zone.Height}");
            return zone;
        }

        private MSPain()
        {

        }
        #endregion

        /// <summary>
        /// enable command logging to console
        /// </summary>
        public bool LogCommands { get; set; } = false;

        /// <summary>
        /// path to the paint executeable
        /// </summary>
        public string PaintPath { get; } = @"C:\Windows\System32\mspaint.exe";

        /// <summary>
        /// millisecond delay between mouse moves
        /// </summary>
        public int MoveDelay { get; set; } = 50;

        /// <summary>
        /// the bounds we are allowed to work in
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// the ms paint commands abstraction
        /// </summary>
        public IPaintControlCommands Commands { get; private set; }

        /// <summary>
        /// like <see cref="Commands"/>, but calls <see cref="DontBreakStuffPls"/> first
        /// </summary>
        private IPaintControlCommands Cmd
        {
            get
            {
                DontBreakStuffPls();
                return Commands;
            }
        }

        /// <summary>
        /// Flag to indicate if we are drawing with the primary color
        /// == use left instead of right mouse button
        /// </summaryt
        private bool paintInPrimary = true;

        /// <summary>
        /// is the brush tool currently safe to be selected?
        /// </summary>
        private bool isBrushSelected = false;

        /// <summary>
        /// the current primary color
        /// </summary>
        private Color? currentPrimary = null;

        /// <summary>
        /// the current secondary color
        /// </summary>
        private Color? currentSecondary = null;

        /// <summary>
        /// The currently selected stroke size
        /// </summary>
        private int? currentStroke = null;

        #region IDrawTarget

        /// <summary>
        /// get the normalized size of one pixel, assuming square pixels
        /// </summary>
        public float OnePixel
        {
            get
            {
                return 1 / Bounds.Width;
            }
        }

        /// <summary>
        /// set the current primary color.
        /// does not select the primary color
        /// </summary>
        /// <param name="color">the color to set</param>
        public void SetPrimaryColor(Color color)
        {
            if (currentPrimary.HasValue && currentPrimary.Equals(color))
                return;

            Log("SetPrimaryColor");
            Cmd.SelectPrimaryColor();
            Cmd.SetColorRGB(color.R, color.G, color.B);
            isBrushSelected = false;
            currentPrimary = color;
        }

        /// <summary>
        /// set the current secondary color.
        /// does not select the secondary color
        /// </summary>
        /// <param name="color">the color to set</param>
        public void SetSecondaryColor(Color color)
        {
            if (currentSecondary.HasValue && currentSecondary.Equals(color))
                return;

            Log("SetSecondaryColor");
            Cmd.SelectSecondaryColor();
            Cmd.SetColorRGB(color.R, color.G, color.B);
            isBrushSelected = false;
            currentSecondary = color;
        }

        /// <summary>
        /// select the current primary color
        /// </summary>
        public void SelectPrimaryColor()
        {
            Log("SelectPrimaryColor");
            //Cmd.SelectPrimaryColor();
            paintInPrimary = true;
        }

        /// <summary>
        /// select the current secondary color
        /// </summary>
        public void SelectSecondaryColor()
        {
            Log("SelectSecondaryColor");
            //Cmd.SelectSecondaryColor();
            paintInPrimary = false;
        }

        /// <summary>
        /// clear the given rectangle
        /// </summary>
        /// <param name="rect">the rectangle to clear. the rectangle is normalized, so all values are in range 0.0 - 1.0</param>
        public void Clear(RectangleF rect)
        {
            Log($"Clear X: {rect.X} Y: {rect.Y} W: {rect.Width} H: {rect.Height}");

            // prepare start and end coordinates
            Point start = PointToScreen(rect.Location);
            Point end = PointToScreen(new PointF(rect.X + rect.Width, rect.Y + rect.Height));

            // enter rectangle select mode
            Cmd.RectangleSelectionMode();

            // move mouse to the start pos and mouse down
            Mouse.MoveTo(start);
            MoveSleep();
            MouseDown();

            // move to end point and release
            Mouse.MoveTo(end);
            MoveSleep();
            MouseUp();

            // delete selection
            Cmd.DeleteSelection();

            isBrushSelected = false;
        }

        /// <summary>
        /// set the stroke width
        /// </summary>
        /// <param name="width">stroke width, range 0.0 - 1.0</param>
        public void SetStroke(float width)
        {
            // make sure paintbrush is selected
            RequireBrush();

            int w = (int)Math.Floor(width * 3);
            if (currentStroke.HasValue && currentStroke == w)
                return;

            Log($"SetStroke {width} ({w})");
            Cmd.SetStrokeWidth(w);
            isBrushSelected = false;
            currentStroke = w;
        }

        /// <summary>
        /// flood- fill using the currently selected color, starting at the given position
        /// </summary>
        /// <param name="at">the position, range 0.0 - 1.0</param>
        public void Fill(PointF at)
        {
            Log($"Fill X: {at.X} Y: {at.Y}");

            // select the bucket tool
            Cmd.SelectBucketTool();

            // move to the point
            Mouse.MoveTo(PointToScreen(at));
            MoveSleep();

            // mouse down, then up
            // use LMB for primary color and RMB for secondary color
            MouseDown(paintInPrimary);
            MouseUp(paintInPrimary);

            isBrushSelected = false;
        }

        /// <summary>
        /// draw a dot in the currently selected color and stroke
        /// </summary>
        /// <param name="at">where to draw the dot, range 0.0 - 1.0</param>
        public void DrawDot(PointF at)
        {
            Log($"DrawDot X: {at.X} Y: {at.Y}");

            // make sure paintbrush is selected
            RequireBrush();

            // move to position
            DontBreakStuffPls();
            Mouse.MoveTo(PointToScreen(at));
            MoveSleep();

            // mouse down, then up
            MouseDown(paintInPrimary);
            MouseUp(paintInPrimary);
        }

        /// <summary>
        /// draw a polygon with the currently selected color and stroke.
        /// does NOT automatically close the polygon (return to the start)
        /// </summary>
        /// <param name="vertices">the vertices of the polygon</param>
        public void DrawPoly(params PointF[] vertices)
        {
            if (vertices.Length <= 0)
                return;

            Log($"DrawPoly Verts: {vertices.Length}");

            // make sure paintbrush is selected
            RequireBrush();

            // move to first vert and mouse down
            DontBreakStuffPls();
            Mouse.MoveTo(PointToScreen(vertices[0]));
            MoveSleep();
            MouseDown(paintInPrimary);
            MoveSleep();

            // each vert
            foreach (PointF vert in vertices)
            {
                Log($"Vert: X: {vert.X} Y: {vert.Y}");
                DontBreakStuffPls();

                // move to position
                Mouse.MoveTo(PointToScreen(vert));
                MoveSleep();

                // release mouse for a moment
                // this seems to resolve problems with paint not wanting to draw lines sometimes
                MouseUp(paintInPrimary);
                MouseDown(paintInPrimary);
                MoveSleep();
            }

            // mouse up again
            MouseUp(paintInPrimary);
        }
        #endregion

        #region Util

        /// <summary>
        /// select the paintbrush tool if not selected
        /// </summary>
        void RequireBrush()
        {
            if(!isBrushSelected)
            {
                Cmd.SelectPaintBrush();
                isBrushSelected = true;
            }
        }

        /// <summary>
        /// mouse button down abstraction.
        /// because we have the choice between LMB / Space for primary color and RMB for secondary color
        /// </summary>
        /// <param name="lmb">use left mouse button? if false, rmb is used</param>
        void MouseDown(bool lmb = true)
        {
            if (lmb)
            {
                //Keyboard.KeyDown(VK.Space);
                Mouse.PressLeftButton();
            }
            else
                Mouse.PressRightButton();
        }

        /// <summary>
        /// mouse button up abstraction.
        /// because we have the choice between LMB / Space for primary color and RMB for secondary color
        /// </summary>
        /// <param name="lmb">use left mouse button? if false, rmb is used</param>
        void MouseUp(bool lmb = true)
        {
            if (lmb)
            {
                //Keyboard.KeyUp(VK.Space);
                Mouse.ReleaseLeftButton();
            }
            else
                Mouse.ReleaseRightButton();
        }

        /// <summary>
        /// sleep for the move delay
        /// </summary>
        void MoveSleep()
        {
            Thread.Sleep(MoveDelay);
        }

        /// <summary>
        /// convert a normalized point (range 0.0 - 1.0) to screen coordinates inside the bounds
        /// </summary>
        /// <param name="p">the normalized point</param>
        /// <returns>the point on screen</returns>
        Point PointToScreen(PointF p)
        {
            // get x and y clamped
            float x = Math.Clamp(p.X, 0, 1);
            float y = Math.Clamp(p.Y, 0, 1);

            // multiply with the bounds dimensions
            x *= Bounds.Size.Width;
            y *= Bounds.Size.Height;

            // add top left of bounds
            x += Bounds.X;
            y += Bounds.Y;

            // floor values
            x = MathF.Floor(x);
            y = MathF.Floor(y);

            // create point instance
            return new Point((int)x, (int)y);
        }

        /// <summary>
        /// don't break stuff
        /// </summary>
        void DontBreakStuffPls()
        {
            // error if we have no bounds
            if (Bounds == null)
                throw new NullReferenceException("Bounds are not set");

            // error if we have no commands abstraction instance
            if (Commands == null)
                throw new NullReferenceException("Commands are not set");

            // check if escape key is down
            if (Keyboard.IsDown(VK.Escape))
                throw new ApplicationException("Emergency Stop (Escape) was pressed");

            // get the foreground process
            Process foreground = Window.GetForegroundProcess();

            // check the foreground process is paint and not exited
            if (foreground.HasExited || !foreground.MainModule.FileName.Equals(PaintPath, StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException("Foreground process was not MS Paint");

            // keep cursor inside safe bounds
            Point cursorPos = Mouse.GetPosition();
            if (!Bounds.Contains(cursorPos))
                Mouse.MoveTo(Bounds.Location);
        }

        /// <summary>
        /// log a message
        /// </summary>
        /// <param name="msg">message to write</param>
        void Log(string msg)
        {
            if (LogCommands)
                Console.WriteLine("[Pain] " + msg);
        }
        #endregion
    }
}
