using Pain.Driver;
using Pain.Interface.MSPaint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace Pain
{
    /// <summary>
    /// I'm sick of debugging rn, so i'll just cheat a bit :P
    /// </summary>
    public class CheatApp
    {
        public static void CheatMain()
        {
            // get video frames path
            Console.Write("Enter frames dir (create using ffmpeg, format png): ");
            string framesDir = Console.ReadLine();

            // setup safezone
            Bounds = GetSafeZone();

            // wait for user
            Console.WriteLine("Ready. Enter to Start");
            Console.WriteLine("Press ESC any time to stop");
            Console.ReadLine();

            // tell user to switch to paint
            for (int i = 5; i > 0; i--)
            {
                Console.Write($"Switch to paint now! starting in {i} s");
                Console.CursorLeft = 0;
                Thread.Sleep(1000);
            }
            Console.WriteLine();

            // setup paint
            Color primary = Color.Black;
            Color secondary = Color.White;

            // select brush
            Cmd.SelectPaintBrush();

            // set primary and secondary color
            Cmd.SelectSecondaryColor();
            Cmd.SetColorRGB(secondary.R, secondary.G, secondary.B);
            Cmd.SelectPrimaryColor();
            Cmd.SetColorRGB(primary.R, primary.G, primary.B);

            // set stroke width to max
            Cmd.SetStrokeWidth(3);

            // select brush (again, to be safe)
            Cmd.SelectPaintBrush();

            // create a DIFF dotmap for every image in the path
            Bitmap prev = null;
            foreach (string img in Directory.EnumerateFiles(framesDir, "*.png", SearchOption.TopDirectoryOnly))
            {
                // load image as bitmap
                using (Bitmap bmp = new Bitmap(img))
                {
                    // draw
                    Bitmap nPrev = RenderWithDots(bmp, prev, 0.2f, primary, secondary);

                    // dispose previous prev
                    prev?.Dispose();
                    prev = nPrev;
                }
            }

        }

        /// <summary>
        /// draw a image to paint using dots.
        /// Assumes the following:
        /// 
        /// - bmp * scale is equal size to prevScaled
        /// - paint is setup (primary and secondary color set, stroke width set, paintbrush selected)
        /// - Bounds is set
        /// 
        /// </summary>
        /// <param name="bmp">the bitmap to draw</param>
        /// <param name="prevScaled">the previous bitmap, scaled (return of call to RenderWithDots)</param>
        /// <param name="scale">the scale to draw at</param>
        /// <param name="primary">the primary draw color</param>
        /// <param name="secondary">the secondary draw color</param>
        /// <returns>the scaled bitmap, use for prevScaled in next call for diff rendering</returns>
        static Bitmap RenderWithDots(Bitmap bmp, Bitmap prevScaled, float scale, Color primary, Color secondary)
        {
            // scale input bitmap
            int scaledW = (int)Math.Floor(Bounds.Width * scale);
            int scaledH = (int)Math.Floor(Bounds.Height * scale);
            Bitmap scaled = new Bitmap(scaledW, scaledH);

            // get graphics from the scaled bitmap
            // and draw bitmap scaled
            using (Graphics g = Graphics.FromImage(scaled))
                g.DrawImage(bmp, 0, 0, scaledW, scaledH);

            // get every pixel of the scaled original
            for (int x = 0; x < scaledW; x++)
            {
                float xNorm = (float)x / scaledW;
                Color? alreadyPressed = null;
                bool? lastMouseButton = null;
                for (int y = 0; y < scaledH; y++)
                {
                    // check if the pixel in the previous bitmap is (more or less) equal
                    // skip if yes
                    Color c = scaled.GetPixel(x, y);
                    if (prevScaled != null
                        && ColorComparisions.DeltaE(c, prevScaled.GetPixel(x, y)) <= 1f)
                        continue;

                    // only change if different color
                    if (c.Equals(alreadyPressed))
                        continue;

                    alreadyPressed = c;

                    // normalize position
                    float yNorm = (float)y / scaledH;

                    // check what color to draw
                    float deltaEPrimary = ColorComparisions.DeltaE(c, primary);
                    float deltaESecondary = ColorComparisions.DeltaE(c, secondary);
                    bool usePrimary = deltaEPrimary <= deltaESecondary;

                    // draw dot
                    //Log($"Dot X: {xNorm} Y: {yNorm}");
                    DontBreakStuffPls();
                    if (lastMouseButton.HasValue)
                    {
                        MouseUp(lastMouseButton.Value);
                        MoveSleep();
                    }
                    Mouse.MoveTo(PointToScreen(new PointF(xNorm, yNorm)));
                    MoveSleep();
                    MouseDown(usePrimary);
                    lastMouseButton = usePrimary;
                }

                Mouse.MoveTo(PointToScreen(new PointF(xNorm, 1f)));
                MoveSleep();
                if (lastMouseButton.HasValue)
                    MouseUp(lastMouseButton.Value);
            }

            // return the scaled image for the next call
            return scaled;
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


        #region Paint Driver
        /// <summary>
        /// path to the paint executeable
        /// </summary>
        static string PaintPath { get; } = @"C:\Windows\System32\mspaint.exe";

        /// <summary>
        /// millisecond delay between mouse moves
        /// </summary>
        static int MoveDelay { get; set; } = 0;

        /// <summary>
        /// the bounds we are allowed to work in
        /// </summary>
        static Rectangle Bounds { get; set; }

        /// <summary>
        /// like <see cref="commands"/>, but calls <see cref="DontBreakStuffPls"/> first
        /// </summary>
        static IPaintControlCommands Cmd
        {
            get
            {
                DontBreakStuffPls();
                return commands;
            }
        }
        static IPaintControlCommands commands = new GermanPaintControlCommands();

        /// <summary>
        /// mouse button down abstraction.
        /// because we have the choice between LMB / Space for primary color and RMB for secondary color
        /// </summary>
        /// <param name="lmb">use left mouse button? if false, rmb is used</param>
        static void MouseDown(bool lmb = true)
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
        static void MouseUp(bool lmb = true)
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
        static void MoveSleep()
        {
            Thread.Sleep(MoveDelay);
        }

        /// <summary>
        /// convert a normalized point (range 0.0 - 1.0) to screen coordinates inside the bounds
        /// </summary>
        /// <param name="p">the normalized point</param>
        /// <returns>the point on screen</returns>
        static Point PointToScreen(PointF p)
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
        static void DontBreakStuffPls()
        {
            // error if we have no bounds
            if (Bounds == null)
                throw new NullReferenceException("Bounds are not set");

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
        static void Log(string msg)
        {
            Console.WriteLine("[Pain] " + msg);
        }

        #endregion
    }
}
