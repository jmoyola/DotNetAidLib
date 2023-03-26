using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.Core
{
    public interface ILogEntryInfo
    {
        string Name { get; }
        string ShortName { get; }
        string GetInfo(LogEntry logEntry);
        void InitConfiguration(IApplicationConfigGroup cfgGroup);
        void SaveConfiguration(IApplicationConfigGroup configGroup);
    }
}