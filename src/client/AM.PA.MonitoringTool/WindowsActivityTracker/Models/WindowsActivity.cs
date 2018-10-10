// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2017-01-04
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using ActivityMapper;

namespace WindowsActivityTracker.Models
{
    public class WindowsActivity
    {
        public int Id { get; set; }
        public string Participant { get; set; }
        public int EventNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationInSeconds { get; set; }
        public List<WindowProcessItem> WindowProcessList { get; set; }
        public ActivityCategory ActivityCategory { get; set; }

        public WindowsActivity()
        {
            WindowProcessList = new List<WindowProcessItem>();
        }
    }

    public class WindowProcessItem
    {
        public WindowProcessItem(string process, string window)
        {
            Process = process;
            WindowTitle = window;
        }

        public string WindowTitle { get; set; }
        public string Process { get; set; }
    }
}
