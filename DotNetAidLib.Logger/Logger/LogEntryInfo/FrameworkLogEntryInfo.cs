using System.Runtime.InteropServices;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class FrameworkLogEntryInfo : ILogEntryInfo
    {
        public string Name => "Framework Description";
        public string ShortName => "FRDESC";

        public string GetInfo(LogEntry logEntry)
        {
            return RuntimeInformation.FrameworkDescription;
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}