using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WindowRecommender.Data;

namespace WindowRecommender.DebugWindow
{
    internal class DebugWindowDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const int MaxLength = 10;

        public ObservableCollection<LogEntryDataSource> Log { get; }
        public ObservableCollection<DrawListEntryDataSource> DrawList { get; private set; }
        public ObservableCollection<ScoreRecordDataSource> Scores { get; private set; }

        internal DebugWindowDataContext()
        {
            Log = new ObservableCollection<LogEntryDataSource>();
            DrawList = new ObservableCollection<DrawListEntryDataSource>();
            Scores = new ObservableCollection<ScoreRecordDataSource>();
        }

        internal void AddLogMessage(object sender, string message)
        {
            if (Log.Count == MaxLength)
            {
                Log.RemoveAt(Log.Count - 1);
            }

            var model = sender.GetType().Name;
            Log.Insert(0, new LogEntryDataSource(DateTime.Now, model, message));
        }

        public void SetDrawList(IEnumerable<(WindowRecord windowRecord, bool show)> drawList)
        {
            DrawList = new ObservableCollection<DrawListEntryDataSource>(drawList.Select(tuple => new DrawListEntryDataSource(tuple.windowRecord.Handle, tuple.windowRecord.Title, tuple.show)));
            OnPropertyChanged(nameof(DrawList));
        }

        public void SetScores(IEnumerable<ScoreRecord> scores)
        {
            Scores = new ObservableCollection<ScoreRecordDataSource>(scores.Select(record => new ScoreRecordDataSource(record)));
            OnPropertyChanged(nameof(Scores));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    internal class LogEntryDataSource
    {
        public DateTime Timestamp { get; }
        public string Model { get; }
        public string Message { get; }

        internal LogEntryDataSource(DateTime timestamp, string model, string message)
        {
            Timestamp = timestamp;
            Model = model;
            Message = message;
        }
    }

    internal class DrawListEntryDataSource
    {
        public string WindowHandle { get; }
        public string WindowTitle { get; }
        public bool Show { get; }

        public DrawListEntryDataSource(IntPtr windowHandle, string windowTitle, bool show)
        {
            WindowHandle = windowHandle.ToString();
            WindowTitle = windowTitle;
            Show = show;
        }
    }

    internal class ScoreRecordDataSource
    {
        public string WindowHandle { get; }
        public double Merged { get; }
        public double Duration { get; }
        public double Frequency { get; }
        public double MostRecentlyActive { get; }
        public double TitleSimilarity { get; }

        public ScoreRecordDataSource(ScoreRecord scoreRecord)
        {
            WindowHandle = scoreRecord.WindowHandle;
            Merged = Math.Round(scoreRecord.Merged, 2);
            Duration = Math.Round(scoreRecord.Duration, 2);
            Frequency = Math.Round(scoreRecord.Frequency, 2);
            MostRecentlyActive = Math.Round(scoreRecord.MostRecentlyActive, 2);
            TitleSimilarity = Math.Round(scoreRecord.TitleSimilarity, 2);
        }
    }
}
