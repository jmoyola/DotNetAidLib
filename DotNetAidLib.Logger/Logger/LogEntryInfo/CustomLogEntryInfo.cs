using System;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class CustomLogEntryInfo : ILogEntryInfo
    {
        private readonly Func<LogEntry, string> function;

        public CustomLogEntryInfo(string name, string shortName, Func<LogEntry, string> function)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNullOrEmpty(shortName, nameof(shortName));
            Assert.NotNull(function, nameof(function));

            this.function = function;
            Name = name;
            ShortName = shortName;
        }

        public string Name { get; }

        public string ShortName { get; }

        public string GetInfo(LogEntry logEntry)
        {
            return function.Invoke(logEntry);
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}