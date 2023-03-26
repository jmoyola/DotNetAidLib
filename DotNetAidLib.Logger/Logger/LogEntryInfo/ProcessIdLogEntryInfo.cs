using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class ProcessIdLogEntryInfo : ILogEntryInfo
    {
        public string Name => "Process ID";
        public string ShortName => "PID";

        public string GetInfo(LogEntry logEntry)
        {
            if (string.IsNullOrEmpty(logEntry.ProcessId))
                return null;
            return "P" + logEntry.ProcessId;
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}