// Created by André Meyer at MSR
// Created: 2015-11-12
// 
// Licensed under the MIT License.
using Shared;
using Shared.Data;
using Shared.Data.Extractors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using ActivityMapper;
using WindowsActivityTracker.Models;

namespace WindowsActivityTracker.Data
{
    public class Queries
    {

        private static readonly string QUERY_CREATE = "CREATE TABLE IF NOT EXISTS " + Settings.DbTable + " (id INTEGER PRIMARY KEY, time TEXT, tsStart TEXT, tsEnd TEXT, window TEXT, process TEXT);";
        private static readonly string QUERY_INDEX = "CREATE INDEX IF NOT EXISTS windows_activity_ts_start_idx ON " + Settings.DbTable + " (tsStart);";
        private static readonly string QUERY_INSERT = "INSERT INTO " + Settings.DbTable + " (time, tsStart, tsEnd, window, process) VALUES ({0}, {1}, {2}, {3}, {4});";

        #region Daemon Queries

        internal static void CreateWindowsActivityTable()
        {
            try
            {
                var res = Database.GetInstance().ExecuteDefaultQuery(QUERY_CREATE);
                if (res == 1) Database.GetInstance().ExecuteDefaultQuery(QUERY_INDEX); // add index when table was newly created
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        internal static void UpdateDatabaseTables(int version)
        {
            try
            {
                // database update 2017-07-20 (added two columns to 'windows_activity' table: tsStart and tsEnd)
                // need to migrate the existing values in the table
                if (version == 4)
                {
                    if (Database.GetInstance().HasTable(Settings.DbTable))
                    {
                        // update table: add columns & index
                        Database.GetInstance().ExecuteDefaultQuery("ALTER TABLE " + Settings.DbTable + " ADD COLUMN tsStart TEXT;");
                        Database.GetInstance().ExecuteDefaultQuery("ALTER TABLE " + Settings.DbTable + " ADD COLUMN tsEnd TEXT;");
                        Database.GetInstance().ExecuteDefaultQuery(QUERY_INDEX);

                        // migrate data (set tsStart / tsEnd)
                        MigrateWindowsActivityTable();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        /// <summary>
        /// Updates all entries, sets time -> tsStart and time (of next item) -> tsEnd (or empty if end of the day)
        /// </summary>
        private static void MigrateWindowsActivityTable()
        {
            try
            {
                // copy time -> tsStart
                Database.GetInstance().ExecuteDefaultQuery("UPDATE " + Settings.DbTable + " set tsStart = time;");

                // copy tsStart of next item to tsEnd of this item
                var QUERY_UPDATE_TSEND = "UPDATE " + Settings.DbTable + " SET tsEnd = (SELECT t2.tsStart FROM " + Settings.DbTable + " t2 WHERE windows_activity.id + 1 = t2.id LIMIT 1);";
                Database.GetInstance().ExecuteDefaultQuery(QUERY_UPDATE_TSEND);

                // set tsEnd
                //var querySelect = "SELECT id, tsStart FROM " + Settings.DbTable + ";"; // LIMIT 10000;";
                //var table = Database.GetInstance().ExecuteReadQuery(querySelect);

                //if (table != null)
                //{
                //    WindowsActivity _previousItem = null;

                //    foreach (DataRow row in table.Rows)
                //    {
                //        // read values for this item
                //        var currentItem_Id = (long)row["id"];
                //        var urrentItem_tsStart = DateTime.Parse((string)row["tsStart"], CultureInfo.InvariantCulture);

                //        // update and store previous item
                //        if (_previousItem != null)
                //        {
                //            var tsEndString = (_previousItem.StartTime.Day == urrentItem_tsStart.Day)
                //                                ? Database.GetInstance().QTime2(urrentItem_tsStart) // previous items' tsEnd is current items' tsStart
                //                                : "''"; // if end of day: keep empty

                //            var queryUpdate = "UPDATE " + Settings.DbTable + " SET tsEnd = " + tsEndString + " WHERE id = '" + _previousItem.Id + "';";
                //            Database.GetInstance().ExecuteDefaultQuery(queryUpdate);
                //            //Logger.WriteToConsole(queryUpdate);
                //        }

                //        // set new previous item
                //        _previousItem = new WindowsActivity() { Id = (int)currentItem_Id, StartTime = urrentItem_tsStart }; //tsEnd is not yet known
                //    }
                //    table.Dispose();
                //}
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        /// <summary>
        /// Saves the timestamp, start and end, process name and window title into the database.
        /// 
        /// In case the user doesn't want the window title to be stored (For privacy reasons),
        /// it is obfuscated.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="process"></param>
        internal static void InsertSnapshot(WindowsActivityEntry entry)
        {
            try
            {
                // if user is browsing in InPrivate-mode, obfuscate window title (doesn't work in Google Chrome!)
                if (ProcessToActivityMapper.IsBrowser(entry.Process) && Settings.InkognitoBrowsingTerms.Any(entry.WindowTitle.ToLower().Contains))
                {
                    entry.WindowTitle = Dict.Anonymized;  // obfuscate window title
                }

                // if user enabled private tracking, obfuscate window title
                if (Shared.Settings.AnonymizeSensitiveData)
                {
                    var activityCategory = ProcessToActivityMapper.Map(entry.Process, entry.WindowTitle);
                    entry.WindowTitle = string.Format("{0} (category: {1})", Dict.Anonymized, activityCategory);  // obfuscate window title
                }

                // if end time is missing, don't store anything
                if (entry.TsEnd == DateTime.MinValue)
                {
                    Database.GetInstance().LogWarning("TsEnd of WindowsActivitySwitch was empty.");
                    return;
                }

                var query = string.Format(QUERY_INSERT,
                                          "strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')",
                                          Database.GetInstance().QTime2(entry.TsStart),
                                          Database.GetInstance().QTime2(entry.TsEnd),
                                          Database.GetInstance().Q(entry.WindowTitle),
                                          Database.GetInstance().Q(entry.Process));

                Database.GetInstance().ExecuteDefaultQuery(query);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        internal static bool UserInputTableExists()
        {
            var res = Database.GetInstance().HasTable(Shared.Settings.UserInputTable);
            return res;
        }

        /// <summary>
        /// Returns a list with tsStart and tsEnd of all missed sleep events
        /// </summary>
        /// <param name="ts_checkFrom"></param>
        /// <param name="ts_checkTo"></param>
        /// <returns></returns>
        internal static List<Tuple<long, DateTime, DateTime>> GetMissedSleepEvents(DateTime ts_checkFrom, DateTime ts_checkTo)
        {
            var results = new List<Tuple<long, DateTime, DateTime>>();

            try
            {
                var query = "SELECT wa.id, wa.tsStart, wa.tsEnd, ( "
                          + "SELECT sum(ui.keyTotal) + sum(ui.clickTotal) + sum(ui.ScrollDelta) + sum(ui.movedDistance) "
                          + "FROM " + Shared.Settings.UserInputTable + " AS ui "
                          + "WHERE (ui.tsStart between wa.tsStart and wa.tsEnd) AND (ui.tsEnd between wa.tsStart and wa.tsEnd) "
                          + ") as 'sumUserInput' "
                          + "FROM " + Settings.DbTable + " AS wa "
                          + "WHERE wa.process <> '" + Dict.Idle + "' " // we are looking for cases where the IDLE event was not catched
                          + "AND wa.process <> 'skype' AND wa.process <> 'lync' " // IDLE during calls are okay
                          + "AND (wa.tsStart between " + Database.GetInstance().QTime(ts_checkFrom) + " AND " + Database.GetInstance().QTime(ts_checkTo) + ") " // perf
                          + "AND (strftime('%s', wa.tsEnd) - strftime('%s', wa.tsStart)) > " + Settings.IdleSleepValidate_ThresholdIdleBlocks_s + ";"; // IDLE time window we are looking for

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    if (row["sumUserInput"] == DBNull.Value || Convert.ToInt32(row["sumUserInput"]) == 0)
                    {
                        var id = (long)row["id"];
                        var tsStart = DateTime.Parse((string)row["tsStart"], CultureInfo.InvariantCulture);
                        var tsEnd = DateTime.Parse((string)row["tsEnd"], CultureInfo.InvariantCulture);
                        var tuple = new Tuple<long, DateTime, DateTime>(id, tsStart, tsEnd);
                        results.Add(tuple);
                    }
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return results;
        }

        internal static void AddMissedSleepIdleEntry(List<Tuple<long, DateTime, DateTime>> toFix)
        {
            foreach (var item in toFix)
            {
                var idleTimeFix = item.Item2.AddMilliseconds(Settings.NotCountingAsIdleInterval_ms);
                var tsEnd = item.Item3;

                // add missed sleep idle entry
                var tempItem = new WindowsActivityEntry(idleTimeFix, tsEnd, Settings.ManualSleepIdle, Dict.Idle, IntPtr.Zero);
                InsertSnapshot(tempItem);

                // update tsEnd of previous (wrong entry)
                var query = "UPDATE " + Settings.DbTable + " SET tsEnd = " + Database.GetInstance().QTime2(idleTimeFix) + " WHERE id = " + item.Item1;
                Database.GetInstance().ExecuteDefaultQuery(query);
            }

            if (toFix.Count > 0) Database.GetInstance().LogInfo("Fixed " + toFix.Count + " missed IDLE sleep entries.");
        }

        #endregion

        #region Visualization Queries

        /// <summary>
        /// Returns the program where the user focused on the longest
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static FocusedWorkDto GetLongestFocusOnProgram(DateTimeOffset date)
        {
            try
            {
                var query = "SELECT process, (strftime('%s', tsEnd) - strftime('%s', tsStart)) as 'difference', tsStart, tsEnd "
                          + "FROM " + Settings.DbTable + " "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsStart") + " AND " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsEnd") + " "
                          + "AND process <> '" + Dict.Idle + "' "
                          + "GROUP BY id, tsStart "
                          + "ORDER BY difference DESC "
                          + "LIMIT 1;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var process = Shared.Helpers.ProcessNameHelper.GetFileDescriptionFromProcess((string)row["process"]);
                    //var window = (string)row["window"];
                    var difference = Convert.ToInt32(row["difference"], CultureInfo.InvariantCulture);
                    var tsStart = DateTime.Parse((string)row["tsStart"], CultureInfo.InvariantCulture);
                    var tsEnd = DateTime.Parse((string)row["tsEnd"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return new FocusedWorkDto(process, difference, tsStart, tsEnd);
                }
                else
                {
                    table.Dispose();
                    return null;
                }  
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return null;
            }
        }

        /// <summary>
        /// Fetches the activities a developer has on his computer for a given date in an
        /// ordered list according to time.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static List<WindowsActivity> GetDayTimelineData(DateTimeOffset date)
        {
            var orderedActivityList = new List<WindowsActivity>();

            try
            {
                var query = "SELECT tsStart, tsEnd, window, process, (strftime('%s', tsEnd) - strftime('%s', tsStart)) as 'durInSec' "
                              + "FROM " + Settings.DbTable + " "
                              + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsStart") + " AND " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsEnd") + " "
                              + "ORDER BY tsStart;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null)
                {
                    WindowsActivity previousWindowsActivityEntry = null;

                    foreach (DataRow row in table.Rows)
                    {
                        // fetch items from database
                        var e = new WindowsActivity();
                        e.StartTime = DateTime.Parse((string)row["tsStart"], CultureInfo.InvariantCulture);
                        e.EndTime = DateTime.Parse((string)row["tsEnd"], CultureInfo.InvariantCulture);
                        e.DurationInSeconds = row.IsNull("durInSec") ? 0 : Convert.ToInt32(row["durInSec"], CultureInfo.InvariantCulture);
                        var processName = (string)row["process"];

                        // make window titles more readable (TODO: improve!)
                        var windowTitle = (string)row["window"];
                        windowTitle = WindowTitleWebsitesExtractor.GetWebsiteDetails(processName, windowTitle);
                        //windowTitle = WindowTitleArtifactExtractor.GetArtifactDetails(processName, windowTitle);
                        //windowTitle = WindowTitleCodeExtractor.GetProjectName(windowTitle);

                        // map process and window to activity
                        e.ActivityCategory = ProcessToActivityMapper.Map(processName, windowTitle);


                        // check if we add a new item, or merge with the previous one
                        if (previousWindowsActivityEntry != null)
                        {
                            // previous item is same, update it (duration and tsEnd)
                            if (e.ActivityCategory == previousWindowsActivityEntry.ActivityCategory)
                            {
                                var lastItem = orderedActivityList.Last();
                                lastItem.DurationInSeconds += e.DurationInSeconds;
                                lastItem.EndTime = e.EndTime;
                                lastItem.WindowProcessList.Add(new WindowProcessItem(processName, windowTitle));
                            }
                            // previous item is different, add it to list
                            else
                            {
                                e.WindowProcessList.Add(new WindowProcessItem(processName, windowTitle));
                                orderedActivityList.Add(e);
                            }
                        }
                        else // first item
                        {
                            orderedActivityList.Add(e);
                        }
                        previousWindowsActivityEntry = e;
                    }
                    table.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return orderedActivityList;
        }

        /// <summary>
        /// Fetches the activities a developer has on his computer for a given date and prepares the data
        /// to be visualized as a pie chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static Dictionary<string, double> GetActivityPieChartData(DateTimeOffset date)
        {
            var dto = new Dictionary<string, double>();

            try
            {
                var query = "SELECT process, sum(difference) / 60.0 / 60.0  as 'durInHrs' "
                          + "FROM (	" 
                          + "SELECT process, (strftime('%s', tsEnd) - strftime('%s', tsStart)) as 'difference' "
                          + "FROM " + Settings.DbTable + " "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsStart") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsEnd") + " "
                          + "GROUP BY id, tsStart"
                          + ") "
                          + "WHERE difference > 0 and process <> '" + Dict.Idle + "' "
                          + "GROUP BY process;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var process = (string)row["process"];
                        var fileDesc = Shared.Helpers.ProcessNameHelper.GetFileDescription(process);
                        var share = (double)row["durInHrs"];

                        if (dto.ContainsKey(fileDesc))
                        {
                            dto[fileDesc] += share;
                        }
                        else
                        {
                            dto.Add(fileDesc, share);
                        }
                    }
                    table.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return dto;
        }

        /// <summary>
        /// For a given date, return the total time spent at work (from first to last input)
        /// and the total time spent on the computer.
        /// In HOURS.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static Tuple<double, double> GetWorkTimeDetails(DateTimeOffset date)
        {
            try
            {
                var query = "SELECT sum(difference) / 60.0 / 60.0  as 'durInHrs' "
                            + "FROM ( "
                            + "SELECT (strftime('%s', tsEnd) - strftime('%s', tsStart)) as 'difference' "
                            + "FROM " + Settings.DbTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsStart") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "tsEnd")
                            + "AND process <> 'IDLE' );";

                var timeSpentActive = Database.GetInstance().ExecuteScalar3(query);
                var timeFirstEntry = Database.GetInstance().GetUserWorkStart(date);
                var timeLastEntry = Database.GetInstance().GetUserWorkEnd(date);
                var totalWorkTime = (timeLastEntry - timeFirstEntry).TotalHours;

                if (totalWorkTime < 0.2) totalWorkTime = 0.0;
                if (timeSpentActive < 0.2) timeSpentActive = 0.0;

                return new Tuple<double, double>(totalWorkTime, timeSpentActive);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return null;
            }
        }

        #endregion
    }
}
