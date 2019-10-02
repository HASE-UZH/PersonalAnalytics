// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-10-20
// 
// Licensed under the MIT License.
using System;

namespace Shared.Data
{
    public class FocusedWorkDto
    {
        public string Process { get; set; }
        public int DurationInSec { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public FocusedWorkDto(string process, int durationInSec, DateTime from, DateTime to)
        {
            Process = process;
            DurationInSec = durationInSec;
            From = from;
            To = to;
        }
    }

    public class SettingsDto
    {
        public bool? PopUpEnabled { get; set; }
        public int? PopUpInterval { get; set; }
        public bool? UserInputTrackerEnabled { get; set; }
        public bool? Office365ApiEnabled { get; set; }
        public bool? OpenRetrospectionInFullScreen { get; set; }
        public bool? TimeSpentShowEmailsEnabled { get; set; }
        public bool? TimeSpentHideMeetingsWithoutAttendeesEnabled { get; set; }
        public bool? TimeSpentShowProgramsEnabled { get; set; }
        public bool? PolarTrackerEnabled { get; set; }
        public bool? FitbitTrackerEnabled { get; set; }
        public bool? FitbitTokenRevokeEnabled { get; set; }
        public bool? FitbitTokenRevoked { get; set; }
        public bool? WindowRecommenderEnabled { get; set; }
        public bool? WindowRecommenderTreatmentMode { get; set; }
        public int? WindowRecommenderNumberOfWindows { get; set; }
    }
    public class StartEndTimeDto
    {
        public long StartTime { get; set; }
        public long EndTime { get; set; }
    }

    public class ProductivityTimeDto
    {
        public int UserProductvity { get; set; }
        public long Time { get; set; }
    }

    public class TasksWorkedOnTimeDto
    {
        public int TasksWorkedOn { get; set; }
        public long Time { get; set; }
    }

    //public class ContextDto
    //{
    //    public long StartTime { get; set; }
    //    public long EndTime { get; set; }

    //    public ContextInfos Context { get; set; }
    //}
}
