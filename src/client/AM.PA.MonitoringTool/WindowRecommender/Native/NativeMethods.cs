using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowRecommender.Native
{
    internal static class NativeMethods
    {
        internal static IEnumerable<RECT> GetMonitorRects()
        {
            var monitorList = new List<RECT>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                monitorList.Add(lprcMonitor);
                return true;
            }, IntPtr.Zero);
            return monitorList;
        }

        internal static IEnumerable<RECT> GetMonitorWorkingAreas()
        {
            var monitorList = new List<RECT>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
                GetMonitorInfo(hMonitor, monitorInfo);
                monitorList.Add(monitorInfo.rcWork);
                return true;
            }, IntPtr.Zero);
            return monitorList;
        }

        internal static List<IntPtr> GetOpenWindows()
        {
            var windowList = new List<IntPtr>();
            var shellWindowHandle = GetShellWindow();
            EnumWindows((windowHandle, lParam) =>
            {
                if (IsOpenWindow(windowHandle, shellWindowHandle))
                {
                    windowList.Add(windowHandle);
                }
                return true;
            }, IntPtr.Zero);
            return windowList;
        }

        internal static string GetProcessName(IntPtr windowHandle)
        {
            GetWindowThreadProcessId(windowHandle, out var processId);
            var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }

        internal static IntPtr GetWindowIconPointer(IntPtr windowHandle)
        {
            var result = SendMessageTimeout(windowHandle, Message.WM_GETICON, (uint)GetIconParameter.ICON_BIG, 0, SendMessageTimeoutFlag.SMTO_ABORTIFHUNG, 50, out var iconHandle);
            if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
                result = SendMessageTimeout(windowHandle, Message.WM_GETICON, (uint)GetIconParameter.ICON_SMALL, 0, SendMessageTimeoutFlag.SMTO_ABORTIFHUNG, 50, out iconHandle);
            if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
                result = SendMessageTimeout(windowHandle, Message.WM_GETICON, (uint)GetIconParameter.ICON_SMALL2, 0, SendMessageTimeoutFlag.SMTO_ABORTIFHUNG, 50, out iconHandle);
            if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
                iconHandle = new IntPtr(GetClassLong(windowHandle, GetClassLongOffset.GCL_HICON));

            return iconHandle;
        }

        internal static RECT GetWindowRectangle(IntPtr windowHandle)
        {
            GetWindowRect(windowHandle, out var rectangle);
            return rectangle;
        }

        internal static string GetWindowTitle(IntPtr windowHandle)
        {
            var numChars = GetWindowTextLength(windowHandle) + 1;
            var stringBuilder = new StringBuilder(numChars);
            GetWindowText(windowHandle, stringBuilder, numChars);
            return stringBuilder.ToString().Trim();
        }

        internal static bool IsOpenWindow(IntPtr windowHandle, IntPtr shellWindowHandle = default)
        {
            if (shellWindowHandle == default)
            {
                shellWindowHandle = GetShellWindow();
            }
            if (windowHandle == shellWindowHandle)
            {
                return false;
            }

            if (!IsWindowVisible(windowHandle))
            {
                return false;
            }

            if (IsIconic(windowHandle))
            {
                return false;
            }

            var windowStyles = GetWindowLong(windowHandle, GWL_STYLE);
            if ((windowStyles & WS_CAPTION) == 0)
            {
                return false;
            }

            var result = DwmGetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out var isCloaked, Marshal.SizeOf(typeof(int)));
            if (result != S_OK)
            {
                isCloaked = 0;
            }
            return isCloaked == 0;
        }

        internal static IntPtr SetWinEventHook(WinEventConstant eventConstant, Wineventproc winEventDelegate)
        {
            const uint dwFlags = WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS;
            return SetWinEventHook(eventConstant, eventConstant, IntPtr.Zero, winEventDelegate, 0, 0, dwFlags);
        }

        #region private extern
        // ReSharper disable IdentifierTypo
        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Local

        /// <summary>
        /// Delete objects like bitmaps, freeing all system resources.
        /// </summary>
        /// <param name="ho">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>Whether the function succeeds.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/wingdi/nf-wingdi-deleteobject
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr ho);

        /// <summary>
        /// Retrieves the current value of a specified attribute applied to a window.
        /// </summary>
        /// <param name="hwnd">The handle to the window from which the attribute data is retrieved.</param>
        /// <param name="dwAttribute">The attribute to retrieve, specified as a DWMWINDOWATTRIBUTE value.</param>
        /// <param name="pvAttribute">A pointer to a value that, when this function returns successfully, receives the
        /// current value of the attribute. The type of the retrieved value depends on the value of the dwAttribute
        /// parameter.</param>
        /// <param name="cbAttribute">The size of the DWMWINDOWATTRIBUTE value being retrieved. The size is dependent
        /// on the type of the pvAttribute parameter.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/dwmapi/nf-dwmapi-dwmgetwindowattribute
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out int pvAttribute, int cbAttribute);

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
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-enumdisplaymonitors
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an
        /// application-defined callback function. EnumWindows continues until the last top-level window is enumerated
        /// or the callback function returns FALSE.
        /// </summary>
        /// <param name="lpEnumFunc">A pointer to an application-defined callback function.</param>
        /// <param name="lParam">An application-defined value to be passed to the callback function.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-enumwindows
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);


        /// <summary>
        /// Retrieves the specified 32-bit value from the WNDCLASSEX structure associated with the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The value to be retrieved.</param>
        /// <returns>If the function succeeds, the return value is the requested value. If the function fails, the
        /// return value is zero.</returns>
        [DllImport("user32.dll")]
        private static extern uint GetClassLong(IntPtr hWnd, GetClassLongOffset nIndex);

        /// <summary>
        /// The GetMonitorInfo function retrieves information about a display monitor.
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor of interest.</param>
        /// <param name="lpmi">A pointer to a <see cref="MONITORINFO"/> or MONITORINFOEX structure that receives
        /// information about the specified display monitor. You must set the cbSize member of the structure to
        /// sizeof(MONITORINFO) or sizeof(MONITORINFOEX) before calling the GetMonitorInfo function.Doing so lets the
        /// function determine the type of structure you are passing to it. The MONITORINFOEX structure is a superset
        /// of the MONITORINFO structure. It has one additional member: a string that contains a name for the display
        /// monitor.Most applications have no use for a display monitor name, and so can save some bytes by using a
        /// MONITORINFO structure.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is
        /// zero.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getmonitorinfoa
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// Retrieves a handle to the Shell's desktop window.
        /// </summary>
        /// <returns>The return value is the handle of the Shell's desktop window. If no Shell process is present, the
        /// return value is NULL.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getshellwindow
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the 32-bit (DWORD) value at
        /// the specified offset into the extra window memory. 
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved. Valid values are in the range zero
        /// through the number of bytes of extra window memory, minus four; for example, if you specified 12 or more
        /// bytes of extra memory, a value of 8 would be an index to the third 32-bit integer.</param>
        /// <returns></returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowlonga
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in
        /// screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpRect">A pointer to a <see cref="RECT"/> structure that receives the screen coordinates of
        /// the upper-left and lower-right corners of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is
        /// zero.To get extended error information, call GetLastError.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowrect
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window
        /// is a control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a
        /// control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control containing the text.</param>
        /// <param name="lpString">The buffer that will receive the text. If the string is as long or longer than the
        /// buffer, the string is truncated and terminated with a null character.</param>
        /// <param name="nMaxCount">The maximum number of characters to copy to the buffer, including the null character
        /// . If the text exceeds this limit, it is truncated.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the copied string, not
        /// including the terminating null character. If the window has no title bar or text, if the title bar is
        /// empty, or if the window or control handle is invalid, the return value is zero.
        /// To get extended error information, call GetLastError.
        /// This function cannot retrieve the text of an edit control in another application.
        /// </returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowtexta
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Retrieves the length, in characters, of the specified window's title bar text (if the window has a title
        /// bar). If the specified window is a control, the function retrieves the length of the text within the
        /// control. However, GetWindowTextLength cannot retrieve the length of the text of an edit control in another
        /// application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the text. Under certain
        /// conditions, this value may actually be greater than the length of the text. For more information, see the
        /// following Remarks section.
        /// If the window has no text, the return value is zero.To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        /// If the target window is owned by the current process, GetWindowTextLength causes a WM_GETTEXTLENGTH message
        /// to be sent to the specified window or control.
        /// Under certain conditions, the GetWindowTextLength function may return a value that is larger than the
        /// actual length of the text.This occurs with certain mixtures of ANSI and Unicode, and is due to the system
        /// allowing for the possible existence of double-byte character set (DBCS) characters within the text.The
        /// return value, however, will always be at least as large as the actual length of the text; you can thus
        /// always use it to guide buffer allocation.This behavior can occur when an application uses both ANSI
        /// functions and common dialogs, which use Unicode.It can also occur when an application uses the ANSI version
        /// of GetWindowTextLength with a window whose window procedure is Unicode, or the Unicode version of
        /// GetWindowTextLength with a window whose window procedure is ANSI.For more information on ANSI and ANSI
        /// functions, see Conventions for Function Prototypes.
        /// To obtain the exact length of the text, use the WM_GETTEXT, LB_GETTEXT, or CB_GETLBTEXT messages, or the
        /// GetWindowText function.
        /// </remarks>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowtextlengtha
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of
        /// the process that created the window.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier. If this parameter
        /// is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it
        /// does not.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowthreadprocessid
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Determines whether the specified window is minimized (iconic).
        /// </summary>
        /// <param name="hWnd">A handle to the window to be tested.</param>
        /// <returns>If the window is iconic, the return value is nonzero. If the window is not iconic, the return
        /// value is zero.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-isiconic
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        /// <summary>
        /// Determines the visibility state of the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window to be tested.</param>
        /// <returns>If the specified window, its parent window, its parent's parent window, and so forth, have the
        /// WS_VISIBLE style, the return value is nonzero. Otherwise, the return value is zero.
        /// Because the return value specifies whether the window has the WS_VISIBLE style, it may be nonzero even if
        /// the window is totally obscured by other windows.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-iswindowvisible
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Sends the specified message to one or more windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Any additional message-specific information.</param>
        /// <param name="lParam">Any additional message-specific information.</param>
        /// <param name="fuFlag">The behavior of this function.</param>
        /// <param name="uTimeout">The duration of the time-out period, in milliseconds.</param>
        /// <param name="lpdwResult">The result of the message processing. The value of this parameter depends on the
        /// message that is specified.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-sendmessagetimeouta
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, Message msg, uint wParam, int lParam, SendMessageTimeoutFlag fuFlag, uint uTimeout, out IntPtr lpdwResult);

        /// <summary>
        /// Sets an event hook function for a range of events.
        /// </summary>
        /// <param name="eventMin">Specifies the event constant for the lowest event value in the range of events that
        /// are handled by the hook function.
        /// This parameter can be set to EVENT_MIN to indicate the lowest possible event value.</param>
        /// <param name="eventMax">Specifies the event constant for the highest event value in the range of events that
        /// are handled by the hook function. This parameter can be set to EVENT_MAX to indicate the highest possible
        /// event value.</param>
        /// <param name="hmodWinEventProc">Handle to the DLL that contains the hook function at lpfnWinEventProc, if
        /// the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter.
        /// If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this
        /// parameter is NULL.</param>
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
        /// https://docs.microsoft.com/windows/desktop/api/Winuser/nf-winuser-setwineventhook
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(WinEventConstant eventMin, WinEventConstant eventMax, IntPtr hmodWinEventProc, Wineventproc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        /// <summary>
        /// Removes an event hook function created by a previous call to <see cref="SetWinEventHook(WinEventConstant, WinEventConstant, IntPtr, Wineventproc, uint, uint, uint)" />.
        /// </summary>
        /// <param name="hWinEventHook">Handle to the event hook returned in the previous call to SetWinEventHook.</param>
        /// <returns>If successful, returns TRUE; otherwise, returns FALSE.</returns>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-unhookwinevent
        [DllImport("user32.dll")]
        internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        /// <summary>
        /// An application-defined callback function used with the EnumWindows or EnumDesktopWindows function. It
        /// receives top-level window handles. The WNDENUMPROC type defines a pointer to this callback function.
        /// EnumWindowsProc is a placeholder for the application-defined function name.
        /// </summary>
        /// <param name="hwnd">A handle to a top-level window.</param>
        /// <param name="lParam">The application-defined value given in EnumWindows or EnumDesktopWindows.</param>
        /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE.</returns>
        /// https://msdn.microsoft.com/library/windows/desktop/ms633498.aspx
        private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

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
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nc-winuser-monitorenumproc
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
        /// https://docs.microsoft.com/windows/desktop/api/Winuser/nc-winuser-wineventproc
        internal delegate void Wineventproc(IntPtr hWinEventHook, WinEventConstant @event, IntPtr hwnd, ObjectIdentifier idObject, int idChild, uint idEventThread, uint dwmsEventTime);

        /// <summary>
        /// Identifies whether the event was triggered by an object or a child element of the object.
        /// </summary>
        /// https://docs.microsoft.com/windows/desktop/api/Winuser/nc-winuser-wineventproc#parameters
        internal const int CHILDID_SELF = 0;

        /// <summary>
        /// Window Long Index. Retrieves the window styles.
        /// </summary>
        /// https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowlonga#GWL_STYLE
        private const int GWL_STYLE = -16;

        /// <summary>
        /// Return value constant for Desktop Window Manager functions.
        /// </summary>
        private const int S_OK = 0;

        /// <summary>
        /// The callback function is not mapped into the address space of the process that generates the event. Because
        /// the hook function is called across process boundaries, the system must queue events. Although this method
        /// is asynchronous, events are guaranteed to be in sequential order.
        /// </summary>
        /// https://docs.microsoft.com/windows/desktop/api/Winuser/nf-winuser-setwineventhook#parameters
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        /// <summary>
        /// Prevents this instance of the hook from receiving the events that are generated by threads in this process.
        /// This flag does not prevent threads from generating events. 
        /// </summary>
        /// https://docs.microsoft.com/windows/desktop/api/Winuser/nf-winuser-setwineventhook#parameters
        private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

        /// <summary>
        /// Window Style: The window has a title bar (includes the WS_BORDER style).
        /// </summary>
        /// https://docs.microsoft.com/windows/desktop/winmsg/window-styles#WS_CAPTION
        private const long WS_CAPTION = 0x00C00000L;

        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
        // ReSharper restore MemberCanBePrivate.Local
        #endregion
    }
}
