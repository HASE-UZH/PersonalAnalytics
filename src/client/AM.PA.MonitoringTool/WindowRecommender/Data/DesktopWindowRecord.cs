using System;
using WindowRecommender.Graphics;

namespace WindowRecommender.Data
{
    internal struct DesktopWindowRecord
    {
        internal readonly string WindowHandle;
        internal readonly bool Hazed;
        internal readonly int ZIndex;
        internal readonly Rectangle Rectangle;

        public DesktopWindowRecord(IntPtr windowHandle, bool hazed, int zIndex, Rectangle rectangle)
        {
            WindowHandle = windowHandle.ToString();
            Hazed = hazed;
            ZIndex = zIndex;
            Rectangle = rectangle;
        }
    }
}