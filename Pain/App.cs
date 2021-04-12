using Pain.Draw;
using Pain.Driver;
using Pain.Interface.MSPaint;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Pain
{
    public static class App
    {

        public static void Main()
        {
            // load image to draw
            Console.Write("Enter Image path to draw: ");
            string path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Console.WriteLine("cannot find file!");
                return;
            }
            Bitmap imageToDraw = new Bitmap(path);

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

            // create dotmap
            Console.WriteLine("preparing dotmap...");
            DotMap dotMap = DotMap.Of(imageToDraw, pain.Bounds.Size, .2f, 2)
                .Optimize(128, ColorComparisions.DeltaE);

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

            // start drawing and stop time for draw
            dotMap.Progress += pog =>
            {
                Console.Title = $"Press ESC to stop | {pog * 100:0.0}% done";
            };

            Stopwatch sw = new Stopwatch();
            sw.Start();

            dotMap.DrawTo(pain, false);

            sw.Stop();
            Console.WriteLine($"Done after {sw.Elapsed} ({sw.Elapsed.TotalSeconds:0.0} seconds)");
        }

    }
}
