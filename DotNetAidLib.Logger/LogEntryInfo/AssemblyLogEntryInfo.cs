using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.LogEntryInfo{
    public class AssemblyLogEntryInfo : ILogEntryInfo
    {
        public String Name { get => "Assemblies"; }
        public String ShortName { get => "ASSEM"; }
        
        public String GetInfo(LogEntry logEntry)
        {
            return logEntry.AppAssembly.ToString() + (!logEntry.AppAssembly.Equals(logEntry.CallAssembly) ? "<" + logEntry.CallAssembly.ToString() : "");
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }
        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}