using WinCron.File.Date;
using WinCron.Logging;
using WinCron.Service;
using WinCron.Task;

namespace WinCron
{
    public class CronImplFactory : IServiceAndConsoleImplFactory
    {
        public IServiceAsyncImpl Create(IErrorReporter reporter)
        {
            return new CronImpl(reporter, new SubProcessFactory(), new EmptyDateSourceFactory());
        }
    }
}