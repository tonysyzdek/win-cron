using System;
using System.Collections.Generic;
using NUnit.Framework;
using WinCron.File;
using WinCron.File.Date;

namespace Cron.Test
{
    [TestFixture]
    public class TestCronFileReader
    {
        [Test]
        public void ShouldReadCronTabFile()
        {
            //Inspired examples from https://www.thegeekstuff.com/2009/06/15-practical-crontab-examples

            var result = new CronFileReader("cron.tab", new HolidayDateSourceFactory()).Read();
            Assert.IsTrue(result.Success);

            Assert.AreEqual(1, result.EnvironmentVariables.Count);
            Assert.AreEqual("PYTHONPATH", result.EnvironmentVariables[0].Item1);
            Assert.AreEqual(@"c:/daily_process", result.EnvironmentVariables[0].Item2);

            Assert.AreEqual(3, result.Entries.Count);

            var e3 = result.Entries[2];
            Assert.AreEqual(@"c:\Program Files\Python\Python.exe", e3.FileName);
            Assert.IsFalse(e3.WDays.Matches(new DateTime(2018, 7, 4)));
            Assert.IsTrue(e3.WDays.Matches(new DateTime(2018, 7, 9)));
            Assert.IsTrue(e3.WDays.Matches(new DateTime(2018, 7, 13)));
            Assert.IsFalse(e3.WDays.Matches(new DateTime(2018, 7, 14)));
            Assert.AreEqual("these are args", e3.Args);
            //30 08 10 06 * /home/ramesh/full-backup

            var e1 = result.Entries[0];
            Assert.IsTrue(e1.Minutes.Matches(30));
            Assert.IsTrue(e1.Hours.Matches(8));
            Assert.IsFalse(e1.Hours.Matches(9));
            Assert.IsTrue(e1.MDays.Matches(10));
            Assert.IsTrue(e1.Months.Matches(6));
            Assert.IsTrue(e1.WDays.Matches(new DateTime(2018, 7, 13)));
            Assert.IsTrue(e1.WDays.Matches(new DateTime(2018, 7, 14)));
            Assert.AreEqual("/home/ramesh/bin/full-backup", e1.FileName);
            Assert.AreEqual(string.Empty, e1.Args);

            //            The basic usage of cron is to execute a job in a specific time as shown below. This will execute the Full backup shell script (full-backup) on 10th June 08:30 AM.
            //
            //Please note that the time field uses 24 hours format. So, for 8 AM use 8, and for 8 PM use 20.
            //
            //30 08 10 06 * /home/ramesh/full-backup
            //30 – 30th Minute
            //08 – 08 AM
            //10 – 10th Day
            //06 – 6th Month (June)
            //* – Every day of the week
            //2. Schedule a Job For More Than One Instance (e.g. Twice a Day)
            //
            //The following script take a incremental backup twice a day every day.
            //
            //This example executes the specified incremental backup shell script (incremental-backup) at 11:00 and 16:00 on every day. The comma separated value in a field specifies that the command needs to be executed in all the mentioned time.
            //
            //00 11,16 * * * /home/ramesh/bin/incremental-backup
            //00 – 0th Minute (Top of the hour)
            //11,16 – 11 AM and 4 PM
            //* – Every day
            //* – Every month
            //* – Every day of the week
            //3. Schedule a Job for Specific Range of Time (e.g. Only on Weekdays)
            //
            //If you wanted a job to be scheduled for every hour with in a specific range of time then use the following.
            //
            //CronImpl Job everyday during working hours
            //This example checks the status of the database everyday (including weekends) during the working hours 9 a.m – 6 p.m
            //
            //00 09-18 * * * /home/ramesh/bin/check-db-status
            //00 – 0th Minute (Top of the hour)
            //09-18 – 9 am, 10 am,11 am, 12 am, 1 pm, 2 pm, 3 pm, 4 pm, 5 pm, 6 pm
            //* – Every day
            //* – Every month
            //* – Every day of the week
            //CronImpl Job every weekday during working hours
            //This example checks the status of the database every weekday (i.e excluding Sat and Sun) during the working hours 9 a.m – 6 p.m.
            //
            //00 09-18 * * 1-5 /home/ramesh/bin/check-db-status
            //00 – 0th Minute (Top of the hour)
            //09-18 – 9 am, 10 am,11 am, 12 am, 1 pm, 2 pm, 3 pm, 4 pm, 5 pm, 6 pm
            //* – Every day
            //* – Every month
            //1-5 -Mon, Tue, Wed, Thu and Fri (Every Weekday)
        }
    }

    public class HolidayDateSourceFactory : ICronDateSourceFactory, ICronDateSource
    {
        private readonly HashSet<DateTime> _holidays = new HashSet<DateTime> {new DateTime(2018, 7, 4)};

        public bool TryGetValue(string token, out ICronDateSource source)
        {
            if (token == "B")
            {
                source = this;
                return true;
            }

            source = null;
            return false;
        }

        public bool Contains(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                return false;
            }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            return !_holidays.Contains(date);
        }
    }
}