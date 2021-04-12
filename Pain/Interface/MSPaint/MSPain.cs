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
        /// <summary>
        /// enable command logging to console
        /// </summary>
        public bool LogCommands { get; set; } = false;

        /// <summary>
        /// path to the paint executeable
        /// </summary>
        public string PaintPath { get; set; } = @"C:\Windows\System32\mspaint.exe";

        /// <summary>
        /// the bounds we are allowed to work in
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// millisecond delay between mouse moves
        /// </summary>
        public int MoveDelay { get; set; } = 50;

        /// <summary>
        /// the ms paint commands abstraction
        /// </summary>
        public IPaintControlCommands Commands { get; set; }

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

        #region IDrawTarget
        /// <summary>
        /// set the current primary color.
        /// does not select the primary color
        /// </summary>
        /// <param name="color">the color to set</param>
        public void SetPrimaryColor(Color color)
        {
            Log("SetPrimaryColor");
            Cmd.SelectPrimaryColor();
            Cmd.SetColorRGB(color.R, color.G, color.B);
        }

        /// <summary>
        /// set the current secondary color.
        /// does not select the secondary color
        /// </summary>
        /// <param name="color">the color to set</param>
        public void SetSecondaryColor(Color color)
        {
            Log("SetSecondaryColor");
            Cmd.SelectSecondaryColor();
            Cmd.SetColorRGB(color.R, color.G, color.B);
        }

        /// <summary>
        /// select the current primary color
        /// </summary>
        public void SelectPrimaryColor()
        {
            Log("SelectPrimaryColor");
            Cmd.SelectPrimaryColor();
        }

        /// <summary>
        /// select the current secondary color
        /// </summary>
        public void SelectSecondaryColor()
        {
            Log("SelectSecondaryColor");
            Cmd.SelectSecondaryColor();
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

            // change back to paintbrush
            Cmd.SelectPaintBrush();
        }

        /// <summary>
        /// set the stroke width
        /// </summary>
        /// <param name="width">stroke width, range 0.0 - 1.0</param>
        public void SetStroke(float width)
        {
            byte w = (byte)Math.Floor(width * 3);
            Log($"SetStroke {width} ({w})");
            Cmd.SetStrokeWidth(w);
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
            MouseDown();
            MouseUp();

            // change back to paintbrush
            Cmd.SelectPaintBrush();
        }

        /// <summary>
        /// draw a dot in the currently selected color and stroke
        /// </summary>
        /// <param name="at">where to draw the dot, range 0.0 - 1.0</param>
        public void DrawDot(PointF at)
        {
            Log($"DrawDot X: {at.X} Y: {at.Y}");

            // move to position
            Mouse.MoveTo(PointToScreen(at));
            MoveSleep();

            // mouse down, then up
            MouseDown();
            MouseUp();
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

            // move to first vert and mouse down
            DontBreakStuffPls();
            Mouse.MoveTo(PointToScreen(vertices[0]));
            MoveSleep();
            MouseDown();

            // each vert
            DontBreakStuffPls();
            foreach (PointF vert in vertices)
            {
                // move to position
                Mouse.MoveTo(PointToScreen(vert));
                MoveSleep();
            }

            // mouse up again
            MouseUp();
        }
        #endregion

        #region Util
        /// <summary>
        /// LMB down abstraction.
        /// because we have the choice between LMB and Space for this one
        /// </summary>
        void MouseDown()
        {
            //Keyboard.KeyDown(VK.Space);
            Mouse.PressLeftButton();
        }

        /// <summary>
        /// LBM up abstraction.
        /// because we have the choice between LMB and Space for this one
        /// </summary>
        void MouseUp()
        {
            //Keyboard.KeyUp(VK.Space);
            Mouse.ReleaseLeftButton();
        }

        /// <summary>
        /// sleep for the move delay
        /// </summary>
        void MoveSleep()
        {
            if (MoveDelay > 0)
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
            if (foreground.HasExited || foreground.MainModule.FileName.Equals(PaintPath, StringComparison.OrdinalIgnoreCase))
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
