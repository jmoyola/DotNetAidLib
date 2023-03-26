using System;

namespace DotNetAidLib.Logger.Core
{
    public class LogEntryArrivatedEventArgs : EventArgs
    {
        public LogEntryArrivatedEventArgs(LogEntry logEntry)
        {
            LogEntry = logEntry;
        }

        public LogEntry LogEntry { get; }
    }
}