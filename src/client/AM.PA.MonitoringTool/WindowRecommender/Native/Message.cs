// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace WindowRecommender.Native
{
    internal enum Message
    {
        /// <summary>
        /// Sent to a window to retrieve a handle to the large or small icon associated with a window. The system
        /// displays the large icon in the ALT+TAB dialog, and the small icon in the window caption.
        /// </summary>
        /// https://docs.microsoft.com/windows/desktop/winmsg/wm-geticon
        WM_GETICON = 0x7F,
    }

    /// <summary>
    /// The type of icon being retrieved.
    /// </summary>
    /// https://docs.microsoft.com/windows/desktop/winmsg/wm-geticon#parameters
    internal enum GetIconParameter : uint
    {
        /// <summary>
        /// Retrieve the small icon for the window.
        /// </summary>
        ICON_SMALL,

        /// <summary>
        /// Retrieve the large icon for the window.
        /// </summary>
        ICON_BIG,

        /// <summary>
        /// Retrieves the small icon provided by the application. If the application does not provide one, the system
        /// uses the system-generated icon for that window.
        /// </summary>
        ICON_SMALL2,
    }
}
