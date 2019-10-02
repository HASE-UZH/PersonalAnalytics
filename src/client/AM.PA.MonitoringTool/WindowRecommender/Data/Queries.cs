using Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using WindowRecommender.Graphics;

namespace WindowRecommender.Data
{
    internal static class Queries
    {
        private static readonly (string name, string columns)[] Tables = {
            (Settings.WindowEventTable, "id INTEGER PRIMARY KEY, time TEXT, event Text, windowHandle TEXT, processName TEXT, windowTitle TEXT, zIndex INTEGER, rank INTEGER, score REAL"),
            (Settings.ScoreChangeTable, "id INTEGER PRIMARY KEY, time TEXT, windowHandle TEXT, mergedScore REAL, durationScore REAL, frequencyScore REAL, mraScore REAL, titleScore REAL"),
            (Settings.DesktopEventTable, "id INTEGER PRIMARY KEY, time TEXT, windowHandle TEXT, zIndex INTEGER, hazed INTEGER, left INTEGER, top INTEGER, right INTEGER, bottom INTEGER"),
            (Settings.ScreenEventTable, "id INTEGER PRIMARY KEY, time TEXT, screenId INTEGER, left INTEGER, top INTEGER, right INTEGER, bottom INTEGER"),
            (Settings.PopupResponseTable, "id INTEGER PRIMARY KEY, time TEXT, windowHandle TEXT, relevant INTEGER, hazed INTEGER"),
        };

        internal static void CreateTables()
        {
            foreach (var (name, columns) in Tables)
            {
                Database.GetInstance().ExecuteDefaultQuery($@"CREATE TABLE IF NOT EXISTS {name} ({columns});");
            }
        }

        internal static void DropTables()
        {
            foreach (var (name, _) in Tables)
            {
                Database.GetInstance().ExecuteDefaultQuery($@"DROP TABLE IF EXISTS {name};");
            }
        }

        #region Save Events

        internal static void SaveWindowEvent(EventName eventName, WindowEventRecord entry)
        {
            var db = Database.GetInstance();
            var query = $@"INSERT INTO {Settings.WindowEventTable} (time, event, windowHandle, processName, windowTitle, zIndex, rank, score) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
            var parameters = new object[] { DateTime.Now, eventName.ToString(), entry.WindowHandle, entry.ProcessName, entry.WindowTitle, entry.ZIndex, entry.Rank, entry.Score };
            db.ExecuteDefaultQuery(query, parameters);
        }

        internal static void SaveWindowEvents(EventName eventName, IEnumerable<WindowEventRecord> entries)
        {
            var db = Database.GetInstance();
            var query = $@"INSERT INTO {Settings.WindowEventTable} (time, event, windowHandle, processName, windowTitle, zIndex, rank, score) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
            var timestamp = DateTime.Now;
            var parameterList = entries
                .Select(entry => new object[] { timestamp, eventName.ToString(), entry.WindowHandle, entry.ProcessName, entry.WindowTitle, entry.ZIndex, entry.Rank, entry.Score })
                .ToArray();
            db.ExecuteBatchQueries(query, parameterList);
        }

        internal static void SaveScoreChange(IEnumerable<ScoreRecord> scoreRecords)
        {
            var db = Database.GetInstance();
            var query = $@"INSERT INTO {Settings.ScoreChangeTable} (time, windowHandle, mergedScore, durationScore, frequencyScore, mraScore, titleScore) VALUES (?, ?, ?, ?, ?, ?, ?);";
            var timestamp = DateTime.Now;
            var parameterList = scoreRecords
                .Select((record, i) => new object[] { timestamp, record.WindowHandle, record.Merged, record.Duration, record.Frequency, record.MostRecentlyActive, record.TitleSimilarity })
                .ToArray();
            db.ExecuteBatchQueries(query, parameterList);
        }

        internal static void SaveDesktopEvents(IEnumerable<DesktopWindowRecord> desktopWindowRecords)
        {
            var db = Database.GetInstance();
            var query = $@"INSERT INTO {Settings.DesktopEventTable} (time, windowHandle, zIndex, hazed, left, top, right, bottom) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
            var timestamp = DateTime.Now;
            var parameterList = desktopWindowRecords
                .Select((record, i) => new object[] { timestamp, record.WindowHandle, record.ZIndex, record.Hazed, record.Rectangle.Left, record.Rectangle.Top, record.Rectangle.Right, record.Rectangle.Bottom })
                .ToArray();
            db.ExecuteBatchQueries(query, parameterList);
        }

        internal static void SaveScreenEvents(IEnumerable<Rectangle> screenRectangles)
        {
            var db = Database.GetInstance();
            var query = $@"INSERT INTO {Settings.ScreenEventTable} (time, screenId, left, top, right, bottom) VALUES (?, ?, ?, ?, ?, ?);";
            var timestamp = DateTime.Now;
            var parameterList = screenRectangles
                .Select((screenRectangle, i) => new object[] { timestamp, i, screenRectangle.Left, screenRectangle.Top, screenRectangle.Right, screenRectangle.Bottom })
                .ToArray();
            db.ExecuteBatchQueries(query, parameterList);
        }

        internal static void SavePopupResponses(IEnumerable<(string handle, bool relevant, bool hazed)> responses)
        {
            var db = Database.GetInstance();
            var query = $@"INSERT INTO {Settings.PopupResponseTable} (time, windowHandle, relevant, hazed) VALUES (?, ?, ?, ?);";
            var timestamp = DateTime.Now;
            var parameterList = responses
                .Select(response => new object[] { timestamp, response.handle, response.relevant, response.hazed })
                .ToArray();
            db.ExecuteBatchQueries(query, parameterList);
        }
        #endregion

        #region Settings

        internal static bool GetEnabledSetting()
        {
            var db = Database.GetInstance();
            return db.GetSettingsBool(Settings.EnabledSettingDatabaseKey, Settings.EnabledDefault);
        }

        internal static void SetEnabledSetting(bool enabled)
        {
            var db = Database.GetInstance();
            db.SetSettings(Settings.EnabledSettingDatabaseKey, enabled);
            db.LogInfo($"The participant updated the setting '{Settings.EnabledSettingDatabaseKey}' to {enabled}.");
        }

        internal static bool GetTreatmentModeSetting()
        {
            var db = Database.GetInstance();
            return db.GetSettingsBool(Settings.TreatmentModeSettingDatabaseKey, Settings.TreatmentModeDefault);
        }

        internal static void SetTreatmentModeSetting(bool treatmentMode)
        {
            var db = Database.GetInstance();
            db.SetSettings(Settings.TreatmentModeSettingDatabaseKey, treatmentMode);
            db.LogInfo($"The participant updated the setting '{Settings.TreatmentModeSettingDatabaseKey}' to {treatmentMode}.");
        }

        internal static int GetNumberOfWindowsSetting()
        {
            var db = Database.GetInstance();
            return db.GetSettingsInt(Settings.NumberOfWindowsSettingDatabaseKey, Settings.NumberOfWindowsDefault);
        }

        internal static void SetNumberOfWindowsSetting(int numberOfWindows)
        {
            var db = Database.GetInstance();
            db.SetSettings(Settings.NumberOfWindowsSettingDatabaseKey, numberOfWindows.ToString());
            db.LogInfo($"The participant updated the setting '{Settings.NumberOfWindowsSettingDatabaseKey}' to {numberOfWindows}.");
        }

        #endregion
    }
}
