using WinCron.Logging;
using WinCron.Task;
using WinCron.Util;

namespace WinCron.Service
{
    public interface IServerProcess
    {
        void Open(IConfiguration configuration, ILoggingInfo loggingInfo);
        void Start();
        void Close(IWaitEvent waitEvent);
    }
}