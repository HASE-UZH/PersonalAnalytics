using System;

namespace WindowRecommender.Data
{
    internal struct WindowEventRecord
    {
        internal readonly string WindowHandle;
        internal readonly string WindowTitle;
        internal readonly string ProcessName;
        internal readonly int ZIndex;
        internal readonly int Rank;
        internal readonly double Score;

        internal WindowEventRecord(IntPtr windowHandle, string windowTitle, string processName, int zIndex)
        {
            WindowHandle = windowHandle.ToString();
            WindowTitle = windowTitle;
            ProcessName = processName;
            ZIndex = zIndex;
            Rank = -1;
            Score = -1;
        }

        internal WindowEventRecord(IntPtr windowHandle, string windowTitle, string processName, int zIndex, int rank, double score) : this(windowHandle, windowTitle, processName, zIndex)
        {
            Rank = rank;
            Score = score;
        }
    }
}