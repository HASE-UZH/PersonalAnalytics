using System.Collections.Generic;

namespace WindowRecommender
{
    internal class WindowStack
    {
        internal List<WindowRecord> WindowRecords { get; private set; }

        internal WindowStack(IWindowEvents windowEvents)
        {
            WindowRecords = new List<WindowRecord>();
            windowEvents.WindowOpenedOrFocused += OnWindowOpenedOrFocused;
            windowEvents.WindowClosedOrMinimized += OnWindowClosedOrMinimized;
            windowEvents.Setup += OnSetup;
        }

        internal int GetZIndex(WindowRecord windowRecord)
        {
            return WindowRecords.IndexOf(windowRecord);
        }

        private void OnSetup(object sender, List<WindowRecord> windowRecords)
        {
            WindowRecords = windowRecords;
        }

        private void OnWindowClosedOrMinimized(object sender, WindowRecord e)
        {
            var windowRecord = e;
            WindowRecords.Remove(windowRecord);
        }

        private void OnWindowOpenedOrFocused(object sender, WindowRecord e)
        {
            var windowRecord = e;
            WindowRecords.Remove(windowRecord);
            WindowRecords.Insert(0, windowRecord);
        }
    }
}
