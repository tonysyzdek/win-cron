using System.Reflection;
using log4net;
using WinCron.Logging;

namespace WinCron.Service
{
    public class ServiceErrorReporter : IErrorReporter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Write(string text)
        {
            Log.Error(text);
        }
    }
}