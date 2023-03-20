using System;
using System.IO;
using DotNetAidLib.Core.Logger.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Logger.Writers{
	public class SyslogLoggerWriter : LoggerWriter
	{
		private FileInfo loggerPath;
		private static SyslogLoggerWriter _Instance=null;

		private SyslogLoggerWriter(FileInfo loggerPath)
		{
			Assert.Exists( loggerPath, nameof(loggerPath));
			this.loggerPath = loggerPath;
		}

		public override void WriteLog(LogEntry logEntry)
		{
			try
			{
				this.loggerPath.CmdExecuteSync(LogPriorityLevels._INFO.ToString().Substring(1) + " " + logEntry.ToString());
			} catch (Exception ex) {
				throw new LoggerException("Error writing log to syslog", ex, logEntry);
			}

		}

		~SyslogLoggerWriter()
		{
		}

		public static SyslogLoggerWriter Instance(FileInfo loggerPath){
			if (_Instance == null)
				_Instance = new SyslogLoggerWriter (loggerPath);

			return _Instance;
		}
		
		public static SyslogLoggerWriter Instance(){
			if (_Instance == null)
				_Instance = new SyslogLoggerWriter (new FileInfo(".").FromPathEnvironmentVariable("logger"));

			return _Instance;
		}
	}
}