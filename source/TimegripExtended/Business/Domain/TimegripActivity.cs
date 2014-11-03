using System;

namespace TimegripExtended.Business.Domain
{
    public class TimegripActivity
    {
        public string Customer { get; set; }
        public string Project { get; set; }
        public string Activity { get; set; }

        private TimeSpan _timeUsed;
        public TimeSpan TimeUsed
        {
            get { return _timeUsed; }
            set { _timeUsed = value; }
        }
        public decimal Amount { get; set; }

        public bool IsActivity()
        {
            return TimeUsed != TimeSpan.Zero;
        }

        public override string ToString()
        {
            const string baseString = "Customer: {0}, Project: {1}, Activity: {2}, Timeused: {3}, Amount: {4}";
            return string.Format(baseString, Customer, Project, Activity, TimeUsed.Duration(), Amount);
        }
    }
}