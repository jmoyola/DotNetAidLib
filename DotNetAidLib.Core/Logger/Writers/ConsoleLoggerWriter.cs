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
	public class ConsoleLoggerWriter : LoggerWriter
	{
		private static ConsoleLoggerWriter _Instance = null;
		private ConsoleLoggerWriter()
		{
			_MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ALL;
			_MaxLogPriorityVerboseInRunTime = LogPriorityLevels._ALL;
		}

		public override void WriteLog(LogEntry logEntry)
		{
			try {

                if(logEntry.LogPriority<=LogPriorityLevels._WARN)
                    Console.Error.WriteLine(logEntry.ToString());
                else
                    Console.Out.WriteLine(logEntry.ToString());

			} catch (Exception ex) {
				throw new LoggerException("Error writing log to console", ex, logEntry);
			}
		}


		~ConsoleLoggerWriter()
		{
		}

		private static Object oInstance = new object();
		public static ConsoleLoggerWriter Instance(){
			lock (oInstance)
			{
				if (_Instance == null)
					_Instance = new ConsoleLoggerWriter();

				return _Instance;
			}
		}
	}
}