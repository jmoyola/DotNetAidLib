using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Linq;
using DotNetAidLib.Core.Logger.Core;
using DotNetAidLib.Core.Logger.Writers;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Logger.Client{
	public class LoggerWriters : ThreadSafeList<ILoggerWriter>
	{
		private AsyncExceptionHandler _AsyncExceptionHandler;
		public AsyncExceptionHandler AsyncExceptionHandler {
			get { return _AsyncExceptionHandler; }
			set { _AsyncExceptionHandler = value; }
		}

        private Thread WriteLog_Thread = null;
        public void WriteLog(LogEntry logEntry, LoggerEx loggerEx)
		{
			WriteLog_Thread = new Thread(new ParameterizedThreadStart(WriteLog_ThreadStart));
            WriteLog_Thread.Start(logEntry);
		}

        public void Flush() {
            if (WriteLog_Thread != null && WriteLog_Thread.IsAlive)
                WriteLog_Thread.Join();
        }

        private Object oWriteLog_ThreadStartLock = new object();

        private void WriteLog_ThreadStart(object o)
		{
            lock (oWriteLog_ThreadStartLock)
            {
                LogEntry logEntry = (LogEntry)o;

                RunLevels actualRunlevel = LoggerEx.GetActualRunLevel();

                lock (this.Lock)
                {
                    foreach (ILoggerWriter loggerWriter in this)
                    {
                        bool doLog = false;

                        // Especificamos que se realice el log según el run level y el verbose especificado para el
                        if (actualRunlevel.Equals(RunLevels._DEBUGTIME))
                        {
                            doLog = (logEntry.LogPriority <= loggerWriter.MaxLogPriorityVerboseInDebugTime);
                        }
                        else if (actualRunlevel.Equals(RunLevels._RUNTIME))
                        {
                            doLog = (logEntry.LogPriority <= loggerWriter.MaxLogPriorityVerboseInRunTime);
                        }

                        // Si hay que realizar el log....
                        if ((doLog))
                        {
                            if (loggerWriter.AsyncExceptionHandler == null)
                            {
                                loggerWriter.AsyncExceptionHandler = LoggerWriter_AsyncExceptionHandler;
                            }

                            try
                            {
                                loggerWriter.WriteLog(logEntry);
                                if ((loggerWriter.LastLoggerIfNotError))
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogEntry cLogEntry = new LogEntry(logEntry.Logger, logEntry.Instant, "!!!(" + loggerWriter.GetType().Name + ") " + logEntry.Message, logEntry.LogPriority, logEntry.CallAssembly, logEntry.AppAssembly, logEntry.ProcessId, logEntry.Application, logEntry.LogEntryInformation);
                                ILoggerWriter debugLoggerWriter = this.FirstOrDefault(v => typeof(DebugLoggerWriter).IsAssignableFrom(v.GetType()));
                                if (debugLoggerWriter != null)
                                {
                                    debugLoggerWriter.WriteLog(cLogEntry);
                                }
                                else
                                {
                                    LoggerWriter_AsyncExceptionHandler(this, new LoggerException("Error writing to logger '" + loggerWriter.GetType().FullName + "'", ex, cLogEntry));
                                }
                            }
                        }
                    }
                }
            }
		}

		private void LoggerWriter_AsyncExceptionHandler(object sender, Exception ex)
		{
			if (((this.AsyncExceptionHandler != null))) {
				this.AsyncExceptionHandler.Invoke(sender, ex);
			}
		}
	}
}