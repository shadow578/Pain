using System.Drawing;

namespace Pain.Interface
{
    /// <summary>
    /// interface for a painting program
    /// </summary>
    public interface IDrawTarget
    {
        /// <summary>
        /// set the current primary color.
        /// does not select the primary color
        /// </summary>
        /// <param name="color">the color to set</param>
        void SetPrimaryColor(Color color);

        /// <summary>
        /// set the current secondary color.
        /// does not select the secondary color
        /// </summary>
        /// <param name="color">the color to set</param>
        void SetSecondaryColor(Color color);

        /// <summary>
        /// select the current primary color
        /// </summary>
        void SelectPrimaryColor();

        /// <summary>
        /// select the current secondary color
        /// </summary>
        void SelectSecondaryColor();

        /// <summary>
        /// clear the given rectangle
        /// </summary>
        /// <param name="rect">the rectangle to clear. the rectangle is normalized, so all values are in range 0.0 - 1.0</param>
        void Clear(RectangleF rect);

        /// <summary>
        /// set the stroke width
        /// </summary>
        /// <param name="width">stroke width, range 0.0 - 1.0</param>
        void SetStroke(float width);

        /// <summary>
        /// flood- fill using the currently selected color, starting at the given position
        /// </summary>
        /// <param name="at">the position, range 0.0 - 1.0</param>
        void Fill(PointF at);

        /// <summary>
        /// draw a dot in the currently selected color and stroke
        /// </summary>
        /// <param name="at">where to draw the dot, range 0.0 - 1.0</param>
        void DrawDot(PointF at);

        /// <summary>
        /// draw a polygon with the currently selected color and stroke.
        /// does NOT automatically close the polygon (return to the start)
        /// </summary>
        /// <param name="vertices">the vertices of the polygon</param>
        void DrawPoly(params PointF[] vertices);
    }
}
