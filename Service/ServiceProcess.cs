using WinCron.Logging;
using WinCron.Task;
using WinCron.Util;

namespace WinCron.Service
{
    public class ServiceProcess : IServerProcess
    {
        private readonly IServiceAsyncImpl _cronImpl;
        private readonly string _name;

        public ServiceProcess(IServiceAsyncImpl cronImpl, string name)
        {
            _cronImpl = cronImpl;
            _name = name;
        }

        public void Open(IConfiguration configuration, ILoggingInfo loggingInfo)
        {
            _cronImpl.AsyncStart(configuration, loggingInfo);
        }

        public void Start()
        {
        }

        public void Close(IWaitEvent waitEvent)
        {
            _cronImpl.Dispose();
            waitEvent.Done();
        }

        public string Name { get { return _name; }}
    }
}