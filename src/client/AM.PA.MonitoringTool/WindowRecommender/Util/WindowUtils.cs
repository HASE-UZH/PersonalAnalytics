using System.Collections.Generic;
using WindowRecommender.Graphics;
using WindowRecommender.Native;

namespace WindowRecommender.Util
{
    internal static class WindowUtils
    {
        private const int Correction = 8;

        private static readonly ISet<string> CORRECT_PROCESSES = new HashSet<string>
        {
            // MS Office
            "WINWORD",
            "EXCEL",
            "ONENOTE",
            "OUTLOOK",
            "POWERPNT",
            "MSACCESS",

            // IDE
            "devenv",
            "Code",

            // Chat
            "Discord",
            "Teams",
            "Slack",
        };

        internal static Rectangle GetCorrectedWindowRectangle(WindowRecord windowRecord)
        {
            var rectangle = NativeMethods.GetWindowRectangle(windowRecord.Handle);
            if (CORRECT_PROCESSES.Contains(windowRecord.ProcessName))
            {
                return (Rectangle)rectangle;
            }

            return new Rectangle(rectangle.Left + Correction, rectangle.Top, rectangle.Right - Correction, rectangle.Bottom - Correction);
        }
    }
}
