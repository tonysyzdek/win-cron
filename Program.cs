using WinCron.Service;

namespace WinCron
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Logging.Logging.Initialize();
            return ServiceAndConsoleRunner.Run(args, new CronImplFactory(), "WinCron");
        }
    }
}
