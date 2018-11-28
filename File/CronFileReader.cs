using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WinCron.File.Date;
using WinCron.Util;

namespace WinCron.File
{
    public class CronFileReader
    {
        private readonly string _fileName;
        private readonly ICronDateSourceFactory _factory;

        public CronFileReader(string fileName, ICronDateSourceFactory factory)
        {
            _fileName = fileName;
            _factory = factory;
        }

        public CronReaderResult Read()
        {
            var entries = new List<CronEntry>();
            var envVars = new List<Tuple<string, string>>();
            var vars = new Dictionary<string, string>();

            using (var sr = new StreamReader(_fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    line = ReplaceVars(line, vars);

                    const string envStart = "set ";
                    if (line.StartsWith(envStart))
                    {
                        var theRest = line.Substring(envStart.Length);
                        var parts = theRest.SplitOn('=');
                        if (parts.Length != 2)
                        {
                            return CronReaderResult.Failed("{0} is not in expected form for env var");
                        }

                        envVars.Add(Tuple.Create(parts[0], parts[1]));
                        continue;
                    }

                    const string varStart = "var ";
                    if (line.StartsWith(varStart))
                    {
                        var theRest = line.Substring(varStart.Length);
                        var parts = theRest.SplitOn('=');
                        if (parts.Length != 2)
                        {
                            return CronReaderResult.Failed("{0} is not in expected form for variable");
                        }

                        vars[parts[0]] = parts[1];
                        continue;
                    }

                    TimeFieldMatch mDays;
                    IDateFieldMatch wDays;
                    line = line.Trim();

                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    // re-escape space- and backslash-escapes in a cheap fashion
                    line = line.Replace("\\\\", "<BACKSLASH>");
                    line = line.Replace("\\ ", "<SPACE>");

                    var newCols = new List<string>();
                    var current = new StringBuilder();
                    var lastChar = ' ';
                    var withinQuotes = false;

                    foreach (var c in line)
                    {
                        if (!withinQuotes && c == '"')
                        {
                            withinQuotes = true;
                            continue;
                        }
                        
                        if (withinQuotes)
                        {
                            if (c != '"')
                            {
                                current.Append(c);
                            }
                            else
                            {
                                withinQuotes = false;
                                lastChar = ' ';
                                newCols.Add(current.ToString());
                                current = new StringBuilder();
                            }

                            continue;
                        }

                        if (char.IsWhiteSpace(c) && !char.IsWhiteSpace(lastChar))
                        {
                            newCols.Add(current.ToString());
                            current = new StringBuilder();
                            continue;
                        }
                        
                        current.Append(c);
                        lastChar = c;
                    }

                    if (current.Length > 0)
                    {
                        newCols.Add(current.ToString());
                    }

                    var cols = newCols.Select(x => x.Trim()).ToArray();

                    for (var i = 0; i < cols.Length; i++)
                    {
                        cols[i] = cols[i].Replace("<BACKSLASH>", "\\");
                        cols[i] = cols[i].Replace("<SPACE>", " ");
                    }

                    if (cols.Length < 6)
                    {
                        return CronReaderResult.Failed("Parse error in crontab (line too short).");
                    }

                    var minutes = parseTimes(cols[0], 0, 59);
                    var hours = parseTimes(cols[1], 0, 23);
                    var months = parseTimes(cols[3], 1, 12);

                    if (!cols[2].Equals("*") && cols[3].Equals("*"))
                    {
                        // every n monthdays, disregarding weekdays
                        mDays = parseTimes(cols[2], 1, 31);
                        wDays = DateFieldMatch.Any;
                    }
                    else if (cols[2].Equals("*") && !cols[3].Equals("*"))
                    {
                        // every n weekdays, disregarding monthdays
                        mDays = TimeFieldMatch.Any;
                        wDays = ParseDates(cols[4], 1, 7); // 60 * 24 * 7
                    }
                    else
                    {
                        // every n weekdays, every m monthdays
                        mDays = parseTimes(cols[2], 1, 31);
                        wDays = ParseDates(cols[4], 1, 7); // 60 * 24 * 7
                    }

                    var args = new StringBuilder();

                    for (var i = 6; i < cols.Length; i++)
                    {
                        args.Append(" ").Append(cols[i]);
                    }

                    var fileName = cols[5];

                    var entry = new CronEntry(months, mDays, wDays, hours, minutes, fileName, args.Length > 0 ? args.ToString(1, args.Length - 1) : string.Empty);
                    entries.Add(entry);
                }
            }

            return CronReaderResult.Succeeded(entries, envVars);
        }

        private string ReplaceVars(string original, Dictionary<string, string> vars)
        {
            foreach (var pair in vars)
            {
                var varName = "${0}".Fmt(pair.Key);
                original = original.Replace(varName, pair.Value);
            }

            return original;
        }

        private TimeFieldMatch parseTimes(String line, int startNr, int maxNr)
        {
            var vals = new List<int>();
            var list = line.Split(',');

            foreach (var entry in list)
            {
                int start, end, interval;
                var parts = entry.Split('-', '/');

                if (parts[0].Equals("*"))
                {
                    if (parts.Length > 1)
                    {
                        start = startNr;
                        end = maxNr;

                        interval = int.Parse(parts[1]);
                    }
                    else
                    {
                        return TimeFieldMatch.Any;
                    }
                }
                else
                {
                    // format is 0-8/2
                    start = int.Parse(parts[0]);
                    end = parts.Length > 1 ? int.Parse(parts[1]) : int.Parse(parts[0]);
                    interval = parts.Length > 2 ? int.Parse(parts[2]) : 1;
                }
            
                for (var i = start; i <= end; i += interval)
                {
                    vals.Add(i);
                }
            }
            return new TimeFieldMatch(vals);
        }

        private IDateFieldMatch ParseDates(string line, int startNr, int maxNr)
        {
            var vals = new List<int>();
            var list = line.Split(',');

            foreach (var entry in list)
            {
                int start, end, interval;
                var parts = entry.Split('-', '/');

                if (parts[0].Equals("*"))
                {
                    if (parts.Length > 1)
                    {
                        start = startNr;
                        end = maxNr;

                        interval = int.Parse(parts[1]);
                    }
                    else
                    {
                        return DateFieldMatch.Any;
                    }
                }
                else
                {
                    ICronDateSource source;
                    if (_factory.TryGetValue(parts[0], out source))
                    {
                        return new DateSourceMatch(source);
                    }

                    // format is 0-8/2
                    start = int.Parse(parts[0]);
                    end = parts.Length > 1 ? int.Parse(parts[1]) : int.Parse(parts[0]);
                    interval = parts.Length > 2 ? int.Parse(parts[2]) : 1;
                }

                for (var i = start; i <= end; i += interval)
                {
                    vals.Add(i);
                }
            }

            return new DateFieldMatch(vals);
        }
    }
}