
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Logger.Core{
	public interface ILoggerWriter
	{
		LogPriorityLevels MaxLogPriorityVerboseInDebugTime { get; set; }
		LogPriorityLevels MaxLogPriorityVerboseInRunTime { get; set; }
		AsyncExceptionHandler AsyncExceptionHandler { get; set; }
		bool LastLoggerIfNotError { get; set; }
		void WriteLog(LogEntry logEntry);
		void InitConfiguration(IApplicationConfigGroup configGroup);
		void SaveConfiguration(IApplicationConfigGroup configGroup);
	}
}