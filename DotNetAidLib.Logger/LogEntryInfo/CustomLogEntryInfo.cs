using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Logger.LogEntryInfo{
    public class CustomLogEntryInfo: ILogEntryInfo
	{
        private String name;
        private String shortName;
        private Func<LogEntry,String> function;
        public CustomLogEntryInfo(String name, String shortName, Func<LogEntry, String> function) {
            Assert.NotNullOrEmpty( name, nameof(name));
            Assert.NotNullOrEmpty( shortName, nameof(shortName));
            Assert.NotNull( function, nameof(function));
            
            this.function = function;
            this.name = name;
            this.shortName = shortName;
        }

        public String Name { get => this.name; }
        public String ShortName { get => this.shortName; }
        
        public String GetInfo(LogEntry logEntry) {

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