using System;
using System.Collections.Generic;
using System.Linq;
using WindowRecommender.Data;

namespace WindowRecommender
{
    internal class WindowRecorder
    {
        private readonly WindowStack _windowStack;

        private Dictionary<IntPtr, double> _scores;
        private IntPtr[] _ranks;

        internal WindowRecorder(IWindowEvents windowEvents, WindowStack windowStack)
        {
            _windowStack = windowStack;

            _scores = new Dictionary<IntPtr, double>();
            _ranks = new IntPtr[0];

            windowEvents.Setup += OnSetup;
            windowEvents.WindowOpened += OnWindowOpened;
            windowEvents.WindowFocused += OnWindowFocused;
            windowEvents.WindowClosed += OnWindowClosed;
            windowEvents.WindowMinimized += OnWindowMinimized;
        }

        internal void SetScores(Dictionary<IntPtr, double> scores)
        {
            _scores = scores;
        }

        internal void SetTopWindows(IEnumerable<IntPtr> ranks)
        {
            _ranks = ranks.ToArray();
        }

        private void OnSetup(object sender, List<WindowRecord> e)
        {
            var windowRecords = e;
            Queries.SaveWindowEvents(EventName.Initial, windowRecords.Select(GetBasicEntry));
        }

        private void OnWindowClosed(object sender, WindowRecord e)
        {
            var windowRecord = e;
            if (_scores.ContainsKey(windowRecord.Handle))
            {
                Queries.SaveWindowEvent(EventName.Close, GetExtendedEntry(windowRecord));
                _scores.Remove(windowRecord.Handle);
            }
        }

        private void OnWindowMinimized(object sender, WindowRecord e)
        {
            var windowRecord = e;
            if (_scores.ContainsKey(windowRecord.Handle))
            {
                Queries.SaveWindowEvent(EventName.Minimize, GetExtendedEntry(windowRecord));
            }
        }

        private void OnWindowFocused(object sender, WindowRecord e)
        {
            var windowRecord = e;
            if (_scores.ContainsKey(windowRecord.Handle))
            {
                Queries.SaveWindowEvent(EventName.Focus, GetExtendedEntry(windowRecord));
            }
            else
            {
                Queries.SaveWindowEvent(EventName.Open, GetBasicEntry(windowRecord));
            }
        }

        private void OnWindowOpened(object sender, WindowRecord e)
        {
            var windowRecord = e;
            Queries.SaveWindowEvent(EventName.Open, GetBasicEntry(windowRecord));
        }

        private WindowEventRecord GetBasicEntry(WindowRecord windowRecord)
        {
            var zIndex = _windowStack.GetZIndex(windowRecord);
            return new WindowEventRecord(windowRecord.Handle, windowRecord.Title, windowRecord.ProcessName, zIndex);
        }

        private WindowEventRecord GetExtendedEntry(WindowRecord windowRecord)
        {
            var zIndex = _windowStack.GetZIndex(windowRecord);
            var rank = Array.IndexOf(_ranks, windowRecord.Handle);
            var score = _scores[windowRecord.Handle];
            return new WindowEventRecord(windowRecord.Handle, windowRecord.Title, windowRecord.ProcessName, zIndex, rank, score);
        }
    }
}
