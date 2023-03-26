using System;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public class SyslogLoggerWriter : LoggerWriter
    {
        private static SyslogLoggerWriter _Instance;
        private readonly FileInfo loggerPath;

        private SyslogLoggerWriter(FileInfo loggerPath)
        {
            Assert.Exists(loggerPath, nameof(loggerPath));
            this.loggerPath = loggerPath;
        }

        public override void WriteLog(LogEntry logEntry)
        {
            try
            {
                loggerPath.CmdExecuteSync(LogPriorityLevels._INFO.ToString().Substring(1) + " " + logEntry);
            }
            catch (Exception ex)
            {
                throw new LoggerException("Error writing log to syslog", ex, logEntry);
            }
        }

        ~SyslogLoggerWriter()
        {
        }

        public static SyslogLoggerWriter Instance(FileInfo loggerPath)
        {
            if (_Instance == null)
                _Instance = new SyslogLoggerWriter(loggerPath);

            return _Instance;
        }

        public static SyslogLoggerWriter Instance()
        {
            if (_Instance == null)
                _Instance = new SyslogLoggerWriter(EnvironmentHelper.SearchInPath("logger"));

            return _Instance;
        }
    }
}