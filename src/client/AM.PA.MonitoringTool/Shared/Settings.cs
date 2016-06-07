﻿// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-10-20
// 
// Licensed under the MIT License.

using Shared.Data;
using System;
using System.IO;

namespace Shared
{
    public static class Settings
    {
        public const bool IsUploadEnabled = false;
        public const bool IsFeedbackEnabled = false;

        public const bool AnonymizeSensitiveData = false;
        public const bool PrintQueriesToConsole = false;

        internal const string LogDbTable = "log";
        public const string SettingsDbTable = "settings";
        public const string FeedbackDbTable = "feedback";

        public const string WindowsActivityTable = "windows_activity"; //used for the retrospection
        public const string UserEfficiencySurveyTable = "user_efficiency_survey"; // used for the retrospection
        public const string EmailsTable = "emails"; // used for the retrospection
        public const string MeetingsTable = "meetings";  // used for the retrospection
        public const string CallsTable = "calls"; // used for the retrospection
        public const string ChatsTable = "chats"; // used for the retrospection
        public const string InteractionsTable = "interactions"; // used for the retrospection (TODO: remove later)

        public static TimeSpan CheckForToolUpdatesInterval = TimeSpan.FromHours(5); // every 5 hours
        public static TimeSpan TooltipIconUpdateInterval = TimeSpan.FromSeconds(10); // every 10 seconds
        public static TimeSpan RemindToResumeToolInterval = TimeSpan.FromMinutes(30); // every 30 minutes

        public static TimeSpan IntervalCheckThresholds = TimeSpan.FromMinutes(10); // every 10 mins
        public const bool IsAutoMiniRetrospectionPopUpEnabled = true;

        public const int Port = 57827; // needed for the retrospection (local web server)
        public static string ExportFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersonalAnalytics");
        //public static string RemoteFolderName = "remote\\";

        public const string RegAppName = "PersonalDeveloperAnalytics"; // set manually

        // path (Regedit): Computer\HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
        public const string RegAppPath = @"\Personal Analytics\Personal Analytics\Personal Analytics.appref-ms"; // change also publisher name in .csproj
        // was: public const string RegAppPath = @"\Andre Meyer (S.E.A.L., University of Zurich)\Personal Analytics\Personal Analytics.appref-ms";

        ////////////////////////////////////////////////////////////
        // retrospection constants
        // hint: any changes must also be done in styles_css
        internal static double ItemSmallWidthEm = (11.875 - sideMargin);
        internal static double ItemSmallHeightEm = (10 - titleMargin);
        internal static double ItemSquareWidthEm = (25 - sideMargin);
        internal static double ItemSquareHeightEm = (21.25 - titleMargin);
        internal static double ItemWideWidthEm = (51.25 - sideMargin);
        internal static double ItemWideHeightEm = (21.25 - titleMargin);
        internal const double sideMargin = 1.25; //2.5; //1.25;
        internal const double titleMargin = 4.0; //5.625; //3.125;

        ////////////////////////////////////////////////////////////
        // contact 
        public const string ProjectWebsite = "http://www.focuslight.com"; 
        public const string EmailAddress1 = "focuslight@abb.com"; //"ameyer@ifi.uzh.ch"; // main email address
        public const string EmailAddress2 = ""; // no needed for now

    }
}
