using Pain.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pain.Draw
{
    /// <summary>
    /// a map of colored dots that can be drawn to a <see cref="IDrawTarget"/>
    /// </summary>
    public class DotMap
    {
        #region Init
        /// <summary>
        /// create a dotmap from a bitmap
        /// </summary>
        /// <param name="bmp">the bitmap</param>
        /// <param name="canvasSize">the size of the canvas</param>
        /// <param name="canvasScale">scale of the canvas. 0.5 would create a dotmap of half the dots horizontally and verticalls</param>
        /// <param name="colorBitsToDrop">color bits to drop from every color component, range 0-8</param>
        /// <returns>the dotmap</returns>
        public static DotMap Of(Bitmap bmp, Size canvasSize, float canvasScale, int colorBitsToDrop = 0)
        {
            // scale bitmap down
            int scaledW = (int)Math.Floor(canvasSize.Width * canvasScale);
            int scaledH = (int)Math.Floor(canvasSize.Height * canvasScale);
            Bitmap bmpScaled = new Bitmap(scaledW, scaledH);

            // get graphics from the scaled bitmap
            // and draw bitmap scaled
            using (Graphics g = Graphics.FromImage(bmpScaled))
                g.DrawImage(bmp, 0, 0, scaledW, scaledH);

            // create dotmap
            Dictionary<Color, List<PointF>> dots = new Dictionary<Color, List<PointF>>();
            for (int x = 0; x < scaledW; x++)
                for (int y = 0; y < scaledH; y++)
                {
                    // get pixel
                    Color px = bmpScaled.GetPixel(x, y);

                    // drop bits
                    colorBitsToDrop = Math.Clamp(colorBitsToDrop, 0, 8);
                    if (colorBitsToDrop > 0)
                    {
                        byte r = (byte)((px.R >> colorBitsToDrop) << colorBitsToDrop);
                        byte g = (byte)((px.G >> colorBitsToDrop) << colorBitsToDrop);
                        byte b = (byte)((px.B >> colorBitsToDrop) << colorBitsToDrop);
                        px = Color.FromArgb(r, g, b);
                    }

                    // get position and normalize
                    PointF pos = new PointF(
                        (float)x / scaledW,
                        (float)y / scaledH);

                    // create entry for this color if needed
                    if (!dots.ContainsKey(px))
                        dots.Add(px, new List<PointF>());

                    // insert into the list
                    dots[px].Add(pos);
                }

            return new DotMap(dots);
        }

        private DotMap()
        {

        }
        #endregion

        /// <summary>
        /// The total number of dots in this dotmap
        /// </summary>
        public int TotalDots
        {
            get
            {
                int cnt = 0;
                foreach (Color c in dotMap.Keys)
                    cnt += dotMap[c].Count;

                return cnt;
            }
        }

        /// <summary>
        /// the total number of unique colors
        /// </summary>
        public int TotalColors
        {
            get
            {
                return dotMap.Keys.Count;
            }
        }

        /// <summary>
        /// internal dotmap, grouped by color
        /// </summary>
        private readonly Dictionary<Color, List<PointF>> dotMap;

        /// <summary>
        /// progress listener
        /// value is range 0.0 - 1.0
        /// </summary>
        public event Action<double> Progress;

        private DotMap(Dictionary<Color, List<PointF>> dotsDict)
        {
            dotMap = dotsDict;
        }

        #region Optimize
        /// <summary>
        /// optimize the dotmap to use less unique colors.
        /// Uses <see cref="ColorComparisions.Euclidean(Color, Color)"/> comparison function
        /// </summary>
        /// <param name="maxUniqueColors">the max number of unique colors to use</param>
        /// <returns>the dotmap instance reference</returns>
        public DotMap Optimize(int maxUniqueColors)
        {
            return Optimize(maxUniqueColors, ColorComparisions.Euclidean);
        }

        /// <summary>
        /// optimize the dotmap to use less unique colors
        /// </summary>
        /// <param name="maxUniqueColors">the max number of unique colors to use</param>
        /// <param name="colorComparison">function to compare colors</param>
        /// <returns>the dotmap instance reference</returns>
        public DotMap Optimize(int maxUniqueColors, Func<Color, Color, float> colorComparison)
        {
            if (colorComparison == null)
                throw new ArgumentNullException("color comparision function is required");

            // get keys ordered by dot count 
            // so that .First() has the most dots
            List<Color> colorsOrdered = dotMap.Keys.ToList().OrderByDescending(cx => dotMap[cx].Count).ToList();

            // map the least used color to the nearest matching color
            // until we are below the maximum unique color count
            while (colorsOrdered.Count > maxUniqueColors)
            {
                // remove this color from the list
                Color colorToMap = colorsOrdered.Last();
                colorsOrdered.Remove(colorToMap);

                // find the nearest color
                Color nearestColor = colorsOrdered.First();
                float nearestDistance = float.MaxValue;
                foreach (Color c in colorsOrdered)
                {
                    // calculate and compare distance
                    float distance = colorComparison.Invoke(colorToMap, c);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestColor = c;
                    }
                }

                // add all dots to the nearest color's dot list
                dotMap[nearestColor].AddRange(dotMap[colorToMap]);

                // remove mapped color from dict
                dotMap[colorToMap].Clear();
                dotMap.Remove(colorToMap);
            }

            return this;
        }
        #endregion

        #region Sort/Shuffle

        /// <summary>
        /// shuffle the dots of each color so that there is no left- to- right "seam" visible.
        /// This may slow down drawing
        /// </summary>
        /// <returns>instance reference</returns>
        public DotMap Shuffle()
        {
            Random rnd = new Random();

            // every color
            foreach (Color c in dotMap.Keys)
            {
                // shuffle list 
                // based on https://stackoverflow.com/a/1262619
                List<PointF> dots = dotMap[c];
                int n = dots.Count;
                while(n > 1)
                {
                    n--;
                    int k = rnd.Next(n + 1);
                    PointF x = dots[k];
                    dots[k] = dots[n];
                    dots[n] = x;
                }
            }

            return this;
        }

        /// <summary>
        /// Sort the Dots of each color so that dots are drawn left- to- right
        /// </summary>
        /// <returns>instance reference</returns>
        public DotMap Sort()
        {
            // every color
            foreach (Color c in dotMap.Keys)
            {
                dotMap[c].Sort((d1, d2) =>
                {
                    // Y 0 ==> Y 1
                    if (d1.X.Equals(d2.X))
                        return d1.Y.CompareTo(d2.Y);

                    // X 0 ==> X 1
                    return d1.X.CompareTo(d2.X);
                });

            }

            return this;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draw the dotmap to a target
        /// </summary>
        /// <param name="target">where to draw to</param>
        /// <param name="firstColorFill">should the first color be drawn using flood- fill? (canvas has to be empty for this to work well)</param>
        /// <param name="strokeSize">the stroke size, 0-1</param>
        public void DrawTo(IDrawTarget target, bool firstColorFill, float strokeSize = 1)
        {
            strokeSize = Math.Clamp(strokeSize, 0, 1);
            double totalDots = TotalDots;
            double dotsDrawn = 0;
            bool useFill = !firstColorFill;

            // set stroke size
            target.SetStroke(strokeSize);

            // draw colors in batches
            foreach (Color c in dotMap.Keys.OrderByDescending(cx => dotMap[cx].Count))
                if (dotMap[c].Count > 0)
                {
                    // select the color
                    target.SetPrimaryColor(c);

                    // fill the first color using bucket tool if enabled
                    if (useFill)
                    {
                        // bucket in the center of the image
                        target.Fill(new PointF(.5f, .5f));

                        // add dots drawn to total
                        dotsDrawn += dotMap[c].Count;

                        useFill = false;
                        continue;
                    }

                    // draw all dots
                    dotsDrawn += DrawUsingDots(target, dotMap[c], totalDots, dotsDrawn);
                    ReportProgress(dotsDrawn / totalDots);
                }
        }

        /// <summary>
        /// Draw a list of dots dot-by-dot
        /// </summary>
        /// <param name="target">draw target</param>
        /// <param name="dots">the dot list</param>
        /// <param name="totalDots">the total number of dots (progress reporting)</param>
        /// <param name="dotsDrawn">the total number of dots drawn (progress reporting)</param>
        /// <returns>the number of dots drawn</returns>
        private int DrawUsingDots(IDrawTarget target, List<PointF> dots, double totalDots, double dotsDrawn)
        {
            // for every dot
            foreach (PointF dot in dots)
            {
                // draw the dot
                target.DrawDot(dot);

                // report progress
                dotsDrawn++;
                ReportProgress(dotsDrawn / totalDots);
            }

            return dots.Count;
        }
        #endregion

        /// <summary>
        /// report the progress to the progress listener
        /// </summary>
        /// <param name="progress">the progress to report, 0.0 - 1.0</param>
        private void ReportProgress(double progress)
        {
            if (Progress != null)
                Progress.Invoke(Math.Clamp(progress, 0, 1));
        }
    }
}
