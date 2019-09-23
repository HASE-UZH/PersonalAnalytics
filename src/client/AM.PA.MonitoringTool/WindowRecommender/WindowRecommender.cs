﻿using Microsoft.Win32;
using Shared;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WindowRecommender.Data;
using WindowRecommender.DebugWindow;
using WindowRecommender.Graphics;
using WindowRecommender.Models;
using WindowRecommender.Util;

namespace WindowRecommender
{
    public class WindowRecommender : BaseTracker
    {
        private readonly HazeOverlay _hazeOverlay;
        private readonly ModelEvents _modelEvents;
        private readonly ModelCore _modelCore;
        private readonly WindowRecorder _windowRecorder;
        private readonly WindowStack _windowStack;
        private readonly WindowCache _windowCache;
        private readonly DebugWindow.DebugWindow _debugWindow;
        private readonly DebugWindowDataContext _debugWindowDataContext;

        public WindowRecommender()
        {
            Name = "Window Recommender";

            Settings.Enabled = Queries.GetEnabledSetting();
            Settings.NumberOfWindows = Queries.GetNumberOfWindowsSetting();
            Settings.TreatmentMode = Queries.GetTreatmentModeSetting();

            _hazeOverlay = new HazeOverlay();

            _modelEvents = new ModelEvents();

            _windowCache = new WindowCache(_modelEvents);
            _windowStack = new WindowStack(_windowCache);
            _windowRecorder = new WindowRecorder(_windowCache, _windowStack);

            _modelCore = new ModelCore(new (IModel model, double weight)[]
            {
                (new MostRecentlyActive(_windowCache), 1),
                (new Frequency(_windowCache), 1),
                (new Duration(_windowCache), 1),
                (new TitleSimilarity(_windowCache), 1),
            });
            _modelCore.ScoreChanged += OnScoresChanged;
            _modelCore.WindowsChanged += OnWindowsChanged;

            if (Settings.ShowDebugWindow)
            {
                _debugWindow = new DebugWindow.DebugWindow();
                _debugWindowDataContext = (DebugWindowDataContext)_debugWindow.DataContext;
            }
        }

        public override void Start()
        {
            if (Settings.ShowDebugWindow)
            {
                _debugWindow.Show();
            }

            IsRunning = true;
            if (TreatmentMode)
            {
                StartTreatment();
            }
            _windowCache.Start();
            _modelCore.Start();
            _modelEvents.Start();

        }

        public override void Stop()
        {
            IsRunning = false;
            _modelEvents.Stop();
            if (TreatmentMode)
            {
                StopTreatment();
            }

            if (Settings.ShowDebugWindow)
            {
                _debugWindow.Hide();
            }
        }

        public override void CreateDatabaseTablesIfNotExist()
        {
            // During development, drop and recreate tables
            Queries.DropTables();
            Queries.CreateTables();
        }

        public override void UpdateDatabaseTables(int version)
        {
            // During development, drop and recreate tables
            Queries.DropTables();
            Queries.CreateTables();
        }

        public override string GetVersion()
        {
            return VersionHelper.GetFormattedVersion(new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version);
        }

        public override bool IsEnabled()
        {
            return WindowRecommenderEnabled;
        }

        public bool WindowRecommenderEnabled
        {
            private get => Settings.Enabled;
            set
            {
                var enabled = value;
                if (enabled != Settings.Enabled)
                {
                    Settings.Enabled = enabled;
                    Queries.SetEnabledSetting(enabled);

                    // start/stop tracker if necessary
                    if (!enabled && IsRunning)
                    {
                        Stop();
                    }
                    else if (enabled && !IsRunning)
                    {
                        CreateDatabaseTablesIfNotExist();
                        Start();
                    }
                }
            }
        }

        public int NumberOfWindows
        {
            get => Settings.NumberOfWindows;
            set
            {
                var numberOfWindows = value;
                if (numberOfWindows != Settings.NumberOfWindows)
                {
                    Settings.NumberOfWindows = numberOfWindows;
                    Queries.SetNumberOfWindowsSetting(numberOfWindows);
                }
            }
        }

        public bool TreatmentMode
        {
            get => Settings.TreatmentMode;
            set
            {
                var treatmentMode = value;
                if (treatmentMode != Settings.TreatmentMode)
                {
                    Settings.TreatmentMode = treatmentMode;
                    Queries.SetTreatmentModeSetting(treatmentMode);

                    if (treatmentMode)
                    {
                        StartTreatment();
                    }
                    else
                    {
                        StopTreatment();
                    }
                }
            }
        }

