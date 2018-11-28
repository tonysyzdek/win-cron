using System;
using System.Collections.Generic;
using System.Diagnostics;
using WinCron.Util;

namespace WinCron.Task
{
    public class ProcWrapper : ITask
    {
        private readonly Process _proc;
        private readonly List<Action> _onExit = new List<Action>();
        private readonly List<Action<string>> _onStdError = new List<Action<string>>();
        private readonly List<Action<string>> _onStdOut = new List<Action<string>>();

        public ProcWrapper(Process proc)
        {
            _proc = proc;
            _proc.EnableRaisingEvents = true;
            _proc.Exited += ProcOnExited;
            _proc.ErrorDataReceived += ProcOnErrorDataReceived;
            _proc.OutputDataReceived += ProcOnOutputDataReceived;
        }

        private void ProcOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            _onStdOut.ForEach(x => x(dataReceivedEventArgs.Data));
        }

        private void ProcOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            _onStdError.ForEach(x => x(dataReceivedEventArgs.Data));
        }

        private void ProcOnExited(object sender, EventArgs eventArgs)
        {
            _onExit.ForEach(x => x());
        }

        public bool Start()
        {
            return _proc.Start();
        }

        public ExitedArgs ExitCondition
        {
            get
            {
                try
                {
                    if (!_proc.HasExited)
                    {
                        return ExitedArgs.StillRunning;
                    }

                    return ExitedArgs.Done(_proc.ExitCode);
                }
                catch (InvalidOperationException e)
                {
                    return ExitedArgs.Done(-e.HResult);
                }
            }
        }

        public void OnExit(Action listener)
        {
            _onExit.Add(listener);
        }

        public void OnStdError(Action<string> listener)
        {
            _onStdError.Add(listener);
        }

        public void OnStdOut(Action<string> listener)
        {
            _onStdOut.Add(listener);
        }

        public void Dispose()
        {
            _proc.Exited -= ProcOnExited;
            _proc.ErrorDataReceived -= ProcOnErrorDataReceived;
        }

        public override string ToString()
        {
            return "{0} {1}".Fmt(_proc.StartInfo.FileName, _proc.StartInfo.Arguments);
        }
    }
}