using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PaintTestFX
{
    class Program
    {
        static void Main(string[] args)
        {
            // wait for user
            Console.WriteLine("Enter to start setup");
            Console.ReadLine();

            // setup safe bounds
            Rectangle safeZone = GetSafeZone();

            // wait for user
            Console.WriteLine("Ready. Enter to Start.");
            Console.ReadLine();

            // tell user to switch to paint
            for (int i = 5; i > 0; i--)
            {
                Console.Write($"Switch to paint now! starting in {i} s");
                Console.CursorLeft = 0;
                Thread.Sleep(1000);
            }
            Console.WriteLine();
            Console.Title = "Press ESC to cancel";

            // enable cursor and utillogs
            Cursor.ENABLE_CW = true;
            Util.ENABLE_CW = true;

            // init pain
            PainInterface pain = new PainInterface
            {
                Bounds = safeZone,
                EnableCW = true,
                MoveDelay = 10
            };

            pain.SetStrokeWidth(2);
            pain.ClearBounds();
            pain.SetColor(255, 100, 90);
            pain.EnterPaintMode();


            Console.WriteLine("done!");
            Console.ReadLine();
        }

        /// <summary>
        /// Get the safe bounds from the user
        /// </summary>
        /// <returns>the safe bounds</returns>
        static Rectangle GetSafeZone()
        {
            // top left
            Console.WriteLine("Move the cursor to the TOP LEFT corner of your safe zone and click LMB");
            while (!Util.IsDown(Keys.LButton))
            {
                Point p = Cursor.GetCursorPos();
                Console.Write($"START_POS: {p.X} / {p.Y}      ");
                Console.CursorLeft = 0;
                Thread.Sleep(100);
            }

            Point topLeft = Cursor.GetCursorPos();

            Console.WriteLine("\nOk, release now");
            Thread.Sleep(2000);

            // bottom right
            Console.WriteLine("Move the cursor to the BOTTOM RIGHT corner of your safe zone and click LMB");
            while (!Util.IsDown(Keys.LButton))
            {
                Point p = Cursor.GetCursorPos();
                Console.Write($"END_POS: {p.X} / {p.Y}      ");
                Console.CursorLeft = 0;
                Thread.Sleep(100);
            }

            Point bottomRight = Cursor.GetCursorPos();

            Console.WriteLine("\nOk, release now");
            Thread.Sleep(2000);

            // get rect
            Rectangle rect = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            Console.WriteLine($"Safe zone is XYWH: {rect.X} / {rect.Y} / {rect.Width} / {rect.Height}");

            return rect;
        }
    }
}
