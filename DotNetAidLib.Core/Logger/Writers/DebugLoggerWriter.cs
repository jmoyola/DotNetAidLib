using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Collections.Specialized;
using DotNetAidLib.Core.Logger.Core;

namespace DotNetAidLib.Core.Logger.Writers{
	public class DebugLoggerWriter : LoggerWriter
	{
		private static DebugLoggerWriter _Instance = null;
		private DebugLoggerWriter()
		{
			_MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ALL;
			_MaxLogPriorityVerboseInRunTime = LogPriorityLevels._OFF;
		}

		public override void WriteLog(LogEntry logEntry)
		{
			try {
				Debug.WriteLine(logEntry.ToString());
			} catch (Exception ex) {
				throw new LoggerException("Error writing log to debug", ex, logEntry);
			}
		}


		~DebugLoggerWriter()
		{
		}

		private static Object oInstance = new object();
		public static DebugLoggerWriter Instance(){
			lock (oInstance)
			{
				if (_Instance == null)
					_Instance = new DebugLoggerWriter();

				return _Instance;
			}
		}
	}
}