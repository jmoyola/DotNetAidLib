using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.LogEntryInfo{
    public class ProcessIdLogEntryInfo: ILogEntryInfo
	{
        public String Name { get => "Process ID"; }
        public String ShortName { get => "PID"; }
        
        public String GetInfo(LogEntry logEntry) {
            if (String.IsNullOrEmpty(logEntry.ProcessId))
                return null;
            else
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