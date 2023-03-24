using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.LogEntryInfo{
    public class ProcessRAMLogEntryInfo: ILogEntryInfo
	{
        public String Name { get => "Process Memory"; }
        public String ShortName { get => "PMEM"; }
        
        public String GetInfo(LogEntry logEntry) {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            return "M" + (process.WorkingSet64 / 1024 / 1024) + "/" + (process.VirtualMemorySize64 / 1024 / 1024);
        }
        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}