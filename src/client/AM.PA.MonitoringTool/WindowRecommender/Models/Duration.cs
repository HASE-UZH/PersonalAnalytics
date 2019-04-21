﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace WindowRecommender.Models
{
    internal class Duration : BaseModel
    {
        private readonly Dictionary<IntPtr, double> _scores;
        private readonly List<(IntPtr windowHandle, DateTime dateTime)> _focusEvents;
        private readonly HashSet<IntPtr> _closedWindows;

        private IntPtr[] _topWindows;

        internal Duration(ModelEvents modelEvents) : base(modelEvents)
        {
            _scores = new Dictionary<IntPtr, double>();
            _focusEvents = new List<(IntPtr windowHandle, DateTime dateTime)>();
            _closedWindows = new HashSet<IntPtr>();
            _topWindows = new IntPtr[0];

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
            var lastPoll = DateTime.Now.AddSeconds(-Settings.DurationIntervalSeconds);

            // Add dummy event to have and end for last focus and create pairs
            var currentEvents = _focusEvents.Concat(new[]
            {
                (windowHandle: IntPtr.Zero, dateTime: DateTime.Now)
            }).Pairwise(Tuple.Create).ToList();

            foreach (var ((windowHandle, startTime), (_, endTime)) in currentEvents)
            {
                var score = (endTime - startTime).TotalMinutes / Settings.DurationTimeframeMinutes;

                // Duration outside relevant timeframe -> Remove
                if (endTime <= cutoff)
                {
                    _focusEvents.RemoveAt(0);
                    score = -score;
                }
                // Duration starts outside relevant timeframe -> Reduce
                else if (startTime < cutoff)
                {
                    _focusEvents[0] = (windowHandle: windowHandle, dateTime: cutoff);
                    score = -(cutoff - startTime).TotalMinutes / Settings.DurationTimeframeMinutes;
                }
                // Duration extends into new timeframe
                else if (endTime > lastPoll)
                {
                    // Duration started before last poll -> Increase
                    if (startTime < lastPoll)
                    {
                        score = (endTime - lastPoll).TotalMinutes / Settings.DurationTimeframeMinutes;
                    }
                    // Duration started after last pool -> Add
                    else
                    {
                        if (!_scores.ContainsKey(windowHandle) && !_closedWindows.Contains(windowHandle))
                        {
                            _scores[windowHandle] = 0;
                        }
                    }
                }
                // Skip events that do not reach the borders as their scores don't change
                else
                {
                    continue;
                }

                // Update score if it was added or not yet deleted
                if (_scores.ContainsKey(windowHandle))
                {
                    _scores[windowHandle] += score;
                    // Remove entries when score is 0 (or close enough for floating point values)
                    if (Math.Abs(_scores[windowHandle]) < 0.0000001)
                    {
                        _scores.Remove(windowHandle);
                    }
                }
            }
            _closedWindows.Clear();

            var newTop = GetTopWindows(_scores).ToArray();
            if (!_topWindows.SequenceEqual(newTop))
            {
                InvokeOrderChanged();
                _topWindows = newTop;
            }
        }

        public override Dictionary<IntPtr, double> GetScores()
        {
            return _scores;
        }

        public override void SetWindows(IEnumerable<IntPtr> windows)
        {
            if (windows.Count() > 0)
            {
                var windowHandle = windows.First();
                _focusEvents.Add((windowHandle, dateTime: DateTime.Now));
                _topWindows = new[] { windowHandle };
            }
        }

        protected override void OnWindowClosed(object sender, IntPtr e)
        {
            var windowHandle = e;
            if (_scores.ContainsKey(windowHandle))
            {
                _scores.Remove(windowHandle);
            }
            else
            {
                _closedWindows.Add(windowHandle);
            }
        }

        protected override void OnWindowFocused(object sender, IntPtr e)
        {
            var windowHandle = e;
            _focusEvents.Add((windowHandle: windowHandle, dateTime: DateTime.Now));
        }

        protected override void OnWindowOpened(object sender, IntPtr e)
        {
            var windowHandle = e;
            _focusEvents.Add((windowHandle: windowHandle, dateTime: DateTime.Now));
        }
    }
}