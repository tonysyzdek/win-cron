using System;

namespace WinCron.File.Date
{
    public interface ICronDateSource
    {
        bool Contains(DateTime date);
    }
}