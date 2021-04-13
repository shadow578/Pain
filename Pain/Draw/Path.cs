using Pain.Interface;
using System.Drawing;

namespace Pain.Draw
{
    /// <summary>
    /// a path drawn in a single color that can be drawn to a <see cref="IDrawTarget"/>
    /// </summary>
    public class Path
    {
        #region Init
        /// <summary>
        /// create a path for the outline of a normal rectangle
        /// </summary>
        /// <param name="rectangle">the rectangle, ranges 0.0-1.0</param>
        /// <returns>the path</returns>
        public static Path Of(RectangleF rectangle)
        {
            float rectRight = rectangle.X + rectangle.Width;
            float rectBottom = rectangle.Y + rectangle.Height;

            return Of(rectangle.Location,
                new PointF(rectangle.X, rectBottom),
                new PointF(rectRight, rectBottom),
                new PointF(rectRight, rectangle.Y),
                rectangle.Location);
        }

        /// <summary>
        /// create a path from a list of normal points
        /// </summary>
        /// <param name="points">the list of points on this path, range 0.0-1.0</param>
        /// <returns>the path</returns>
        public static Path Of(params PointF[] points)
        {
            return new Path
            {
                Vertices = points
            };
        }

        private Path()
        {

        }
        #endregion

        /// <summary>
        /// the list of vertices of this path
        /// </summary>
        private PointF[] Vertices { get; set; }

        /// <summary>
        /// the color to draw in
        /// </summary>
        private Color color = Color.Black;

        /// <summary>
        /// set the color in which this path is drawn.
        /// if not set, defaults to black
        /// </summary>
        /// <param name="color">the color</param>
        /// <returns>instance reference</returns>
        public Path SetColor(Color color)
        {
            this.color = color;
            return this;
        }

        /// <summary>
        /// draw the path to a target
        /// </summary>
        /// <param name="target">the draw target</param>
        /// <param name="strokeSize">the stroke size, 0-1</param>
        public void DrawTo(IDrawTarget target, float strokeSize = 1)
        {
            // set the color and stroke
            target.SetPrimaryColor(color);
            target.SetStroke(strokeSize);

            // draw the path
            target.DrawPoly(Vertices);
        }

    }
}
