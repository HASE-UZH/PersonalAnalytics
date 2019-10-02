using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Timers;
using WindowRecommender.Util;

namespace WindowRecommender.Models
{
    internal class Frequency : BaseModel
    {

        private readonly List<(IntPtr windowHandle, DateTime dateTime)> _focusEvents;
        private readonly HashSet<IntPtr> _closedWindows;

        private Dictionary<IntPtr, double> _scores;

        internal Frequency(IWindowEvents windowEvents) : base(windowEvents)
        {
            _scores = new Dictionary<IntPtr, double>();
            _focusEvents = new List<(IntPtr windowHandle, DateTime dateTime)>();
            _closedWindows = new HashSet<IntPtr>();

            windowEvents.WindowClosedOrMinimized += OnWindowClosedOrMinimized;
            windowEvents.WindowOpenedOrFocused += OnWindowOpenedOrFocused;

            var timer = new Timer(Settings.DurationIntervalSeconds * 1000)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += OnInterval;
        }

        internal void OnInterval(object sender, ElapsedEventArgs e)
        {
            var cutoff = DateTime.Now.AddMinutes(-Settings.DurationTimeframeMinutes);

            // Remove outdated events and events of closed windows
            _focusEvents.RemoveAll(tuple => _closedWindows.Contains(tuple.windowHandle) || tuple.dateTime < cutoff);

            // Group events by window, count events, and divide by total count
            var eventCount = (double)_focusEvents.Count;
            var newScores = _focusEvents.GroupBy(tuple => tuple.windowHandle)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Count() / eventCount);

            _closedWindows.Clear();

            if (!_scores.SequenceEqual(newScores, new ScoreEqualityComparer()))
            {
                InvokeScoreChanged();
                _scores = newScores;
            }
        }

        public override ImmutableDictionary<IntPtr, double> GetScores()
        {
            return _scores.ToImmutableDictionary();
        }

        protected override void Setup(List<WindowRecord> windowRecords)
        {
            if (windowRecords.Count > 0)
            {
                var windowHandle = windowRecords.First().Handle;
                _focusEvents.Add((windowHandle, dateTime: DateTime.Now));
                _scores = new Dictionary<IntPtr, double>
                {
                    {windowHandle, 1}
                };
            }
        }

        private void OnWindowClosedOrMinimized(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _closedWindows.Add(windowRecord.Handle);
        }

        private void OnWindowOpenedOrFocused(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _focusEvents.Add((windowHandle: windowRecord.Handle, dateTime: DateTime.Now));
        }
    }
}
