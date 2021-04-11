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
            int pathW = 2100 + 350;
            int pathH = 2100 + 350;
            //string path = "m 365.6635,1182.9658 c 51.66494,-73.0315 86.47605,-159.2902 156.1408,-218.9562 50.95896,-45.73833 136.36806,-43.78036 182.19016,-83.31128 35.9929,-125.27918 50.5912,-268.39157 148.5551,-364.68132 37.3883,-53.26985 149.90004,-60.99986 165.25454,13.47588 1.3092,92.11619 118.8439,-114.9792 133.0501,2.88625 -27.4151,104.37154 -50.5276,222.60832 -145.2266,288.52867 -33.80814,40.68442 -54.37754,89.23399 -54.84574,142.16412 -11.6296,70.68358 -51.3935,183.71768 -10.6175,228.88798 77.68644,54.7586 156.18754,-13.8149 236.71324,-16.173 95.3664,9.4979 191.8171,20.2341 279.0473,62.7909 96.4559,36.9594 169.0325,116.1716 241.1178,187.1772 81.0331,105.8546 148.6775,230.469 159.6117,365.609 -8.8281,99.6044 10.8452,201.8913 -11.5887,299.6687 -21.5674,38.6292 -73.7645,70.3896 -17.6486,98.1256 12.8486,68.5744 -119.7226,72.0068 -173.4988,85.0974 -85.2386,7.1048 -174.5309,31.6776 -262.464,13.6641 -63.3246,-28.4406 -81.335,50.4412 -153.1551,28.7959 -98.0322,-3.6503 -198.9625,12.6247 -294.73544,-13.8438 -82.0974,-4.7828 -160.0143,-20.1467 -243.1848,-14.6255 -58.7809,8.7915 -168.2033,-48.3968 -64.3665,-89.3163 69.5185,-17.9667 168.0467,-45.8861 169.2987,-131.7275 31.0432,-103.6157 -58.2285,-184.7766 -124.822,-252.7191 -56.9064,-72.0115 -113.484,-150.3105 -138.8356,-238.7615 -39.08172,-58.9314 -5.8958,-191.7569 -48.54019,-212.871 -54.64461,-19.3752 -75.66076,-72.6286 -131.03927,-93.7386 -10.01712,-28.2877 0.66757,-57.6773 3.5894,-86.1466 z";

            string path = @"<Path xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" Name=""path7"" Fill=""#000000"">
      <Path.Data>
        <PathGeometry Figures=""M433.109 23.694c-3.614-3.612-7.898-5.424-12.848-5.424c-4.948 0-9.226 1.812-12.847 5.424l-37.113 36.835             c-20.365-19.226-43.684-34.123-69.948-44.684C274.091 5.283 247.056 0.003 219.266 0.003c-52.344 0-98.022 15.843-137.042 47.536             C43.203 79.228 17.509 120.574 5.137 171.587v1.997c0 2.474 0.903 4.617 2.712 6.423c1.809 1.809 3.949 2.712 6.423 2.712h56.814             c4.189 0 7.042-2.19 8.566-6.565c7.993-19.032 13.035-30.166 15.131-33.403c13.322-21.698 31.023-38.734 53.103-51.106             c22.082-12.371 45.873-18.559 71.376-18.559c38.261 0 71.473 13.039 99.645 39.115l-39.406 39.397             c-3.607 3.617-5.421 7.902-5.421 12.851c0 4.948 1.813 9.231 5.421 12.847c3.621 3.617 7.905 5.424 12.854 5.424h127.906             c4.949 0 9.233-1.807 12.848-5.424c3.613-3.616 5.42-7.898 5.42-12.847V36.542C438.529 31.593 436.733 27.312 433.109 23.694z"" FillRule=""NonZero""/>
      </Path.Data>
    </Path>
    <Path xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" Name=""path9"" Fill=""#000000"">
      <Path.Data>
        <PathGeometry Figures=""M422.253 255.813h-54.816c-4.188 0-7.043 2.187-8.562 6.566c-7.99 19.034-13.038 30.163-15.129 33.4             c-13.326 21.693-31.028 38.735-53.102 51.106c-22.083 12.375-45.874 18.556-71.378 18.556c-18.461 0-36.259-3.423-53.387-10.273             c-17.13-6.858-32.454-16.567-45.966-29.13l39.115-39.112c3.615-3.613 5.424-7.901 5.424-12.847c0-4.948-1.809-9.236-5.424-12.847             c-3.617-3.62-7.898-5.431-12.847-5.431H18.274c-4.952 0-9.235 1.811-12.851 5.431C1.807 264.844 0 269.132 0 274.08v127.907             c0 4.945 1.807 9.232 5.424 12.847c3.619 3.61 7.902 5.428 12.851 5.428c4.948 0 9.229-1.817 12.847-5.428l36.829-36.833             c20.367 19.41 43.542 34.355 69.523 44.823c25.981 10.472 52.866 15.701 80.653 15.701c52.155 0 97.643-15.845 136.471-47.534             c38.828-31.688 64.333-73.042 76.52-124.05c0.191-0.38 0.281-1.047 0.281-1.995c0-2.478-0.907-4.612-2.715-6.427             C426.874 256.72 424.731 255.813 422.253 255.813z"" FillRule=""NonZero""/>
      </Path.Data>
    </Path>";

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
