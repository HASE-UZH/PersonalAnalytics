﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackTracker.Data.SlackModel
{
    class UserActivity
    {
        public DateTime time { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public double intensity { get; set; }
    }
}
