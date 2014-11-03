using System;
using System.IO;
using System.Xml.Linq;
using TimegripExtended.Business.Domain;
using TimegripExtended.Business.Domain.Plurals;

namespace TimegripExtended.Converters
{
    public static class JiraTaskConverter
    {
        private static readonly XName ItemXName = "item";
        private static readonly XName KeyXName = "key";
        private static readonly XName EstimateXName = "timeoriginalestimate";
        private static readonly XName TimespentXName = "timespent";
        private static readonly XName SecondsXName = "seconds";
        private static readonly XName TitleXName = "title";
        private static readonly XName StatusXName = "status";

        public static JiraTasks ConvertToList(Stream stream)
        {
            var jiraTasks = new JiraTasks();

            var doc = XDocument.Load(stream);

            var root = doc.Root;
            if (root != null)
            {
                var channelNode = root.FirstNode;
                if (channelNode != null)
                {
                    var item = (XNode)((XElement)channelNode).Element(ItemXName);
                    while (item != null)
                    {
                        var xElement = (XElement)item;
                        var jiraTask = new JiraTask();

                        var element = xElement.Element(KeyXName);
                        if (element != null)
                        {
                            jiraTask.Task = element.Value;
                        }
                        
                        element = xElement.Element(TitleXName);
                        if (element != null)
                        {
                            jiraTask.Title = element.Value;
                        }
                        
                        element = xElement.Element(StatusXName);
                        if (element != null)
                        {
                            jiraTask.Status = element.Value;
                        }
                        
                        element = xElement.Element(EstimateXName);
                        if (element != null)
                        {
                            var xAttribute = element.Attribute(SecondsXName);
                            if (xAttribute != null)
                            {
                                int seconds;
                                if (int.TryParse(xAttribute.Value, out seconds))
                                {
                                    jiraTask.Estimate = new TimeSpan(0, 0, seconds);
                                }
                            }
                        }
                        
                        element = xElement.Element(TimespentXName);
                        if (element != null)
                        {
                            var xAttribute = element.Attribute(SecondsXName);
                            if (xAttribute != null)
                            {
                                int seconds;
                                if (int.TryParse(xAttribute.Value, out seconds))
                                {
                                    jiraTask.Timespent = new TimeSpan(0, 0, seconds);
                                }
                            }
                        }
                        jiraTasks.Add(jiraTask);

                        item = item.NextNode;
                    }
                }
            }


            return jiraTasks;
        }
    }
}