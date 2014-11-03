using System;
using System.Collections.Generic;
using TimegripExtended.Business.Domain;
using TimegripExtended.Business.Domain.Plurals;

namespace TimegripExtended.Converters
{
    public static class TimegripActivityConverter
    {
        private const decimal SecondsInAnHour = 3600;

        public static TimegripActivities ConvertToActivities(List<string> fileLines)
        {
            var timegripActivities = new TimegripActivities();

            foreach (var fileLine in fileLines)
            {
                timegripActivities.Add(ConvertLine(fileLine));
            }

            return timegripActivities;
        }

        private static TimegripActivity ConvertLine(string line)
        {
            var timegripActivity = new TimegripActivity();

            var lineSplit = line.Split(';');
            if (lineSplit.Length > 0)
            {
                timegripActivity.Customer = lineSplit[0].Trim();
            }

            if (lineSplit.Length > 1)
            {
                timegripActivity.Project = lineSplit[1].Trim();
            }
            if (lineSplit.Length > 3)
            {
                timegripActivity.Activity = lineSplit[3].Trim();
            }
            if (lineSplit.Length > 4)
            {
                decimal output;
                if (decimal.TryParse(lineSplit[4], out output))
                {
                    timegripActivity.TimeUsed = new TimeSpan(0, 0, (int)(output * SecondsInAnHour));
                }
            }
            if (lineSplit.Length > 5)
            {

                decimal output;
                if (decimal.TryParse(lineSplit[5].Replace(",", "."), out output))
                {
                    timegripActivity.Amount = output;
                }
            }

            return timegripActivity;
        }
    }
}