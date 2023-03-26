using System;
using System.Linq;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.Writers;

namespace DotNetAidLib.Logger.Client
{
    public class LoggerWriters : ThreadSafeList<ILoggerWriter>
    {
        private readonly object oWriteLog_ThreadStartLock = new object();

        private Thread WriteLog_Thread;

        public AsyncExceptionHandler AsyncExceptionHandler { get; set; }

        public void WriteLog(LogEntry logEntry, Logger logger)
        {
            WriteLog_Thread = new Thread(WriteLog_ThreadStart);
            WriteLog_Thread.Start(logEntry);
        }

        public void Flush()
        {
            if (WriteLog_Thread != null && WriteLog_Thread.IsAlive)
                WriteLog_Thread.Join();
        }

        private void WriteLog_ThreadStart(object o)
        {
            lock (oWriteLog_ThreadStartLock)
            {
                var logEntry = (LogEntry) o;

                var actualRunlevel = Logger.GetActualRunLevel();

                lock (Lock)
                {
                    foreach (var loggerWriter in this)
                    {
                        var doLog = false;

                        // Especificamos que se realice el log según el run level y el verbose especificado para el
                        if (actualRunlevel.Equals(RunLevels._DEBUGTIME))
                            doLog = logEntry.LogPriority <= loggerWriter.MaxLogPriorityVerboseInDebugTime;
                        else if (actualRunlevel.Equals(RunLevels._RUNTIME))
                            doLog = logEntry.LogPriority <= loggerWriter.MaxLogPriorityVerboseInRunTime;

                        // Si hay que realizar el log....
                        if (doLog)
                        {
                            if (loggerWriter.AsyncExceptionHandler == null)
                                loggerWriter.AsyncExceptionHandler = LoggerWriter_AsyncExceptionHandler;

                            try
                            {
                                loggerWriter.WriteLog(logEntry);
                                if (loggerWriter.LastLoggerIfNotError) break;
                            }
                            catch (Exception ex)
                            {
                                var cLogEntry = new LogEntry(logEntry.Logger, logEntry.Instant,
                                    "!!!(" + loggerWriter.GetType().Name + ") " + logEntry.Message,
                                    logEntry.LogPriority, logEntry.CallAssembly, logEntry.AppAssembly,
                                    logEntry.ProcessId, logEntry.Application, logEntry.LogEntryInformation);
                                var debugLoggerWriter = this.FirstOrDefault(v =>
                                    typeof(DebugLoggerWriter).IsAssignableFrom(v.GetType()));
                                if (debugLoggerWriter != null)
                                    debugLoggerWriter.WriteLog(cLogEntry);
                                else
                                    LoggerWriter_AsyncExceptionHandler(this,
                                        new LoggerException(
                                            "Error writing to logger '" + loggerWriter.GetType().FullName + "'", ex,
                                            cLogEntry));
                            }
                        }
                    }
                }
            }
        }

        private void LoggerWriter_AsyncExceptionHandler(object sender, Exception ex)
        {
            if (AsyncExceptionHandler != null) AsyncExceptionHandler.Invoke(sender, ex);
        }
    }
}