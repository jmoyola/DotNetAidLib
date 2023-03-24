using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Logger.Core{
	public class LoggerException : Exception
	{
		private LogEntry _LogEntry;
		public LoggerException() : base()
		{
		}

		public LoggerException(string message) : base(message)
		{
		}

		public LoggerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public LoggerException(string message, LogEntry logEntry) : base(message)
		{
			_LogEntry = logEntry;
		}

		public LoggerException(string message, Exception innerException, LogEntry logEntry) : base(message, innerException)
		{
			_LogEntry = logEntry;
		}

		public LogEntry LogEntry {
			get { return _LogEntry; }
			set { _LogEntry = value; }
		}
	}
}