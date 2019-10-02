using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Timers;
using WindowRecommender.Util;

namespace WindowRecommender.Models
{
    internal class Duration : BaseModel
    {
        private readonly Dictionary<IntPtr, double> _scores;
        private readonly List<(IntPtr windowHandle, DateTime dateTime)> _focusEvents;
        private readonly HashSet<IntPtr> _closedWindows;

        internal Duration(IWindowEvents windowEvents) : base(windowEvents)
        {
            _scores = new Dictionary<IntPtr, double>();
            _focusEvents = new List<(IntPtr windowHandle, DateTime dateTime)>();
            _closedWindows = new HashSet<IntPtr>();

            windowEvents.WindowOpenedOrFocused += OnWindowOpenedOrFocused;
            windowEvents.WindowClosedOrMinimized += OnWindowClosedOrMinimized;

            var timer = new Timer(Settings.DurationIntervalSeconds * 1000)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += OnInterval;
        }

        internal void OnInterval(object sender, ElapsedEventArgs e)
        {
            var hasChanged = false;
            var cutoff = DateTime.Now.AddMinutes(-Settings.DurationTimeframeMinutes);
            var lastPoll = DateTime.Now.AddSeconds(-Settings.DurationIntervalSeconds);

            // Add dummy event to have an end for last focus and create pairs
            var currentEvents = _focusEvents.Concat(new[]
            {
                (windowHandle: IntPtr.Zero, dateTime: DateTime.Now)
            }).Pairwise(Tuple.Create).ToList();

            foreach (var ((windowHandle, startTime), (_, endTime)) in currentEvents)
            {
                double scoreDiff = 0;

                // Duration starts before cutoff
                if (startTime < cutoff)
                {
                    // Duration completely outside relevant timeframe -> Remove
                    if (endTime <= cutoff)
                    {
                        _focusEvents.RemoveAt(0);
                        scoreDiff -= (endTime - startTime).TotalMinutes / Settings.DurationTimeframeMinutes;
                    }
                    // Duration partly outside relevant timeframe -> Reduce
                    else
                    {
                        _focusEvents[0] = (windowHandle, dateTime: cutoff);
                        scoreDiff -= (cutoff - startTime).TotalMinutes / Settings.DurationTimeframeMinutes;
                    }
                }
                // Duration continues since last poll
                if (endTime > lastPoll)
                {
                    // Duration started before last poll -> Increase
                    if (startTime < lastPoll)
                    {
                        scoreDiff += (endTime - lastPoll).TotalMinutes / Settings.DurationTimeframeMinutes;
                    }
                    // Duration started after last pool -> Add
                    else
                    {
                        scoreDiff += (endTime - startTime).TotalMinutes / Settings.DurationTimeframeMinutes;
                        if (!_scores.ContainsKey(windowHandle) && !_closedWindows.Contains(windowHandle))
                        {
                            _scores[windowHandle] = 0;
                        }
                    }
                }

                // Update score if it has changed
                if (!scoreDiff.IsZero() && _scores.ContainsKey(windowHandle))
                {
                    _scores[windowHandle] += scoreDiff;
                    // Remove entries when score is 0 (or close enough for floating point values)
                    if (_scores[windowHandle].IsZero())
                    {
                        _scores.Remove(windowHandle);
                    }
                    hasChanged = true;
                }
            }
            _closedWindows.Clear();

            if (hasChanged)
            {
                InvokeScoreChanged();
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
            }
        }

        private void OnWindowClosedOrMinimized(object sender, WindowRecord e)
        {
            var windowRecord = e;
            if (_scores.ContainsKey(windowRecord.Handle))
            {
                _scores.Remove(windowRecord.Handle);
                InvokeScoreChanged();
            }
            else
            {
                _closedWindows.Add(windowRecord.Handle);
            }
        }

        private void OnWindowOpenedOrFocused(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _focusEvents.Add((windowHandle: windowRecord.Handle, dateTime: DateTime.Now));
        }
    }
}
