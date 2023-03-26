using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.Core
{
    public interface ILoggerWriter
    {
        LogPriorityLevels MaxLogPriorityVerboseInDebugTime { get; set; }
        LogPriorityLevels MaxLogPriorityVerboseInRunTime { get; set; }
        AsyncExceptionHandler AsyncExceptionHandler { get; set; }
        bool LastLoggerIfNotError { get; set; }
        void WriteLog(LogEntry logEntry);
        void InitConfiguration(IApplicationConfigGroup configGroup);
        void SaveConfiguration(IApplicationConfigGroup configGroup);
    }
}