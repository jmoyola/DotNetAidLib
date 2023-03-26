namespace DotNetAidLib.Logger.Core
{
    public abstract class LogEntryConsumer : ILogEntryConsumer
    {
        protected AsyncExceptionHandler _AsyncExceptionHandler;

        public AsyncExceptionHandler AsyncExceptionHandler
        {
            get => _AsyncExceptionHandler;
            set => _AsyncExceptionHandler = value;
        }

        public abstract void ProcessLogEntry(LogEntry logEntry);
    }
}