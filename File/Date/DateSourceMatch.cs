using System;

namespace WinCron.File.Date
{
    public class DateSourceMatch : IDateFieldMatch
    {
        private readonly ICronDateSource _source;

        public DateSourceMatch(ICronDateSource source)
        {
            _source = source;
        }

        public bool Matches(DateTime date)
        {
            return _source.Contains(date);
        }
    }
}