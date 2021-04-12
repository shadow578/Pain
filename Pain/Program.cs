using Pain;
using System;
using System.Drawing;
using System.Threading;

namespace PaintTestFX
{
    class Program
    {
        static void Main(string[] args)
        {
            //saf: 2173/403 --> 2804/675
            Thread.Sleep(5000);
            Cursor.MoveTo(new Point(2173, 403));
            Cursor.LMBDown();
            Thread.Sleep(50);
            Cursor.MoveTo(new Point(2804, 675));
            Cursor.LMBUp();


            // setup safe bounds
            Rectangle safeZone = GetSafeZone();

            // get image to draw
            //Console.Write("Enter image path to draw:");
            //string bmpPath = Console.ReadLine();
            //Bitmap bmp = new Bitmap(bmpPath);

            // scale down bitmap
            //bmp = ScaleBitmap(bmp, safeZone.Size);


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
                MoveDelay = 1000
            };

            // draw bitmap
            //pain.DrawBitmapWithDots(bmp, 2, 5);
            pain.SetColor(255, 0, 0);
            pain.SetStrokeWidth(2);
            pain.EnterPaintMode();
            pain.DrawPolygon(verticies: new PointF[] { new PointF(0, 0), new PointF(0.5f, 0.5f), new PointF(0, 1), new PointF(1, 1) });

            Console.WriteLine("done!");
            Console.ReadLine();
        }

        static Bitmap ScaleBitmap(Bitmap input, Size to)
        {
            // create new bitmap
            Bitmap newBmp = new Bitmap(to.Width, to.Height);

            // get graphics for new bitmap
            using(Graphics g = Graphics.FromImage(newBmp))
            {
                // hig kwaliti
                // as if this matters :P
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // draw the image scaled
                g.DrawImage(input, 0, 0, to.Width, to.Height);

            }

            return newBmp;
        }

        /// <summary>
        /// Get the safe bounds from the user
        /// </summary>
        /// <returns>the safe bounds</returns>
        static Rectangle GetSafeZone()
        {
            // wait for user
            Console.WriteLine("Press <SPACE> to start setting up the safe zone");
            while (!Util.IsDown(VK.Space))
                Thread.Sleep(500);

            Console.WriteLine("OK, release now");
            while (Util.IsDown(VK.Space))
                Thread.Sleep(500);

            // top left
            Console.WriteLine("Move the cursor to the TOP LEFT corner of your safe zone and click LMB");
            while (!Util.IsDown(VK.LeftButton))
            {
                Point p = Cursor.GetCursorPos();
                Console.Write($"START_POS: {p.X} / {p.Y}      ");
                Console.CursorLeft = 0;
                Thread.Sleep(100);
            }

            Point topLeft = Cursor.GetCursorPos();

            Console.WriteLine("\nOk, release now");
            while(Util.IsDown(VK.LeftButton))
                Thread.Sleep(100);

            // bottom right
            Console.WriteLine("Move the cursor to the BOTTOM RIGHT corner of your safe zone and click LMB");
            while (!Util.IsDown(VK.LeftButton))
            {
                Point p = Cursor.GetCursorPos();
                Console.Write($"END_POS: {p.X} / {p.Y}      ");
                Console.CursorLeft = 0;
                Thread.Sleep(100);
            }

            Point bottomRight = Cursor.GetCursorPos();

            Console.WriteLine("\nOk, release now");
            while (Util.IsDown(VK.LeftButton))
                Thread.Sleep(100);

            // get rect
            Rectangle rect = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            Console.WriteLine($"Safe zone is XYWH: {rect.X} / {rect.Y} / {rect.Width} / {rect.Height}");

            return rect;
        }
    }
}
