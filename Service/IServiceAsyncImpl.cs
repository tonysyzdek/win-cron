using System;
using WinCron.Logging;
using WinCron.Util;

namespace WinCron.Service
{
    public interface IServiceAsyncImpl : IDisposable
    {
        void AsyncStart(IConfiguration config, ILoggingInfo loggingInfo);
    }
}