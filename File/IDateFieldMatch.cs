using System;

namespace WinCron.File
{
    public interface IDateFieldMatch
    {
        bool Matches(DateTime date);
    }
}