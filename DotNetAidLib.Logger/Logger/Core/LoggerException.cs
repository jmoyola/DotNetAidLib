using System;

namespace DotNetAidLib.Logger.Core
{
    public class LoggerException : Exception
    {
        public LoggerException()
        {
        }

        public LoggerException(string message) : base(message)
        {
        }

        public LoggerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LoggerException(string message, LogEntry logEntry) : base(message)
        {
            LogEntry = logEntry;
        }

        public LoggerException(string message, Exception innerException, LogEntry logEntry) : base(message,
            innerException)
        {
            LogEntry = logEntry;
        }

        public LogEntry LogEntry { get; set; }
    }
}