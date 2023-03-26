using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class AssemblyLogEntryInfo : ILogEntryInfo
    {
        public string Name => "Assemblies";
        public string ShortName => "ASSEM";

        public string GetInfo(LogEntry logEntry)
        {
            return logEntry.AppAssembly +
                   (!logEntry.AppAssembly.Equals(logEntry.CallAssembly) ? "<" + logEntry.CallAssembly : "");
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}