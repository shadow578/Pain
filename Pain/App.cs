using Pain.Draw;
using Pain.Driver;
using Pain.Interface.MSPaint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Pain
{
    public static class App
    {

        public static void Main()
        {
            if (true)
            {
                BadApple.BadAppleMain();
                return;
            }

            // load images to draw
            Bitmap img1 = GetBitmap("Enter Image 1 path to draw: ");
            Bitmap img2 = GetBitmap("Enter Image 2 path to draw: ");

            // setup interface
            IPaintControlCommands cmds = new GermanPaintControlCommands
            {
                LogCommands = true
            };

            MSPain pain = MSPain.Create(cmds);
            pain.LogCommands = true;

            // setup delays
            // don't set MoveDelay to 0 unless you have a fast PC, as paint struggles at such speeds :P
            pain.MoveDelay = 0;
            Keyboard.KeyDelay = 35;

            // create dotmaps
            Console.WriteLine("preparing dotmap 1...");
            DotMap dots1 = DotMap.Of(img1, pain.Bounds.Size, .2f, 2)
                .Optimize(4, ColorComparisions.DeltaE);

            Console.WriteLine("prepare dotmap 2...");
            DotMap dots2 = DotMap.Of(img2, pain.Bounds.Size, .2f, 2)
                .Optimize(4, ColorComparisions.DeltaE)
                .Diff(dots1);

            Console.WriteLine($"dotmap1 [dots: {dots1.TotalDots} unique colors: {dots1.TotalColors}] dotmap2 [dots: {dots2.TotalDots} unique colors: {dots2.TotalColors}]");


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

            // setup progress display
            Stopwatch sw = new Stopwatch();
            sw.Start();
            dots1.Progress += pog =>
            {
                Console.Title = $"Press ESC to stop | {pog * 100:0.0}% done | {sw.Elapsed.TotalSeconds}s elapsed";
            };
            dots2.Progress += pog =>
            {
                Console.Title = $"Press ESC to stop | {pog * 100:0.0}% done | {sw.Elapsed.TotalSeconds}s elapsed";
            };

            // draw main dotmap
            dots1.DrawTo(pain, true, mode: DotMap.DrawMode.Polys);

            // draw diff dotmap
            dots2.DrawTo(pain, false, mode: DotMap.DrawMode.Polys);

            // draw a nice black outline around our image
            Draw.Path.Of(new RectangleF(0, 0, 1, 1))
                .SetColor(Color.Black)
                .DrawTo(pain);

            // done, show time elapsed
            sw.Stop();
            Console.WriteLine($"Done after {sw.Elapsed} ({sw.Elapsed.TotalSeconds:0.0} seconds)");
        }

        static void MainVideo()
        {
            // setup interface
            IPaintControlCommands cmds = new GermanPaintControlCommands
            {
                LogCommands = true
            };

            MSPain pain = MSPain.Create(cmds);
            pain.LogCommands = true;

            // setup delays
            // don't set MoveDelay to 0 unless you have a fast PC, as paint struggles at such speeds :P
            pain.MoveDelay = 0;
            Keyboard.KeyDelay = 35;

            // get video frames path
            Console.Write("Enter frames dir (create using ffmpeg, format png): ");
            string framesDir = Console.ReadLine();

            // create a DIFF dotmap for every image in the path
            List<DotMap> dotMaps = new List<DotMap>();
            DotMap prevMapNotDiffed = null;
            foreach (string img in Directory.EnumerateFiles(framesDir, "*.png", SearchOption.TopDirectoryOnly))
            {
                // load bitmap
                using (Bitmap bmp = new Bitmap(img))
                {
                    // create dotmap
                    Console.WriteLine($"prepare dotmap {dotMaps.Count}...");
                    DotMap dots = DotMap.Of(bmp, pain.Bounds.Size, .09f, 4)
                        .Optimize(2, ColorComparisions.Euclidean);

                    // diff if not the first
                    DotMap prev = prevMapNotDiffed;
                    prevMapNotDiffed = dots.Clone();
                    if (prev != null)
                        dots.Diff(prev);

                    // add to list
                    dotMaps.Add(dots.Sort());
                }
            }

            // ready, wait for user
            Console.WriteLine($"Ready, with {dotMaps.Count} dotmaps loaded.");
            Console.WriteLine("Enter to Start, use ESC to cancel any time");
            Console.ReadLine();

            // tell user to switch to paint
            for (int i = 5; i > 0; i--)
            {
                Console.Write($"Switch to paint now! starting in {i} s");
                Console.CursorLeft = 0;
                Thread.Sleep(1000);
            }
            Console.WriteLine();

            // start render:
            // begin stopwatch
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // every dotmap
            int frame = 1;
            foreach (DotMap dots in dotMaps)
            {
                // set progress listener
                dots.Progress += pog =>
                {
                    Console.Title = $"ESC to stop | Frame ${frame}: {pog * 100:0}% done | {sw.Elapsed.TotalSeconds:0}s elapsed";
                };

                // draw the frame
                // use fill on the first frame
                dots.DrawTo(pain, frame == 1, mode: DotMap.DrawMode.Polys);
                frame++;
            }


            // we are done
            sw.Stop();
            Console.WriteLine($"finished {dotMaps.Count} frames after {sw.Elapsed} ({sw.Elapsed.TotalSeconds:0.0} s)");
        }

        static Bitmap GetBitmap(string msg)
        {
            Console.Write(msg);
            string path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Console.WriteLine("cannot find file!");
                return null;
            }
            return new Bitmap(path);
        }

    }
}
