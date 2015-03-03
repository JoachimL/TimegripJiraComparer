using System;
using System.Text;

namespace TimegripExtended.Business.Domain
{
    public class JiraAndTimegrip
    {
        public JiraTask Task { get; set; }
        public TimegripActivity Activity { get; set; }

        public string TaskCode
        {
            get { return Task.Task; }
        }

        public string TaskName
        {
            get { return Task.Task; }
        }

        public TimeSpan Estimate
        {
            get { return Task.Estimate; }
        }

        public TimeSpan Timespent
        {
            get { return Task.Timespent; }
        }

        public TimeSpan TimeUsed
        {
            get { return Activity.TimeUsed; }
        }

        public bool IsClosed
        {
            get { return Task.IsClosed; }
        }

        public string Status
        {
            get
            {
                return Task.Status;
            }
        }

        public TimeSpan Timediff
        {
            get { return Task.Estimate - Activity.TimeUsed; }
        }

        public string Comment
        {
            get
            {
                var sb = new StringBuilder();

                if (Estimate == TimeSpan.Zero)
                {
                    sb.AppendLine("Estimate is missing");
                }
                else if (Estimate < TimeUsed)
                {
                    sb.AppendLine("Too much time is used");
                }
                else
                {
                    sb.AppendLine("Estimate: OK!");
                }

                if (TimeUsed != Timespent)
                {
                    sb.AppendLine("Jira and Timegrip is not in sync");
                }

                return sb.ToString();
            }
        }



        public DateTime Updated { get; set; }
    }
}