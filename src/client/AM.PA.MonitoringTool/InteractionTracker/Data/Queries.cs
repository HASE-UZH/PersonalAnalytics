﻿// Created by André Meyer at UZH and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Shared;
using Shared.Data;

namespace InteractionTracker.Data
{
    public class Queries
    {
        /// <summary>
        /// Returns the focused time for the last hour
        /// (which is 60 mins - time spent in Outlook, Skype, Lync)
        /// </summary>
        /// <returns></returns>
        internal static int GetFocusTimeInLastHour()
        {
            try
            {
                var today = DateTime.Now;

                var query = "SELECT (3600 - SUM(difference)) as 'difference'"
                          + "FROM ( "
                          + "SELECT (strftime('%s', t2.time) - strftime('%s', t1.time)) as 'difference' "
                          + "FROM " + Settings.WindowsActivityTable + " t1 LEFT JOIN " + Settings.WindowsActivityTable + " t2 on t1.id + 1 = t2.id "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t1.time") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t2.time") + " "
                          + "AND TIME(t2.time) between TIME('" + today.AddHours(-1).ToString("HH:mm") + "') and TIME('" + today.ToString("HH:mm") + "') "
                          + "AND ( LOWER(t1.process) == 'outlook' or LOWER(t1.process) == 'skype' or LOWER(t1.process) == 'lync')"
                          + "GROUP BY t1.id, t1.time "
                          + "ORDER BY difference DESC)";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var difference = Convert.ToInt32(row["difference"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return difference;
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// TODO: Delete?
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static Dictionary<string, double> GetActivityStepChartData(DateTimeOffset date)
        {
            var dto = new Dictionary<string, double>();
            //var query = "SELECT * FROM ";
            // var table = Database.GetInstance().ExecuteReadQuery(query);

            return dto;
        }

        /// <summary>
        /// Gets the number of switches to Outlook, Skype, Skype for business
        /// for the last hour (times 2, because there is the switch back to work)
        /// within the last hour
        /// </summary>
        /// <returns></returns>
        public static int GetNumInteractionSwitches()
        {
            try
            {
                var today = DateTime.Now;

                var query = "SELECT (2 * COUNT(*)) as 'totalSwitches'"
                              + "FROM " + Settings.WindowsActivityTable + " t1 LEFT JOIN " + Settings.WindowsActivityTable + " t2 on t1.id + 1 = t2.id "
                              + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t1.time") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t2.time") + " "
                              + "AND TIME(t2.time) between TIME('" + today.AddHours(-1).ToString("HH:mm") + "') and TIME('" + today.ToString("HH:mm") + "') "
                              + "AND ( LOWER(t1.process) == 'outlook' or LOWER(t1.process) == 'skype' or LOWER(t1.process) == 'lync')"
                              + "AND (strftime('%s', t2.time) - strftime('%s', t1.time)) > 3;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var difference = Convert.ToInt32(row["totalSwitches"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return difference;
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// Counts the number of meetings for today
        /// </summary>
        /// <returns>returns the number of meetings for today</returns>
        public static int GetNumMeetingsForLastHour()
        {
            // get the meetings
            var meetings = new List<Tuple<DateTime, DateTime>>();
            try
            {
                var query = "SELECT time, durationInMins FROM " + Settings.MeetingsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, DateTime.Now.Date) + ";";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timeStart = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);
                    var duration = Convert.ToInt32(row["durationInMins"], CultureInfo.InvariantCulture);
                    var timeEnd = timeStart.AddMinutes(duration);

                    var t = new Tuple<DateTime, DateTime>(timeStart, timeEnd);
                    meetings.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            // count up the meetings in the last hour
            var count = -1;
            var startTs = DateTime.Now.AddHours(-1);
            var endTs = DateTime.Now;
            foreach (var meeting in meetings)
            {
                if (meeting.Item1 > startTs && meeting.Item2 < endTs) count += 2; // meeting occurs only during the hour
                else if (meeting.Item1 < startTs && meeting.Item2 < endTs) count++; // meeting starts before the hour and ends during the hour
                else if (meeting.Item1 > startTs && meeting.Item2 > endTs) count++; // meeting starts during the hour and ends after the hour
            }

            return count;
        }

        /// <summary>
        /// Counts the number of meetings for today
        /// </summary>
        /// <returns>returns the number of meetings for today</returns>
        public static int GetNumMeetingsForDate(DateTimeOffset date)
        {
            try
            {
                var answer = "SELECT COUNT(*) FROM " + Settings.MeetingsTable + " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date.Date) + ";";
                var count = Database.GetInstance().ExecuteScalar(answer);
                return count;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns meetings that have occurred since 6 am until now</returns>
        internal static List<Tuple<DateTime, DateTime>> GetMeetingsFromSixAm(DateTime later)
        {
            var endTs = later;
            var startTs = endTs.Date.AddHours(6); // 6 am today
            var current = new List<Tuple<DateTime, DateTime>>();
            var meetings = new List<Tuple<DateTime, DateTime>>();

            try
            {
                var query = "SELECT time, durationInMins FROM " + Settings.MeetingsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, endTs.Date) + ";"; // DateTime.Now.Date

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timeStart = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);
                    var duration = Convert.ToInt32(row["durationInMins"], CultureInfo.InvariantCulture);
                    var timeEnd = timeStart.AddMinutes(duration);

                    var t = new Tuple<DateTime, DateTime>(timeStart, timeEnd);
                    meetings.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            foreach (var meeting in meetings)
            {
                if (meeting.Item1 > startTs && meeting.Item2 < endTs)
                {
                    var t = new Tuple<DateTime, DateTime>(meeting.Item1, meeting.Item2);
                    current.Add(t);
                }
                else if (meeting.Item1 < startTs && meeting.Item2 < endTs)
                {
                    var t = new Tuple<DateTime, DateTime>(startTs, meeting.Item2);
                    current.Add(t);
                }
                else if (meeting.Item1 > startTs && meeting.Item2 > endTs)
                {
                    var t = new Tuple<DateTime, DateTime>(meeting.Item1, endTs);
                    current.Add(t);
                }
            }
            return current;
        }

        /// <summary>
        /// The emails sent or received
        /// </summary>
        /// <returns>Tuple item1: sent, item2: received</returns>
        /// <summary>
        /// The emails sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <param name="sentOrReceived"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        public static int GetSentOrReceivedEmails(DateTimeOffset date, string sentOrReceived)
        {
            try
            {
                var query = "SELECT time, " + sentOrReceived + " FROM " + Settings.EmailsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                            + "ORDER BY time DESC "
                            + "LIMIT 1;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var answer = Convert.ToInt32(row[sentOrReceived], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return answer;
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns emails that have occurred since 6 am until now</returns>
        internal static List<DateTime> GetEmailsSentOrReceivedFromSixAm(DateTime later, string sentOrReceived)
        {
            var endTs = later;
            var startTs = endTs.Date.AddHours(6); // 6 am today
            var current = new List<DateTime>();
            var emails = new List<DateTime>();

            try
            {
                var query = "SELECT * FROM " + Settings.InteractionsTable
                            + " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, endTs.Date) // DateTime.Now.Date
                            + "AND interactionType = 'email'"
                            + " AND sentType = '" + sentOrReceived + "';"; 

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var t = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);
                    emails.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            foreach (var email in emails)
            {
                if (email >= startTs)
                {
                    current.Add(email);
                }
            }
            return current;
        }


        /// <summary>
        /// The calls sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        public static int GetNumCallsOrChats(DateTimeOffset date, string tableName)
        {
            try
            {
                var query = "SELECT * FROM " + tableName + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                            + "ORDER BY time DESC "
                            + "LIMIT 1;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var sent = Convert.ToInt32(row["sent"], CultureInfo.InvariantCulture);
                    var received = Convert.ToInt32(row["received"], CultureInfo.InvariantCulture);
                    var timestamp = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return (sent + received);
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns chats that have occurred since 6 am until now</returns>
        internal static List<DateTime> GetChatsSentOrReceivedFromSixAm(DateTime later)//(string sentOrReceived)
        {
            var endTs = later;
            var startTs = endTs.Date.AddHours(6); // 6 am today
            var current = new List<DateTime>();
            var chats = new List<DateTime>();

            try
            {
                var query = "SELECT * FROM " + Settings.InteractionsTable
                            + " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, endTs.Date) // DateTime.Now.Date
                            + "AND interactionType = 'chat'"
                            + ";";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var t = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);
                    chats.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            foreach (var chat in chats)
            {
                if (chat >= startTs)
                {
                    current.Add(chat);
                }
            }
            return current;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns calls that have occurred since 6 am until now</returns>
        internal static List<DateTime> GeCallsSentOrReceivedFromSixAm(DateTime later, string sentOrReceived)
        {
            var endTs = later;
            var startTs = endTs.Date.AddHours(6); // 6 am today
            var current = new List<DateTime>();
            var calls = new List<DateTime>();

            try
            {
                var query = "SELECT * FROM " + Settings.InteractionsTable
                            + " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, endTs.Date) // DateTime.Now.Date
                            + "AND interactionType = 'call'"
                            + " AND sentType = '" + sentOrReceived + "';";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var t = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);
                    calls.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            foreach (var call in calls)
            {
                if (call >= startTs)
                {
                    current.Add(call);
                }
            }
            return current;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A dictionary that contains the meetings and their chart values</returns>
        internal static Dictionary<string, List<int>> GetActivityStepChartData(DateTime earlier, DateTime later)
        {
            var activityDictionary = new Dictionary<string, List<int>>();

            var chatsList = new List<int>();
            var emailsReceivedList = new List<int>();
            var emailsSentList = new List<int>();
            var meetingsAttendedList = new List<int>();
            var overallFocusList = new List<int>();

            var chats = GetChatsSentOrReceivedFromSixAm(later);
            var emailsSent = GetEmailsSentOrReceivedFromSixAm(later, "sent");
            var emailsReceived = GetEmailsSentOrReceivedFromSixAm(later, "received");
            var meetings = GetMeetingsFromSixAm(later);

            bool didHappen = false;

            TimeSpan span = later.Subtract(earlier);
            int minutes = (int)Math.Floor(span.TotalMinutes);

            var notFocused = 0;

            for (int i = 0; i < minutes; i++)
            {
                foreach (var chat in chats)
                {
                    if ((chat.AddSeconds(-30) <= earlier) && (chat.AddSeconds(30) >= earlier))
                    {
                        chatsList.Add(1);
                        notFocused = 1;
                        didHappen = true;
                        break;
                    }
                }

                if (!didHappen)
                {
                    chatsList.Add(0);
                }
                didHappen = false;

                // For Emails Sent
                foreach (var email in emailsSent)
                {
                    if ((email.AddSeconds(-30) <= earlier) && (email.AddSeconds(30) >= earlier))
                    {
                        emailsSentList.Add(1);
                        notFocused = 1;
                        didHappen = true;
                        break;
                    }
                }

                if (!didHappen)
                {
                    emailsSentList.Add(0);
                }
                didHappen = false;

                // For Emails Received
                foreach (var email in emailsReceived)
                {
                    if ((email.AddSeconds(-30) <= earlier) && (email.AddSeconds(30) >= earlier))
                    {
                        emailsReceivedList.Add(1);
                        notFocused = 1;
                        didHappen = true;
                        break;
                    }
                }

                if (!didHappen)
                {
                    emailsReceivedList.Add(0);
                }
                didHappen = false;

                // For Meetings
                foreach (var meeting in meetings)
                {
                    if ((meeting.Item1 <= earlier) && (meeting.Item2 >= earlier))
                    {
                        meetingsAttendedList.Add(1);
                        notFocused = 1;
                        didHappen = true;
                        break;
                    }
                }

                if (!didHappen)
                {
                    meetingsAttendedList.Add(0);
                }
                didHappen = false;

                overallFocusList.Add(notFocused);
                notFocused = 0;

                earlier = earlier.AddMinutes(1);
            }

            activityDictionary.Add("Meetings Attended", meetingsAttendedList);
            activityDictionary.Add("Chats", chatsList);
            activityDictionary.Add("Emails Sent", emailsSentList);
            activityDictionary.Add("Emails Received", emailsReceivedList);
            activityDictionary.Add("Overall Focus", overallFocusList);
            return activityDictionary;
        }
    }
}