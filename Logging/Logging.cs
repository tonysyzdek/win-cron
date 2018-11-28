using System.IO;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;

namespace WinCron.Logging
{
    public static class Logging
    {
        public static void Initialize(ILoggingInfo loggingInfo)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(loggingInfo.FilePath));
        }

        public static LoggingInfo Initialize()
        {
            var loggingInfo = LoggingInfo.Default();
            Initialize(loggingInfo);
            return loggingInfo;
        }

        public static string LogDir
        {
            get
            {
                var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                foreach (var appender in log.Logger.Repository.GetAppenders())
                {
                    var aType = appender.GetType();
                    if (aType == typeof(RollingFileAppender) || aType == typeof(FileAppender))
                    {
                        return Path.GetDirectoryName(((FileAppender)appender).File);
                    }
                }

                return string.Empty;
            }
        }
    }
}