using WinCron.Logging;

namespace WinCron.Service
{
    public interface IServiceAndConsoleImplFactory
    {
        IServiceAsyncImpl Create(IErrorReporter reporter);
    }
}