using System;
using System.Collections.Generic;

namespace WinCron.Task
{
    public interface ITaskFactory
    {
        ITask Create(string filename, string args, List<Tuple<string, string>> envVars, string workingDirectory);
    }
}