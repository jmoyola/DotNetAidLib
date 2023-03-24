using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Logger.Core{
	public abstract class LogEntryConsumer : ILogEntryConsumer
	{
		protected AsyncExceptionHandler _AsyncExceptionHandler = null;
		public AsyncExceptionHandler AsyncExceptionHandler {
			get { return _AsyncExceptionHandler; }
			set { _AsyncExceptionHandler = value; }
		}

		public abstract void ProcessLogEntry(LogEntry logEntry);
	}
}