using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Logger.Core{
	public interface ILogEntryInfo
	{
		String Name { get; }
		String ShortName { get; }
		String GetInfo(LogEntry logEntry);
        void InitConfiguration(IApplicationConfigGroup cfgGroup);
        void SaveConfiguration(IApplicationConfigGroup configGroup);
    }
}