        internal static IEnumerable<(WindowRecord windowRecord, bool show)> GetScoredWindows(List<IntPtr> topWindows, List<WindowRecord> windowStack)
        {
            if (windowStack.Count == 0)
            {
                return Enumerable.Empty<(WindowRecord windowRecord, bool show)>();
            }
            var foregroundWindow = windowStack.First();
            // If the foreground window is not one of the top scoring windows
            // add it as first element and
            // remove the one with the lowest score if necessary.
            if (!topWindows.Contains(foregroundWindow.Handle))
            {
                if (topWindows.Count == Settings.NumberOfWindows)
                {
                    topWindows.RemoveAt(topWindows.Count - 1);
                }
                topWindows.Insert(0, foregroundWindow.Handle);
            }
            return windowStack
                .TakeWhile(_ => topWindows.Count != 0)
                .Select(windowRecord =>
                {
                    var show = topWindows.Contains(windowRecord.Handle);
                    topWindows.Remove(windowRecord.Handle);
                    return (windowRecord, show);
                });
        }

        private void OnScoresChanged(object sender, Dictionary<IntPtr, Dictionary<string, double>> e)
        {
            var scores = e;
            var mergedScores = scores.ToDictionary(pair => pair.Key, pair => pair.Value[ModelCore.MergedScoreName]);
            _windowRecorder.SetScores(mergedScores);
            var scoreRecords = scores.Select(pair => new ScoreRecord(pair.Key, pair.Value)).ToList();
            Queries.SaveScoreChange(scoreRecords);

            if (Settings.ShowDebugWindow)
            {
                _debugWindow.Dispatcher.Invoke(delegate
                {
                    _debugWindowDataContext.AddLogMessage(sender, "Scores changed");
                    _debugWindowDataContext.SetScores(scoreRecords);
                });
            }
        }

        private void OnWindowsChanged(object sender, List<IntPtr> e)
        {
            var topWindows = e;
            _windowRecorder.SetTopWindows(topWindows);
            UpdateDrawing(topWindows);
        }

        private void StartTreatment()
        {
            _modelEvents.MoveStarted += OnMoveStarted;
            _modelEvents.MoveEnded += OnMoveEnded;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            _hazeOverlay.Start();
        }

        private void StopTreatment()
        {
            _modelEvents.MoveStarted -= OnMoveStarted;
            _modelEvents.MoveEnded -= OnMoveEnded;
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            _hazeOverlay.Stop();
        }

        private void UpdateDrawing(List<IntPtr> topWindows)
        {
            var scoredWindows = GetScoredWindows(topWindows, _windowStack.WindowRecords).ToList();
            var scoredWindowRectangles = scoredWindows
                .Select(tuple => (tuple.windowRecord, rectangle: WindowUtils.GetCorrectedWindowRectangle(tuple.windowRecord), tuple.show))
                .ToList();
            _hazeOverlay.Show(scoredWindowRectangles.Select(tuple => (tuple.rectangle, tuple.show)));

            var allWindowRectangles = scoredWindowRectangles
                .Concat(_windowStack.WindowRecords
                    .Skip(scoredWindows.Count)
                    .Select(windowRecord => (windowRecord, rectangle: WindowUtils.GetCorrectedWindowRectangle(windowRecord), show: false))
                ).ToList();
            var desktopWindowRecords = allWindowRectangles
                .Select((tuple, i) => new DesktopWindowRecord(tuple.windowRecord.Handle, !tuple.show, i, tuple.rectangle));
            Queries.SaveDesktopEvents(desktopWindowRecords);

            if (Settings.ShowDebugWindow)
            {
                _debugWindow.Dispatcher.Invoke(delegate { _debugWindowDataContext.SetDrawList(allWindowRectangles); });
            }
        }

        private void OnMoveStarted(object sender, EventArgs e)
        {
            _hazeOverlay.Hide();
        }

        private void OnMoveEnded(object sender, EventArgs e)
        {
            var topWindows = _modelCore.GetTopWindows();
            UpdateDrawing(topWindows);
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            _hazeOverlay.ReloadMonitors();
        }
    }
}
