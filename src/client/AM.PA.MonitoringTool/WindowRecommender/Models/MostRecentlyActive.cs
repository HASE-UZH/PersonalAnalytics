using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WindowRecommender.Models
{
    internal class MostRecentlyActive : BaseModel
    {
        private List<IntPtr> _windows;

        internal MostRecentlyActive(IWindowEvents windowEvents) : base(windowEvents)
        {
            _windows = new List<IntPtr>();

            windowEvents.WindowClosedOrMinimized += OnWindowClosedOrMinimized;
            windowEvents.WindowOpened += OnWindowOpened;
            windowEvents.WindowFocused += OnWindowFocused;
        }

        public override ImmutableDictionary<IntPtr, double> GetScores()
        {
            return _windows.Take(Settings.NumberOfWindows).ToImmutableDictionary(windowHandle => windowHandle, _ => 1D);
        }

        protected override void Setup(List<WindowRecord> windowRecords)
        {
            if (windowRecords.Count > 0)
            {
                _windows = new List<IntPtr> { windowRecords[0].Handle };
            }
        }

        private void OnWindowClosedOrMinimized(object sender, WindowRecord e)
        {
            var windowRecord = e;
            var index = _windows.IndexOf(windowRecord.Handle);
            if (index != -1)
            {
                var hasChanged = index < Settings.NumberOfWindows;
                _windows.Remove(windowRecord.Handle);
                if (hasChanged)
                {
                    InvokeScoreChanged();
                }
            }
        }

        private void OnWindowFocused(object sender, WindowRecord e)
        {
            var windowRecord = e;
            var index = _windows.IndexOf(windowRecord.Handle);
            var hasChanged = index == -1 || index >= Settings.NumberOfWindows;
            _windows.Remove(windowRecord.Handle);
            _windows.Insert(0, windowRecord.Handle);
            if (hasChanged)
            {
                InvokeScoreChanged();
            }
        }

        private void OnWindowOpened(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _windows.Remove(windowRecord.Handle);
            _windows.Insert(0, windowRecord.Handle);
            InvokeScoreChanged();
        }
    }
}
