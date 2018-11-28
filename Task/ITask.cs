using System;

namespace WinCron.Task
{
    public interface ITask : IDisposable
    {
        bool Start();
        ExitedArgs ExitCondition { get; }
        void OnExit(Action listener);
        void OnStdError(Action<string> listener);
        void OnStdOut(Action<string> listener);
    }
}