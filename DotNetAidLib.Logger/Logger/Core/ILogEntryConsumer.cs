namespace DotNetAidLib.Logger.Core
{
    public interface ILogEntryConsumer
    {
        AsyncExceptionHandler AsyncExceptionHandler { get; set; }
        void ProcessLogEntry(LogEntry logEntry);
    }
}