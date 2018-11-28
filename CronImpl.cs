using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Retlang.Core;
using Retlang.Fibers;
using WinCron.File;
using WinCron.File.Date;
using WinCron.Logging;
using WinCron.Service;
using WinCron.Task;
using WinCron.Util;

namespace WinCron
{
    public class CronImpl : Disposable, IServiceAsyncImpl
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<ITask> _processes = new List<ITask>();
        private readonly IErrorReporter _errorReporter;
        private readonly ITaskFactory _taskFactory;        
        private readonly List<CronEntry> _crontab = new List<CronEntry>();
        private readonly List<Tuple<string, string>> _envVars = new List<Tuple<string, string>>();
        private readonly ICronDateSourceFactory _dateSourceFactory;

        private int _lastMinute;
        private ThreadFiber _fiber;

        public CronImpl(IErrorReporter errorReporter, ITaskFactory taskFactory, ICronDateSourceFactory dateSourceFactory)
        {
            _errorReporter = errorReporter;
            _taskFactory = taskFactory;
            _dateSourceFactory = dateSourceFactory;
        }

        public void AsyncStart(IConfiguration config, ILoggingInfo loggingInfo)
        {
            _lastMinute = DateTime.Now.Minute - 1;

            try
            {
                var loggingExecutor = new LoggingExecutor((action, exception) => _errorReporter.Write("Running {0}, {1}".Fmt(action, exception.ToString())));
                var queue = new DefaultQueue(loggingExecutor);

                _fiber = _disposables.Add(new ThreadFiber(queue, "WinCron Monitor"));
                _fiber.Start();

                var tabFile = config.ResolveString("tabfile");
                var intervalMs = config.ResolveInt("intervalMs");
                var watchdogIntervalMs = config.ResolveInt("watchdogIntervalMs");
                var fileChangeReadDelayMs = config.ResolveInt("fileChangeReadDelayMs");

                if (string.IsNullOrWhiteSpace(tabFile))
                {
                    tabFile = Path.Combine(loggingInfo.BaseDirectory, "cron.tab");
                }

                var tabFileName = Path.GetFileName(tabFile);
                var tabFileDirectory = Path.GetDirectoryName(tabFile);
                  
                var watcher = new FileSystemWatcher();
                watcher.Path = tabFileDirectory;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = tabFileName;
                watcher.Changed += (sender, args) => _fiber.Schedule(() =>
                {
                    try
                    {
                        ExtractEntries(tabFile);
                        Log.Info("{0} watcher extracted due to file system changes".Fmt(tabFile));
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Not setting new crontab entries", e);
                    }
                }, fileChangeReadDelayMs);

                watcher.Error +=
                    (sender, args) => _fiber.Enqueue(
                        () => _errorReporter.Write("Error from filewatcher: {0}".Fmt(args.GetException().ToString())));
                _disposables.Add(() => watcher.EnableRaisingEvents = false);
                watcher.EnableRaisingEvents = true;

                ExtractEntries(tabFile);

                _disposables.Add(_fiber.ScheduleOnInterval(() => DoCrontab(DateTime.Now), 0, intervalMs));
                _disposables.Add(_fiber.ScheduleOnInterval(CheckProcesses, 0, watchdogIntervalMs));
                Log.Info("Initialized WinCron");
            }
            catch (Exception e)
            {
                _errorReporter.Write(e.ToString());
                throw;
            }
        }

        private void ExtractEntries(string tabFile)
        {
            var cronReadResult = new CronFileReader(tabFile, _dateSourceFactory).Read();

            if (!cronReadResult.Success)
            {
                _errorReporter.Write(cronReadResult.FailReason);
                throw new Exception(cronReadResult.FailReason);
            }

            _crontab.Clear();
            _crontab.AddRange(cronReadResult.Entries);
            _envVars.Clear();
            _envVars.AddRange(cronReadResult.EnvironmentVariables);
        }

        private void CheckProcesses()
        {
            foreach (var proc in _processes)
            {
                if (proc.ExitCondition.HasExited)
                {
                    TryRemove(proc);
                    continue;
                }

                Log.Info("The process " + proc + " is still running");
            }
        }

        private void DoCrontab(DateTime now)
        {
            Log.Info("Checking Cron at {0}".Fmt(now));

            if (now.Minute.Equals(_lastMinute))
            {
                return;
            }

            // for loop: deal with the highly unexpected eventuality of
            // having lost more than one minute to unavailable processor time
            for (var minute = (_lastMinute == 59 ? 0 : _lastMinute + 1); minute <= now.Minute; minute++)
            {
                foreach (var entry in _crontab)
                {
                    Log.Debug("Checking entry {0}".Fmt(entry));

                    if (entry.MatchesTimeSpec(now))
                    {
                        Log.Debug("Running at {0}: {1}".Fmt(now, entry));

                        var cronTask = _taskFactory.Create(entry.FileName, entry.Args, _envVars, null);
                        _processes.Add(cronTask);

                        var entry1 = entry;
                        cronTask.OnStdError(
                            x => _fiber.Enqueue(() => _errorReporter.Write("Error from {0}: {1}".Fmt(entry1, x))));
                        cronTask.OnStdOut(x => _fiber.Enqueue(() => Log.Info("StdOut from {0}: {1}".Fmt(entry1, x))));
                        cronTask.OnExit(() => _fiber.Enqueue(() =>
                        {
                            var exitArgs = cronTask.ExitCondition;
                            if (exitArgs.ExitCode != 0)
                            {
                                _errorReporter.Write("The process " + entry1 + " returned with error " +
                                                     exitArgs.ExitCode);
                            }
                            else
                            {
                                Log.Info("Cron task exited successfully: {0}".Fmt(entry1));
                            }

                            TryRemove(cronTask);
                        }));

                        if (!cronTask.Start())
                        {
                            _errorReporter.Write("Could not start " + entry1.FileName);
                            TryRemove(cronTask);
                        }

                        Log.Info("Cron task started: {0}".Fmt(entry1));
                    }
                }
            }

            _lastMinute = now.Minute;
        }

        private void TryRemove(ITask p)
        {
            if (_processes.Contains(p))
            {
                _processes.Remove(p);
            }
        }
    }
}