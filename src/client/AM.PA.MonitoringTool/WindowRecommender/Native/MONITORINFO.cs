using System.Runtime.InteropServices;

namespace WindowRecommender.Native
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The MONITORINFO structure contains information about a display monitor.
    /// The GetMonitorInfo function stores information into a MONITORINFO structure or a MONITORINFOEX structure.
    /// The MONITORINFO structure is a subset of the MONITORINFOEX structure. The MONITORINFOEX structure adds a string
    /// member to contain a name for the display monitor.
    /// </summary>
    /// https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-tagmonitorinfo
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    // ReSharper disable once InconsistentNaming
    internal class MONITORINFO
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// Set this member to sizeof(MONITORINFO) before calling the <see cref="NativeMethods.GetMonitorInfo"/>
        /// function. Doing so lets the function determine the type of structure you are passing to it.
        /// </summary>
        public int cbSize;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be
        /// negative values.
        /// </summary>
        public RECT rcMonitor;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by
        /// applications, expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an
        /// application on the monitor. The rest of the area in rcMonitor contains system windows such as the task bar
        /// and side bars. Note that if the monitor is not the primary display monitor, some of the rectangle's
        /// coordinates may be negative values.
        /// </summary>
        public RECT rcWork;

        /// <summary>
        /// A set of flags that represent attributes of the display monitor.
        /// The following flag is defined.
        ///   1 : MONITORINFOF_PRIMARY : This is the primary display monitor.
        /// </summary>
        public uint dwFlags;
    }

    // ReSharper restore InconsistentNaming
}
