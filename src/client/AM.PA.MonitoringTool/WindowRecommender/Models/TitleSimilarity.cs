using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Accord.MachineLearning;
using WindowRecommender.Util;

namespace WindowRecommender.Models
{
    internal class TitleSimilarity : BaseModel
    {
        private Dictionary<IntPtr, string[]> _titles;
        private Dictionary<IntPtr, double> _scores;
        private IntPtr _currentWindow;

        public TitleSimilarity(IWindowEvents windowEvents) : base(windowEvents)
        {
            _scores = new Dictionary<IntPtr, double>();
            _titles = new Dictionary<IntPtr, string[]>();

            windowEvents.WindowClosedOrMinimized += OnWindowClosedOrMinimized;
            windowEvents.WindowOpened += OnWindowOpened;
            windowEvents.WindowFocused += OnWindowFocused;
            windowEvents.WindowRenamed += OnWindowRenamed;
        }

        public override ImmutableDictionary<IntPtr, double> GetScores()
        {
            return _scores.ToImmutableDictionary();
        }

        protected override void Setup(List<WindowRecord> windowRecords)
        {
            if (windowRecords.Count > 0)
            {
                _currentWindow = windowRecords.First().Handle;
                _titles = windowRecords
                    .Select(windowRecord => (windowHandle: windowRecord.Handle, preparedTitle: GetPreparedWindowTitle(windowRecord)))
                    .Where(tuple => tuple.preparedTitle.Length != 0)
                    .ToDictionary(tuple => tuple.windowHandle, tuple => tuple.preparedTitle);
                _scores = CalculateScores();
            }
        }

        private void OnWindowClosedOrMinimized(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _titles.Remove(windowRecord.Handle);
            CalculateScoreChanges();
        }

        private void OnWindowFocused(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _currentWindow = windowRecord.Handle;
            CalculateScoreChanges();
        }

        private void OnWindowOpened(object sender, WindowRecord e)
        {
            var windowRecord = e;
            _currentWindow = windowRecord.Handle;
            var preparedTitle = GetPreparedWindowTitle(windowRecord);
            if (preparedTitle.Length != 0)
            {
                _titles[windowRecord.Handle] = preparedTitle;
            }
            CalculateScoreChanges();
        }

        private void OnWindowRenamed(object sender, WindowRecord e)
        {
            var windowRecord = e;
            var preparedTitle = GetPreparedWindowTitle(windowRecord);
            if (!_titles.ContainsKey(windowRecord.Handle) || !_titles[windowRecord.Handle].SequenceEqual(preparedTitle))
            {
                if (preparedTitle.Length != 0)
                {
                    _titles[windowRecord.Handle] = preparedTitle;
                }
                else
                {
                    _titles.Remove(windowRecord.Handle);
                }
                CalculateScoreChanges();
            }
        }

        private void CalculateScoreChanges()
        {
            var newScores = CalculateScores();
            if (!_scores.SequenceEqual(newScores, new ScoreEqualityComparer()))
            {
                InvokeScoreChanged();
                _scores = newScores;
            }
        }

        private Dictionary<IntPtr, double> CalculateScores()
        {
            if (!_titles.ContainsKey(_currentWindow))
            {
                return new Dictionary<IntPtr, double>();
            }
            var windowTitles = _titles.Where(pair => pair.Key != _currentWindow).ToArray();
            var titles = windowTitles
                .Select(pair => pair.Value)
                .Concat(new[] { new string[0], _titles[_currentWindow] }).ToArray();
            var vectors = new TFIDF().Learn(titles).Transform(titles);
            var currentWindowVector = vectors.Last();
            var scores = vectors
                .Take(windowTitles.Length)
                .Select(vector => Utils.CosineSimilarity(vector, currentWindowVector))
                .Select((similarity, i) => (similarity, windowHandle: windowTitles[i].Key))
                .Where(tuple => tuple.similarity > 0)
                .ToDictionary(tuple => tuple.windowHandle, tuple => tuple.similarity);
            return scores;
        }

        private static string[] GetPreparedWindowTitle(WindowRecord windowRecord)
        {
            return TextUtils.PrepareTitle(windowRecord.Title).ToArray();
        }
    }
}
