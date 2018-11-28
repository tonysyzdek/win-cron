using System;
using WinCron.Util;

namespace WinCron.Logging
{
    public class LoggingInfo : ILoggingInfo
    {
        public string FilePathFormat { get; set; }
        public string FileName { get; set; }
        public string BaseDirectory { get; set; }

        public LoggingInfo()
        {
            FilePathFormat = @"{0}\{1}";
            FileName = "log4net.config.xml";
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        public string FilePath
        {
            get { return FilePathFormat.Fmt(BaseDirectory, FileName); }
        }

        public static LoggingInfo Default()
        {
            return new LoggingInfo();
        }
    }
}