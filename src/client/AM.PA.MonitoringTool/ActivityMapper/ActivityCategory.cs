// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2017-01-04
// 
// Licensed under the MIT License.

namespace ActivityMapper
{
    /// <summary>
    /// Activitiy categories which are automatically mapped for the retrospection
    /// by the ProcessToActivityMapper
    /// </summary>
    public enum ActivityCategory
    {
        [Description("Uncategorized")]
        Uncategorized, // default

        [Description("Development")]
        DevCode,

        [Description("Debugger Use")]
        DevDebug,

        [Description("Code Reviewing")]
        DevReview,

        [Description("Version Control")]
        DevVc,

        [Description("Emails")]
        Email,

        [Description("Planning")]
        Planning,

        [Description("Reading/Editing Documents")]
        ReadWriteDocument,

        [Description("Scheduled meetings")]
        PlannedMeeting,

        [Description("Instant Messaging")]
        InformalMeeting,

        [Description("Instant Messaging")]
        InstantMessaging, // subcategory of InformalMeeting
                          //WebBrowsing, // uncategorized web browsing
        [Description("Browsing work-related")]
        WorkRelatedBrowsing,

        [Description("Browsing work-unrelated")]
        WorkUnrelatedBrowsing,

        [Description("Navigation in File Explorer")]
        FileNavigationInExplorer,

        [Description("Other")]
        Other,

        [Description("RDP (uncategorized)")]
        OtherRdp,

        [Description("Idle (e.g. break, lunch, meeting)")]
        Idle, // all IDLE events that can't be mapped elsewhere

        [Description("Uncategorized")]
        Unknown
    }
}