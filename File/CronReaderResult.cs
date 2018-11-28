using System;
using System.Collections.Generic;

namespace WinCron.File
{
    public class CronReaderResult
    {
        public bool Success { get; }
        public string FailReason { get; }
        public List<CronEntry> Entries { get; }
        public List<Tuple<string,string>> EnvironmentVariables { get; } 

        public static CronReaderResult Succeeded(List<CronEntry> entries, List<Tuple<string, string>> envs)
        {
            return new CronReaderResult(true, string.Empty, entries, envs);
        }

        public static CronReaderResult Failed(string reason)
        {
            return new CronReaderResult(false, reason, new List<CronEntry>(), new List<Tuple<string, string>>());
        }

        private CronReaderResult(bool success, string failReason, List<CronEntry> entries, List<Tuple<string, string>> environmentVariables)
        {
            EnvironmentVariables = environmentVariables;
            Success = success;
            FailReason = failReason;
            Entries = entries;
        }
    }
}