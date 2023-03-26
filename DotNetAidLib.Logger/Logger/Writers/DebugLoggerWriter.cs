using System;
using System.Diagnostics;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public class DebugLoggerWriter : LoggerWriter
    {
        private static DebugLoggerWriter _Instance;

        private static readonly object oInstance = new object();

        private DebugLoggerWriter()
        {
            _MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ALL;
            _MaxLogPriorityVerboseInRunTime = LogPriorityLevels._OFF;
        }

        public override void WriteLog(LogEntry logEntry)
        {
            try
            {
                Debug.WriteLine(logEntry.ToString());
            }
            catch (Exception ex)
            {
                throw new LoggerException("Error writing log to debug", ex, logEntry);
            }
        }


        ~DebugLoggerWriter()
        {
        }

        public static DebugLoggerWriter Instance()
        {
            lock (oInstance)
            {
                if (_Instance == null)
                    _Instance = new DebugLoggerWriter();

                return _Instance;
            }
        }
    }
}