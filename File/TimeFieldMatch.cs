using System.Collections.Generic;
using System.Linq;
using WinCron.Util;

namespace WinCron.File
{
    public class TimeFieldMatch : ITimeFieldMatch
    {
        private readonly bool _matchAny;
        private readonly List<int> _selections;

        public static readonly TimeFieldMatch Any = new TimeFieldMatch(true);

        private TimeFieldMatch(bool matchAny) : this()
        {
            _matchAny = matchAny;
        }

        public TimeFieldMatch(List<int> selections=null)
        {
            _selections = selections ?? new List<int>();
        }

        public override string ToString()
        {
            return _matchAny ? "Any" : _selections.Select(x => x.ToString()).StringJoin();
        }

        public bool Matches(int t)
        {
            if (_matchAny)
            {
                return true;
            }

            return _selections.Contains(t);
        }
    }
}