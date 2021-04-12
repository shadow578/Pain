namespace Pain.Interface.MSPaint
{
    /// <summary>
    /// ms paint menu key abstractions
    /// </summary>
    public interface IPaintControlCommands
    {
        /// <summary>
        /// enable command logging
        /// </summary>
        bool LogCommands { get; set; }

        /// <summary>
        /// select the primary color slot
        /// </summary>
        void SelectPrimaryColor();

        /// <summary>
        /// select the secondary color slot
        /// </summary>
        void SelectSecondaryColor();

        /// <summary>
        /// set the color in the current slot
        /// </summary>
        /// <param name="r">red value</param>
        /// <param name="g">green value</param>
        /// <param name="b">blue value</param>
        void SetColorRGB(byte r, byte g, byte b);

        /// <summary>
        /// enter rectangular selection mode
        /// </summary>
        void RectangleSelectionMode();

        /// <summary>
        /// delete the current selection
        /// </summary>
        void DeleteSelection();

        /// <summary>
        /// set the stroke width
        /// </summary>
        /// <param name="val">the stroke width. range 0 - 3</param>
        void SetStrokeWidth(byte val);

        /// <summary>
        /// select the paint brush tool.
        /// the paint brush tool draws in the Primary color
        /// </summary>
        void SelectPaintBrush();

        /// <summary>
        /// select the bucket tool. 
        /// the bucket tool draws in the Primary color
        /// </summary>
        void SelectBucketTool();

        /// <summary>
        /// select the pen tool. 
        /// the pen tool draws in the Primary color
        /// </summary>
        void SelectPenTool();

        /// <summary>
        /// select the eraser tool.
        /// the eraser tool draws in the secondary color
        /// </summary>
        void SelectEraserTool();
    }
}
