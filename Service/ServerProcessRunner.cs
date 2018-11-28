using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using log4net;
using WinCron.Logging;
using WinCron.Task;
using WinCron.Util;

namespace WinCron.Service
{
    public static class ServerProcessRunner
    {
        private const int ExitedOnException = -1;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static int Run(ILoggingInfo loggingInfo,
            IServerProcess process,
            IConfiguration config,
            int processDisposeWait = 60000)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                log.InfoFormat("Process started at {0}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                log.Info("LogDir={0}".Fmt(Logging.Logging.LogDir));

                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress +=
                    (sender, args) => Exit(process, stopwatch, exitEvent, false, processDisposeWait);
                AppDomain.CurrentDomain.UnhandledException +=
                    ((sender, e) => OnUnhandledException(e, process, processDisposeWait));

                log.Info("Start opening processes");
                Console.WriteLine("Starting...");

                var openingMsg = "Opening {0}...".Fmt(process);
                log.Info(openingMsg);
                process.Open(config, loggingInfo);

                log.Info("Opened {0}".Fmt(process));

                log.Info("Done opening processes");
                Console.WriteLine();
                process.Start();
                Console.WriteLine("Started");

                exitEvent.WaitOne();
                Console.WriteLine("Ending");
                return 0;
            }
            catch (Exception e)
            {
                log.Fatal(e);
                LogProcessEnd(stopwatch);
                throw;
            }
        }

        private static void Exit(IServerProcess serverProcess, Stopwatch stopwatch, ManualResetEvent exitEvent,
            bool isConfiguredExit, int millisToWait)
        {
            if (isConfiguredExit) log.Info("Server process exiting due to configured exit");
            CloseAll(serverProcess, millisToWait);
            LogProcessEnd(stopwatch);
            exitEvent.Set();
        }

        private class ManualWaitEvent : IWaitEvent
        {
            private readonly ManualResetEvent _resetEvent;

            public ManualWaitEvent(ManualResetEvent resetEvent)
            {
                _resetEvent = resetEvent;
            }

            public void Done()
            {
                _resetEvent.Set();
            }
        }

        private static void CloseAll(IServerProcess serverProcess, int millisToWait)
        {
            try
            {
                var resetEvent = new ManualResetEvent(false);
                serverProcess.Close(new ManualWaitEvent(resetEvent));

                if (!resetEvent.WaitOne(millisToWait))
                {
                    log.Error("Timed out while waiting on process closes");
                }
            }
            catch (Exception e)
            {
                log.Error("Trying to close", e);
            }
        }

        private static void LogProcessEnd(Stopwatch stopwatch)
        {
            log.InfoFormat("Process ended at {0}.  Total running time {1}",
                DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffff"), stopwatch.Elapsed);
        }

        private static void OnUnhandledException(UnhandledExceptionEventArgs args, IServerProcess serverProcess,
            int millisToWait)
        {
            var ex = args.ExceptionObject as Exception ??
                     new Exception(string.Format("Unknown exception cause by:{0}", args));
            Console.WriteLine("Fatal application exception occurred: {0}", ex);
            log.Fatal("Fatal application exception ocurred:{0}", ex);
            try
            {
                CloseAll(serverProcess, millisToWait);
            }
            finally
            {
                Environment.Exit(ExitedOnException);
            }
        }
    }
}