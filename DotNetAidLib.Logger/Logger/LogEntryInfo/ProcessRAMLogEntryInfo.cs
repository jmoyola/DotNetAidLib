using System.Diagnostics;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class ProcessRAMLogEntryInfo : ILogEntryInfo
    {
        public string Name => "Process Memory";
        public string ShortName => "PMEM";

        public string GetInfo(LogEntry logEntry)
        {
            var process = Process.GetCurrentProcess();
            return "M" + process.WorkingSet64 / 1024 / 1024 + "/" + process.VirtualMemorySize64 / 1024 / 1024;
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}