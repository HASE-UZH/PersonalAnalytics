namespace WindowRecommender.Native
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IdentifierTypo

    /// https://docs.microsoft.com/windows/desktop/WinAuto/event-constants
    internal enum WinEventConstant : uint
    {
        EVENT_MIN = 0x00000001,
        EVENT_MAX = 0x7FFFFFFF,
        EVENT_SYSTEM_SOUND = 0x0001,
        EVENT_SYSTEM_ALERT = 0x0002,
        /// <summary>
        /// The foreground window has changed. The system sends this event even if the foreground window has changed to
        /// another window in the same thread. Server applications never send this event.
        /// For this event, the <see cref="NativeMethods.Wineventproc"/> callback function's hwnd parameter is the
        /// handle to the window that is in the foreground, the idObject parameter is
        /// <see cref="ObjectIdentifier.OBJID_WINDOW"/>, and the idChild parameter is
        /// <see cref="NativeMethods.CHILDID_SELF"/>.
        /// </summary>
        EVENT_SYSTEM_FOREGROUND = 0x0003,
        EVENT_SYSTEM_MENUSTART = 0x0004,
        EVENT_SYSTEM_MENUEND = 0x0005,
        EVENT_SYSTEM_MENUPOPUPSTART = 0x0006,
        EVENT_SYSTEM_MENUPOPUPEND = 0x0007,
        EVENT_SYSTEM_CAPTURESTART = 0x0008,
        EVENT_SYSTEM_CAPTUREEND = 0x0009,
        /// <summary>
        /// A window is being moved or resized. This event is sent by the system, never by servers.
        /// </summary>
        EVENT_SYSTEM_MOVESIZESTART = 0x000A,
        /// <summary>
        /// The movement or resizing of a window has finished. This event is sent by the system, never by servers.
        /// </summary>
        EVENT_SYSTEM_MOVESIZEEND = 0x000B,
        EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C,
        EVENT_SYSTEM_CONTEXTHELPEND = 0x000D,
        EVENT_SYSTEM_DRAGDROPSTART = 0x000E,
        EVENT_SYSTEM_DRAGDROPEND = 0x000F,
        EVENT_SYSTEM_DIALOGSTART = 0x0010,
        EVENT_SYSTEM_DIALOGEND = 0x0011,
        EVENT_SYSTEM_SCROLLINGSTART = 0x0012,
        EVENT_SYSTEM_SCROLLINGEND = 0x0013,
        EVENT_SYSTEM_SWITCHSTART = 0x0014,
        EVENT_SYSTEM_SWITCHEND = 0x0015,
        /// <summary>
        /// A window object is about to be minimized. This event is sent by the system, never by servers.
        /// </summary>
        EVENT_SYSTEM_MINIMIZESTART = 0x0016,
        /// <summary>
        /// A window object is about to be restored. This event is sent by the system, never by servers.
        /// </summary>
        EVENT_SYSTEM_MINIMIZEEND = 0x0017,
        /// <summary>
        /// The active desktop has been switched.
        /// </summary>
        EVENT_SYSTEM_DESKTOPSWITCH = 0x0020,
        EVENT_SYSTEM_END = 0x00FF,
        EVENT_OEM_DEFINED_START = 0x0101,
        EVENT_OEM_DEFINED_END = 0x01FF,
        EVENT_UIA_EVENTID_START = 0x4E00,
        EVENT_UIA_EVENTID_END = 0x4EFF,
        EVENT_UIA_PROPID_START = 0x7500,
        EVENT_UIA_PROPID_END = 0x75FF,
        EVENT_CONSOLE_CARET = 0x4001,
        EVENT_CONSOLE_UPDATE_REGION = 0x4002,
        EVENT_CONSOLE_UPDATE_SIMPLE = 0x4003,
        EVENT_CONSOLE_UPDATE_SCROLL = 0x4004,
        EVENT_CONSOLE_LAYOUT = 0x4005,
        EVENT_CONSOLE_START_APPLICATION = 0x4006,
        EVENT_CONSOLE_END_APPLICATION = 0x4007,
        EVENT_CONSOLE_END = 0x40FF,
        EVENT_OBJECT_CREATE = 0x8000,
        /// <summary>
        /// An object has been destroyed. The system sends this event for the following user interface elements: caret,
        /// header control, list-view control, tab control, toolbar control, tree view control, and window object.
        /// Server applications send this event for their accessible objects.
        /// Clients assume that all of an object's children are destroyed when the parent object sends this event.
        /// </summary>
        EVENT_OBJECT_DESTROY = 0x8001,
        EVENT_OBJECT_SHOW = 0x8002,
        EVENT_OBJECT_HIDE = 0x8003,
        EVENT_OBJECT_REORDER = 0x8004,
        EVENT_OBJECT_FOCUS = 0x8005,
        EVENT_OBJECT_SELECTION = 0x8006,
        EVENT_OBJECT_SELECTIONADD = 0x8007,
        EVENT_OBJECT_SELECTIONREMOVE = 0x8008,
        EVENT_OBJECT_SELECTIONWITHIN = 0x8009,
        EVENT_OBJECT_STATECHANGE = 0x800A,
        /// <summary>
        /// An object has changed location, shape, or size. The system sends this event for the following user
        /// interface elements: caret and window objects. Server applications send this event for their accessible
        /// objects.
        /// This event is generated in response to a change in the top-level object within the object hierarchy; it is
        /// not generated for any children that the object might have. For example, if the user resizes a window, the
        /// system sends this notification for the window, but not for the menu bar, title bar, scroll bar, or other
        /// objects that have also changed.
        /// </summary>
        EVENT_OBJECT_LOCATIONCHANGE = 0x800B,
        /// <summary>
        /// An object's Name property has changed. The system sends this event for the following user interface
        /// elements: check box, cursor, list-view control, push button, radio button, status bar control, tree view
        /// control, and window object. Server applications send this event for their accessible objects.
        /// </summary>
        EVENT_OBJECT_NAMECHANGE = 0x800C,
        EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D,
        EVENT_OBJECT_VALUECHANGE = 0x800E,
        EVENT_OBJECT_PARENTCHANGE = 0x800F,
        EVENT_OBJECT_HELPCHANGE = 0x8010,
        EVENT_OBJECT_DEFACTIONCHANGE = 0x8011,
        EVENT_OBJECT_ACCELERATORCHANGE = 0x8012,
        EVENT_OBJECT_INVOKED = 0x8013,
        EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014,
        EVENT_OBJECT_CONTENTSCROLLED = 0x8015,
        EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016,
        EVENT_OBJECT_END = 0x80FF,
        EVENT_AIA_START = 0xA000,
        EVENT_AIA_END = 0xAFFF
    }

    // ReSharper restore IdentifierTypo
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedMember.Global
}
