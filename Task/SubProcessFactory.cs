using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WinCron.Task
{
    public class SubProcessFactory : ITaskFactory
    {
        public ITask Create(string filename, string args, List<Tuple<string, string>> envVars, string workingDirectory)
        {
            var proc = new Process();

            var startInfo = new ProcessStartInfo();

            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            startInfo.FileName = filename;
            startInfo.Arguments = args;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;

            foreach (var envVar in envVars)
            {
                startInfo.EnvironmentVariables[envVar.Item1] = envVar.Item2;
            }

            proc.StartInfo = startInfo;

            return new ProcWrapper(proc);
        }
    }
}