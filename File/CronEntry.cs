using System;
using WinCron.Util;

namespace WinCron.File
{
    public class CronEntry
    {
        public CronEntry(ITimeFieldMatch months, 
                         ITimeFieldMatch mDays, 
                         IDateFieldMatch wDays, 
                         ITimeFieldMatch hours, 
                         ITimeFieldMatch minutes, 
                         string fileName, 
                         string args)
        {
            Months = months;
            MDays = mDays;
            WDays = wDays;
            Hours = hours;
            Minutes = minutes;
            FileName = fileName;
            Args = args;
        }

        public ITimeFieldMatch Months { get; }
        public ITimeFieldMatch MDays { get; }
        public IDateFieldMatch WDays { get; }
        public ITimeFieldMatch Hours { get; }
        public ITimeFieldMatch Minutes { get; }

        public string FileName { get; }
        public string Args { get; }

        public bool MatchesTimeSpec(DateTime now)
        {
            return Months.Matches(now.Month) &&
                   MDays.Matches(GetMDay(now)) &&
                   WDays.Matches(now) &&
                   Hours.Matches(now.Hour) &&
                   Minutes.Matches(now.Minute);
        }

        private static int GetMDay(DateTime date)
        {
            date = date.AddMonths(-(date.Month - 1));
            return date.DayOfYear;
        }

        public override string ToString()
        {
            return "{0} {1}".Fmt(FileName, Args);
        }
    }
}