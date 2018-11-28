using System;
using System.Collections.Generic;

namespace WinCron.File
{
    public class DateFieldMatch : IDateFieldMatch
    {
        private class AnyFieldMatch : IDateFieldMatch
        {
            public bool Matches(DateTime date)
            {
                return true;
            }
        }

        public static readonly IDateFieldMatch Any = new AnyFieldMatch();
        
        private readonly List<int> _vals;

        public DateFieldMatch(List<int> vals)
        {
            _vals = vals;
        }

        public bool Matches(DateTime date)
        {
            var dayOfWeek = getWDay(date);
            return _vals.Contains(dayOfWeek);
        }

        private int getWDay(DateTime date)
        {
            if (date.DayOfWeek.Equals(DayOfWeek.Sunday))
                return 7;
            return (int)date.DayOfWeek;
        }
    }
}