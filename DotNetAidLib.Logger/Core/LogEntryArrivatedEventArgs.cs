using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace DotNetAidLib.Logger.Core{
	public class LogEntryArrivatedEventArgs : EventArgs
	{
		private LogEntry _LogEntry;
		public LogEntryArrivatedEventArgs(LogEntry logEntry)
		{
			_LogEntry = logEntry;
		}

		public LogEntry LogEntry {
			get { return _LogEntry; }
		}
	}
}