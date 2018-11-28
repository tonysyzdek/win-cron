using System.IO;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using WinCron.Util;

namespace WinCron.Service
{
    public partial class ServiceStarter : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IServiceAsyncImpl _impl;

        public ServiceStarter(IServiceAsyncImpl impl)
        {
            _impl = impl;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("OnStart");
            var settingFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "settings.ini");
            _impl.AsyncStart(Configuration.Load(settingFile), Logging.Logging.Initialize());
            Log.Info("OnStart Compleleted");
        }

        protected override void OnStop()
        {
            Log.Info("OnStop");
            _impl.Dispose();
            Log.Info("OnStop Completed");
        }
    }
}
