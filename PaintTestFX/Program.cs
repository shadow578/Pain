using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            // enable cursor logs
            Cursor.ENABLE_CW = true;

            // init pain
            PainInterface pain = new PainInterface
            {
                Bounds = safeZone
            };

            // testing path
            int pathW = 2100;
            int pathH = 2100;
            string path = "m 365.6635,1182.9658 " +
                "c 51.66494,-73.0315 86.47605,-159.2902 156.1408,-218.9562 50.95896,-45.73833 136.36806,-43.78036 182.19016,-83.31128 35.9929,-125.27918 50.5912,-268.39157 148.5551,-364.68132 37.3883,-53.26985 149.90004,-60.99986 165.25454,13.47588 1.3092,92.11619 118.8439,-114.9792 133.0501,2.88625 -27.4151,104.37154 -50.5276,222.60832 -145.2266,288.52867 -33.80814,40.68442 -54.37754,89.23399 -54.84574,142.16412 -11.6296,70.68358 -51.3935,183.71768 -10.6175,228.88798 77.68644,54.7586 156.18754,-13.8149 236.71324,-16.173 95.3664,9.4979 191.8171,20.2341 279.0473,62.7909 96.4559,36.9594 169.0325,116.1716 241.1178,187.1772 81.0331,105.8546 148.6775,230.469 159.6117,365.609 -8.8281,99.6044 10.8452,201.8913 -11.5887,299.6687 -21.5674,38.6292 -73.7645,70.3896 -17.6486,98.1256 12.8486,68.5744 -119.7226,72.0068 -173.4988,85.0974 -85.2386,7.1048 -174.5309,31.6776 -262.464,13.6641 -63.3246,-28.4406 -81.335,50.4412 -153.1551,28.7959 -98.0322,-3.6503 -198.9625,12.6247 -294.73544,-13.8438 -82.0974,-4.7828 -160.0143,-20.1467 -243.1848,-14.6255 -58.7809,8.7915 -168.2033,-48.3968 -64.3665,-89.3163 69.5185,-17.9667 168.0467,-45.8861 169.2987,-131.7275 31.0432,-103.6157 -58.2285,-184.7766 -124.822,-252.7191 -56.9064,-72.0115 -113.484,-150.3105 -138.8356,-238.7615 -39.08172,-58.9314 -5.8958,-191.7569 -48.54019,-212.871 -54.64461,-19.3752 -75.66076,-72.6286 -131.03927,-93.7386 -10.01712,-28.2877 0.66757,-57.6773 3.5894,-86.1466 " +
                "z";

            pain.DrawPath(path, pathW, pathH);






            // testing poly
            /**
            PointF[] poly = {
                p(0, 0),
                p(0.2, 0.5),
                p(0.5, 0.1),
                p(0.8, 0.9),
                p(1,1)
            };

            PainInterface pain = new PainInterface
            {
                Bounds = safeZone
            };
            pain.DrawPolygon(poly);
            */


            Console.WriteLine("done!");
            Console.ReadLine();
        }


        static PointF p(double x, double y)
        {
            return new PointF((float)x, (float)y);
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
