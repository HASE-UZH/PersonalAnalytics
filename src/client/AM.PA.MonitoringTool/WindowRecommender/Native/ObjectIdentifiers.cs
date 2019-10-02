namespace WindowRecommender.Native
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global

    /// <summary>
    /// Microsoft Active Accessibility object identifiers, 32-bit values that identify categories of accessible objects
    /// within a window.
    /// </summary>
    /// https://docs.microsoft.com/windows/desktop/WinAuto/object-identifiers
    public enum ObjectIdentifier : uint
    {
        /// <summary>
        /// The window itself rather than a child object.
        /// </summary>
        OBJID_WINDOW = 0x00000000,

        /// <summary>
        /// The window's system menu.
        /// </summary>
        OBJID_SYSMENU = 0xFFFFFFFF,

        /// <summary>
        /// The window's title bar.
        /// </summary>
        OBJID_TITLEBAR = 0xFFFFFFFE,

        /// <summary>
        /// The window's menu bar.
        /// </summary>
        OBJID_MENU = 0xFFFFFFFD,

        /// <summary>
        /// The window's client area. In most cases, the operating system controls the frame elements and the client
        /// object contains all elements that are controlled by the application.
        /// Servers only process the WM_GETOBJECT messages in which the lParam is OBJID_CLIENT, OBJID_WINDOW, or a
        /// custom object identifier.
        /// </summary>
        OBJID_CLIENT = 0xFFFFFFFC,

        /// <summary>
        /// The window's vertical scroll bar.
        /// </summary>
        OBJID_VSCROLL = 0xFFFFFFFB,

        /// <summary>
        /// The window's horizontal scroll bar.
        /// </summary>
        OBJID_HSCROLL = 0xFFFFFFFA,

        /// <summary>
        /// The window's size grip: an optional frame component located at the lower-right corner of the window frame.
        /// </summary>
        OBJID_SIZEGRIP = 0xFFFFFFF9,

        /// <summary>
        /// The text insertion bar (caret) in the window.
        /// </summary>
        OBJID_CARET = 0xFFFFFFF8,

        /// <summary>
        /// The mouse pointer. There is only one mouse pointer in the system, and it is not a child of any window.
        /// </summary>
        OBJID_CURSOR = 0xFFFFFFF7,

        /// <summary>
        /// An alert that is associated with a window or an application. System provided message boxes are the only UI
        /// elements that send events with this object identifier. Server applications cannot use the
        /// AccessibleObjectFromX functions with this object identifier. This is a known issue with Microsoft Active
        /// Accessibility.
        /// </summary>
        OBJID_ALERT = 0xFFFFFFF6,

        /// <summary>
        /// A sound object. Sound objects do not have screen locations or children, but they do have name and state
        /// attributes. They are children of the application that is playing the sound.
        /// </summary>
        OBJID_SOUND = 0xFFFFFFF5,

        /// <summary>
        /// An object identifier that Oleacc.dll uses internally.
        /// </summary>
        OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4,

        /// <summary>
        /// In response to this object identifier, third-party applications can expose their own object model.
        /// Third-party applications can return any COM interface in response to this object identifier.
        /// </summary>
        OBJID_NATIVEOM = 0xFFFFFFF0
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedMember.Global
}
