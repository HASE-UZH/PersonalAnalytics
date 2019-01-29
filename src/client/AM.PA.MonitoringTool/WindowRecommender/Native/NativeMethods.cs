﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace WindowRecommender.Native
{
    internal static class NativeMethods
    {

        internal static RECT GetPrimaryMonitorDimensions()
        {
            var monitorList = new List<IntPtr>();
            var listHandle = GCHandle.Alloc(monitorList);
            var monitorEnumProc = new MonitorEnumProc((IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                monitorList.Add(hMonitor);
                return true;
            });
            try
            {
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, monitorEnumProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                {
                    listHandle.Free();
                }
            }
            var primaryMonitorInfo = monitorList.Select(monitorHandle =>
            {
                var monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
                GetMonitorInfo(monitorHandle, ref monitorInfo);
                return monitorInfo;
            }).Single(monitorInfo => (monitorInfo.dwFlags & MONITORINFOF_PRIMARY) != 0);
            return primaryMonitorInfo.rcMonitor;
        }

        internal static RECT GetWindowRect(IntPtr window)
        {
            GetWindowRect(window, out var rect);
            return rect;
        }

        internal static IntPtr SetWinEventHook(uint eventConstant, Wineventproc winEventDelegate)
        {
            const uint dwFlags = WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS;
            return SetWinEventHook(eventConstant, eventConstant, IntPtr.Zero, winEventDelegate, 0, 0, dwFlags);
        }

        #region private extern
        // ReSharper disable IdentifierTypo
        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Local

        /// <summary>
        /// The EnumDisplayMonitors function enumerates display monitors (including invisible pseudo-monitors
        /// associated with the mirroring drivers) that intersect a region formed by the intersection of a specified
        /// clipping rectangle and the visible region of a device context.
        /// EnumDisplayMonitors calls an application-defined MonitorEnumProc callback function once for each monitor
        /// that is enumerated.
        /// Note that GetSystemMetrics (SM_CMONITORS) counts only the display monitors.
        /// </summary>
        /// <param name="hdc">A handle to a display device context that defines the visible region of interest. If this
        /// parameter is NULL, the hdcMonitor parameter passed to the callback function will be NULL, and the visible
        /// region of interest is the virtual screen that encompasses all the displays on the desktop.</param>
        /// <param name="lprcClip">A pointer to a RECT structure that specifies a clipping rectangle. The region of
        /// interest is the intersection of the clipping rectangle with the visible region specified by hdc.</param>
        /// <param name="lpfnEnum">A pointer to a MonitorEnumProc application-defined callback function.</param>
        /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the
        /// MonitorEnumProc function.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is
        /// zero.</returns>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-enumdisplaymonitors
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        /// <summary>
        /// The GetMonitorInfo function retrieves information about a display monitor.
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor of interest.</param>
        /// <param name="lpmi">A pointer to a MONITORINFO or MONITORINFOEX structure that receives information about
        /// the specified display monitor.
        /// You must set the cbSize member of the structure to sizeof(MONITORINFO) or sizeof(MONITORINFOEX) before
        /// calling the GetMonitorInfo function.Doing so lets the function determine the type of structure you are
        /// passing to it.
        /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. It has one additional member: a
        /// string that contains a name for the display monitor.Most applications have no use for a display monitor
        /// name, and so can save some bytes by using a MONITORINFO structure.</param>
        /// <returns> If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.</returns>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getmonitorinfoa
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in
        /// screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpRect">A pointer to a <see cref="RECT"/> structure that receives the screen coordinates of
        /// the upper-left and lower-right corners of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is
        /// zero.To get extended error information, call GetLastError.</returns>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getwindowrect
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Sets an event hook function for a range of events.
        /// </summary>
        /// <param name="eventMin">Specifies the event constant for the lowest event value in the range of events that
        /// are handled by the hook function.
        /// This parameter can be set to EVENT_MIN to indicate the lowest possible event value.</param>
        /// <param name="eventMax">Specifies the event constant for the highest event value in the range of events that
        /// are handled by the hook function. This parameter can be set to EVENT_MAX to indicate the highest possible
        /// event value. </param>
        /// <param name="hmodWinEventProc">Handle to the DLL that contains the hook function at lpfnWinEventProc, if
        /// the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter.
        /// If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this
        /// parameter is NULL. </param>
        /// <param name="lpfnWinEventProc">Pointer to the event hook function.</param>
        /// <param name="idProcess">Specifies the ID of the process from which the hook function receives events.
        /// Specify zero (0) to receive events from all processes on the current desktop.</param>
        /// <param name="idThread">Specifies the ID of the thread from which the hook function receives events. If this
        /// parameter is zero, the hook function is associated with all existing threads on the current desktop.</param>
        /// <param name="dwFlags">Flag values that specify the location of the hook function and of the events to be
        /// skipped.</param>
        /// <returns>If successful, returns an HWINEVENTHOOK value that identifies this event hook instance.
        /// Applications save this return value to use it with the UnhookWinEvent function.
        /// If unsuccessful, returns zero.</returns>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/Winuser/nf-winuser-setwineventhook
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, Wineventproc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        /// <summary>
        /// Removes an event hook function created by a previous call to <see cref="SetWinEventHook(uint, uint, IntPtr, Wineventproc, uint, uint, uint)" />.
        /// </summary>
        /// <param name="hWinEventHook">Handle to the event hook returned in the previous call to SetWinEventHook.</param>
        /// <returns>If successful, returns TRUE; otherwise, returns FALSE.</returns>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-unhookwinevent
        [DllImport("user32.dll")]
        internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        /// <summary>
        /// A MonitorEnumProc function is an application-defined callback function that is called by the
        /// EnumDisplayMonitors function.
        /// A value of type MONITORENUMPROC is a pointer to a MonitorEnumProc function.
        /// </summary>
        /// <param name="hMonitor">A handle to a display device context that defines the visible region of interest. If
        /// this parameter is NULL, the hdcMonitor parameter passed to the callback function will be NULL, and the
        /// visible region of interest is the virtual screen that encompasses all the displays on the desktop.</param>
        /// <param name="hdcMonitor">A pointer to a RECT structure that specifies a clipping rectangle. The region of
        /// interest is the intersection of the clipping rectangle with the visible region specified by hdc.</param>
        /// <param name="lprcMonitor">A pointer to a MonitorEnumProc application-defined callback function.</param>
        /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the
        /// MonitorEnumProc function.</param>
        /// <returns>To continue the enumeration, return TRUE. To stop the enumeration, return FALSE.</returns>
        /// https://docs.microsoft.com/en-ca/windows/desktop/api/winuser/nc-winuser-monitorenumproc
        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        /// <summary>
        /// An application-defined callback (or hook) function that the system calls in response to events generated by
        /// an accessible object. The hook function processes the event notifications as required. Clients install the
        /// hook function and request specific types of event notifications by calling SetWinEventHook.
        /// The WINEVENTPROC type defines a pointer to this callback function. WinEventProc is a placeholder for the
        /// application-defined function name.
        /// </summary>
        /// <param name="hWinEventHook">Handle to an event hook function. This value is returned by SetWinEventHook
        /// when the hook function is installed and is specific to each instance of the hook function.</param>
        /// <param name="event">Specifies the event that occurred. This value is one of the event constants.</param>
        /// <param name="hwnd">Handle to the window that generates the event, or NULL if no window is associated with
        /// the event. For example, the mouse pointer is not associated with a window.</param>
        /// <param name="idObject">Identifies the object associated with the event. This is one of the object
        /// identifiers or a custom object ID.</param>
        /// <param name="idChild">Identifies whether the event was triggered by an object or a child element of the
        /// object. If this value is CHILDID_SELF, the event was triggered by the object; otherwise, this value is the
        /// child ID of the element that triggered the event.</param>
        /// <param name="idEventThread"></param>
        /// <param name="dwmsEventTime">Specifies the time, in milliseconds, that the event was generated.</param>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc
        internal delegate void Wineventproc(IntPtr hWinEventHook, uint @event, IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

        /// <summary>
        /// Identifies whether the event was triggered by an object or a child element of the object.
        /// </summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc#parameters
        internal const int CHILDID_SELF = 0;

        /// <summary>
        /// A window object is about to be restored. This event is sent by the system, never by servers.
        /// </summary>
        /// https://docs.microsoft.com/en-ca/windows/desktop/WinAuto/event-constants#EVENT_SYSTEM_MINIMIZEEND
        internal const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

        /// <summary>
        /// The foreground window has changed. The system sends this event even if the foreground window has changed to
        /// another window in the same thread. Server applications never send this event.
        /// For this event, the <see cref="Wineventproc"/> callback function's hwnd parameter is the handle to the
        /// window that is in the foreground, the idObject parameter is <see cref="OBJID_WINDOW"/>, and the idChild
        /// parameter is <see cref="CHILDID_SELF"/>.
        /// </summary>
        /// https://docs.microsoft.com/en-ca/windows/desktop/WinAuto/event-constants#EVENT_SYSTEM_FOREGROUND
        internal const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

        /// <summary>
        /// This is the primary display monitor.
        /// </summary>
        private const int MONITORINFOF_PRIMARY = 0x00000001;

        /// <summary>
        /// The window itself rather than a child object.
        /// </summary>
        /// https://docs.microsoft.com/en-ca/windows/desktop/WinAuto/object-identifiers#OBJID_WINDOW
        internal const int OBJID_WINDOW = 0x00000000;

        /// <summary>
        /// The callback function is not mapped into the address space of the process that generates the event. Because
        /// the hook function is called across process boundaries, the system must queue events. Although this method
        /// is asynchronous, events are guaranteed to be in sequential order.
        /// </summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/Winuser/nf-winuser-setwineventhook#parameters
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        /// <summary>
        /// Prevents this instance of the hook from receiving the events that are generated by threads in this process.
        /// This flag does not prevent threads from generating events. 
        /// </summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/Winuser/nf-winuser-setwineventhook#parameters
        private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

        /// <summary>
        /// The MONITORINFO structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information into a MONITORINFO structure or a MONITORINFOEX structure.
        /// The MONITORINFO structure is a subset of the MONITORINFOEX structure. The MONITORINFOEX structure adds a
        /// string member to contain a name for the display monitor.
        /// </summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagmonitorinfo
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            /// <summary>
            /// The size, in bytes, of the structure.
            /// Set this member to <c>sizeof(MONITORINFO)</c> before calling the <see cref="GetMonitorInfo"/> function.
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int cbSize;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be
            /// negative values.
            /// </summary>
            public readonly RECT rcMonitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by
            /// applications, expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an
            /// application on the monitor. The rest of the area in rcMonitor contains system windows such as the task
            /// bar and side bars. Note that if the monitor is not the primary display monitor, some of the rectangle's
            /// coordinates may be negative values.
            /// </summary>
            public readonly RECT rcWork;

            /// <summary>
            /// A set of flags that represent attributes of the display monitor.
            /// </summary>
            public readonly uint dwFlags;
        }

        /// <summary>
        /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        /// <remarks>
        /// By convention, the right and bottom edges of the rectangle are normally considered exclusive. 
        /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle. 
        /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including, 
        /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
        /// </remarks>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/windef/ns-windef-rect
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
        }

        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
        // ReSharper restore MemberCanBePrivate.Local
        #endregion
    }
}
