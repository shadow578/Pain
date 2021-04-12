using Pain.Driver;
using System;

namespace Pain.Interface.MSPaint
{
    /// <summary>
    /// abstraction for german paint in Windows 10 1903+
    /// </summary>
    public class GermanPaintControlCommands : IPaintControlCommands
    {
        /// <summary>
        /// enable command logging
        /// </summary>
        public bool LogCommands { get; set; } = false;

        /// <summary>
        /// select the primary color slot
        /// 
        /// ALT - R - 1
        /// </summary>
        public void SelectPrimaryColor()
        {
            Log("SelectPrimaryColor");
            Keyboard.Type(VK.Menu, VK.R, VK.N1);
        }

        /// <summary>
        /// select the secondary color slot
        /// 
        /// ALT - R - 2
        /// </summary>
        public void SelectSecondaryColor()
        {
            Log("SelectSecondaryColor");
            Keyboard.Type(VK.Menu, VK.R, VK.N2);
        }

        /// <summary>
        /// set the color in the current slot
        /// 
        /// ALT - R - EC to open Dialog, then
        /// 7x Tab to reach RED, then
        /// 1x Tab to reach GREEN, then
        /// 1x Tab to reach BLUE, then
        /// ENTER to confirm
        /// </summary>
        /// <param name="r">red value</param>
        /// <param name="g">green value</param>
        /// <param name="b">blue value</param>
        public void SetColorRGB(byte r, byte g, byte b)
        {
            Log($"SetColorRGB R: {r} G: {g} B: {b}");
            Keyboard.Type(VK.Menu, VK.R, VK.E, VK.C,
                VK.Tab, VK.Tab, VK.Tab, VK.Tab, VK.Tab, VK.Tab, VK.Tab,
                r,
                VK.Tab,
                g,
                VK.Tab,
                b,
                VK.Return);
        }

        /// <summary>
        /// enter rectangular selection mode
        /// 
        /// ALT - R - AU - R
        /// </summary>
        public void RectangleSelectionMode()
        {
            Log("RectangleSelectionMode");
            Keyboard.Type(VK.Menu, VK.R, VK.A, VK.U, VK.R);
        }

        /// <summary>
        /// delete the current selection
        /// 
        /// ALT - R - AU - S
        /// </summary>
        public void DeleteSelection()
        {
            Log("DeleteSelection");
            Keyboard.Type(VK.Menu, VK.R, VK.A, VK.U, VK.S);
        }

        /// <summary>
        /// set the stroke width
        /// 
        /// ALT - R - SZ, 
        /// then down arrow 0 to 3 time,
        /// then ENTER
        /// </summary>
        /// <param name="val">the stroke width. range 0 - 3</param>
        public void SetStrokeWidth(int val)
        {
            // clamp to 0-3
            val = Math.Clamp(val, 0, 3);

            Log($"SetStrokeWidth {val}");
            Keyboard.Type(VK.Menu, VK.R, VK.S, VK.Z);
            for (int i = 0; i < val; i++)
                Keyboard.Type(VK.Down);
            Keyboard.Type(VK.Return);
        }

        /// <summary>
        /// select the paint brush tool
        /// the paint brush tool draws in the Primary color
        /// 
        /// ALT - R - P1 - ENTER
        /// </summary>
        public void SelectPaintBrush()
        {
            Log("SelectPaintBrush");
            Keyboard.Type(VK.Menu, VK.R, VK.P, VK.N1, VK.Return);
        }

        /// <summary>
        /// select the bucket tool
        /// the bucket tool draws in the Primary color
        /// 
        /// ALT - R - FF
        /// </summary>
        public void SelectBucketTool()
        {
            Log("SelectBucketTool");
            Keyboard.Type(VK.Menu, VK.R, VK.F, VK.F);
        }

        /// <summary>
        /// select the pen tool
        /// the pen tool draws in the Primary color
        /// 
        /// ALT - R - ST
        /// </summary>
        public void SelectPenTool()
        {
            Log("SelectPenTool");
            Keyboard.Type(VK.Menu, VK.R, VK.S, VK.T);
        }

        /// <summary>
        /// select the eraser tool.
        /// the eraser tool draws in the secondary color
        /// 
        /// ALT - R - RA
        /// </summary>
        public void SelectEraserTool()
        {
            Log("SelectEraserTool");
            Keyboard.Type(VK.Menu, VK.R, VK.R, VK.A);
        }

        #region Util
        /// <summary>
        /// log a message
        /// </summary>
        /// <param name="msg">message to write</param>
        void Log(string msg)
        {
            if (LogCommands)
                Console.WriteLine("[PaintCC_DE] " + msg);
        }
        #endregion
    }
}
