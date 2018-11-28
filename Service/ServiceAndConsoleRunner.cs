using System.Linq;
using System.ServiceProcess;
using WinCron.Logging;
using WinCron.Util;

namespace WinCron.Service
{
    public static class ServiceAndConsoleRunner
    {
        public static int Run(string[] args, IServiceAndConsoleImplFactory implFactory, string name)
        {
            if (args.Any(x => x.ToLower().Contains("console")))
            {
                var cronImpl = implFactory.Create(new ConsoleErrorReporter());
                ServerProcessRunner.Run(LoggingInfo.Default(), new ServiceProcess(cronImpl, name), Configuration.Load());
            }
            else
            {
                var cronImpl = implFactory.Create(new ServiceErrorReporter());
                var servicesToRun = new ServiceBase[]
                {
                    new ServiceStarter(cronImpl)
                };

                ServiceBase.Run(servicesToRun);
            }

            return 0;
        }
    }
}