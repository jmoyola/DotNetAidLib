using System;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public class ConsoleLoggerWriter : LoggerWriter
    {
        private static ConsoleLoggerWriter _Instance;

        private static readonly object oInstance = new object();

        private ConsoleLoggerWriter()
        {
            _MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ALL;
            _MaxLogPriorityVerboseInRunTime = LogPriorityLevels._ALL;
        }

        public override void WriteLog(LogEntry logEntry)
        {
            try
            {
                if (logEntry.LogPriority <= LogPriorityLevels._WARN)
                    Console.Error.WriteLine(logEntry.ToString());
                else
                    Console.Out.WriteLine(logEntry.ToString());
            }
            catch (Exception ex)
            {
                throw new LoggerException("Error writing log to console", ex, logEntry);
            }
        }


        ~ConsoleLoggerWriter()
        {
        }

        public static ConsoleLoggerWriter Instance()
        {
            lock (oInstance)
            {
                if (_Instance == null)
                    _Instance = new ConsoleLoggerWriter();

                return _Instance;
            }
        }
    }
}