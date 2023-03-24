using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace DotNetAidLib.Logger.Core{
	public interface ILogEntryConsumer
	{
		AsyncExceptionHandler AsyncExceptionHandler { get; set; }
		void ProcessLogEntry(LogEntry logEntry);
	}
}