using System;
using System.Collections.Generic;
using DotNetAidLib.Logger.Client;

namespace DotNetAidLib.Logger.Core
{
    public delegate void AsyncExceptionHandler(object sender, Exception ex);

    public enum LogPriorityLevels
    {
        _OFF = 0,
        _FATAL = 1,
        _ERROR = 2,
        _WARN = 3,
        _INFO = 4,
        _DEBUG = 5,
        _TRACE = 6,
        _ALL = 7
    }

    public enum RunLevels
    {
        _DEBUGTIME = 0,
        _RUNTIME = 1
    }

    public enum ExceptionVerbosityLevels
    {
        _LOW = 1,
        _MEDIUM = 2,
        _HIGH = 3
    }

    public interface ILogger
    {
        string LoggerId { get; }
        LoggerWriters LoggerWriters { get; }
        IList<ILogEntryInfo> LogEntryInformation { get; }
        ExceptionVerbosityLevels ExceptionsVerbosityLevel { get; set; }
        void WriteFatal(string message);
        void WriteFatal(string message, string processId);
        void WriteError(string message);
        void WriteError(string message, string processId);
        void WriteWarning(string message);
        void WriteWarning(string message, string processId);
        void WriteInfo(string message);
        void WriteInfo(string message, string processId);
        void WriteDebug(string message);
        void WriteDebug(string message, string processId);
        void WriteTrace(string message);
        void WriteTrace(string message, string processId);
        void Write(string message, LogPriorityLevels logPriority);
        void Write(string message, LogPriorityLevels logPriority, string processId);
        void WriteException(string message, Exception ex, LogPriorityLevels logPriority, string processId);
        void WriteException(Exception ex, LogPriorityLevels logPriority, string processId);
        void WriteException(string message, Exception exception, string processId);
        void WriteException(Exception ex, string processId);
        void WriteException(string message, Exception ex, LogPriorityLevels logPriority);
        void WriteException(Exception ex, LogPriorityLevels logPriority);
        void WriteException(string message, Exception exception);
        void WriteException(Exception ex);
        void WriteLogEntry(LogEntry logEntry);
        void UpdateConfiguration();
        void Flush();
    }
}