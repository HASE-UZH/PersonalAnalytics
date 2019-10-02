using System;
using System.Runtime.InteropServices;

namespace WindowRecommender.Native
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-rect
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public readonly int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public readonly int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public readonly int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public readonly int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        // ReSharper restore InconsistentNaming
    }
}
