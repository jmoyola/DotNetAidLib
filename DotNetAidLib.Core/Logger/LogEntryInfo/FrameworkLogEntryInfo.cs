using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Logger.Core;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Logger.LogEntryInfo{
    public class FrameworkLogEntryInfo : ILogEntryInfo
    {
        public String Name { get => "Framework Description"; }
        public String ShortName { get => "FRDESC"; }
        
        public String GetInfo(LogEntry logEntry)
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