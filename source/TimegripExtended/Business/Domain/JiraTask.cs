﻿using System;
using System.Text;

namespace TimegripExtended.Business.Domain
{
    public class JiraTask
    {
        public string Task { get; set; }
        public string Title { get; set; }
        public TimeSpan Estimate { get; set; }
        public TimeSpan Timespent { get; set; }
        public string Status { get; set; }
        
        public bool IsClosed
        {
            get { return Status == "Closed"; }
        }

        public override string ToString()
        {
            const string baseString = "Task: {0}, Estimate: {1}, Timespent: {2}";
            return string.Format(baseString, Task, Estimate, Timespent);
        }

        public DateTime Updated { get; set; }
    }
}