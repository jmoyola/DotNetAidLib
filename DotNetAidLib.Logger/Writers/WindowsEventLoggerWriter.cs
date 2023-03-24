using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Collections.Specialized;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.Writers{
	public class WindowsEventLoggerWriter : LoggerWriter
	{
		private static WindowsEventLoggerWriter _Instance=null;

		private WindowsEventLoggerWriter(){}

		public override void WriteLog(LogEntry logEntry)
		{
			try {

				String appSource = logEntry.AppAssembly.FullName;

				if ((!EventLog.SourceExists(appSource)))
					EventLog.CreateEventSource(appSource, "Application");

				if ((logEntry.LogPriority == LogPriorityLevels._INFO)) {
					EventLog.WriteEntry(appSource, logEntry.ToString(), EventLogEntryType.Information);
				} else if ((logEntry.LogPriority == LogPriorityLevels._ERROR)) {
					EventLog.WriteEntry(appSource, logEntry.ToString(), EventLogEntryType.Error);
				} else if ((logEntry.LogPriority == LogPriorityLevels._WARN)) {
					EventLog.WriteEntry(appSource, logEntry.ToString(), EventLogEntryType.Warning);
				} else {
					EventLog.WriteEntry(appSource, logEntry.ToString());
				}
			} catch (Exception ex) {
				throw new LoggerException("Error writing log to windows event logs", ex, logEntry);
			}

		}

		~WindowsEventLoggerWriter()
		{
		}

		public static WindowsEventLoggerWriter Instance(){
			if (_Instance == null)
				_Instance = new WindowsEventLoggerWriter ();

			return _Instance;
		}
	}
}