namespace WinCron.Task
{
    public class ExitedArgs
    {
        public bool HasExited { get; }
        public int ExitCode { get; }

        public static readonly ExitedArgs StillRunning = new ExitedArgs(false, int.MinValue);

        public static ExitedArgs Done(int exitCode)
        {
            return new ExitedArgs(true, exitCode);
        }

        private ExitedArgs(bool hasExited, int exitCode)
        {
            HasExited = hasExited;
            ExitCode = exitCode;
        }
    }
}