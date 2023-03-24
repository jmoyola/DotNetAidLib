using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;

using System.Text;


using System.Linq;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Xml;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.LogEntryInfo;
using DotNetAidLib.Logger.Writers;

namespace DotNetAidLib.Logger.Client{
	public class LoggerEx : ILogger
	{
        public static bool RegisterLogInitialization = true;
        public static bool RegisterLogConfiguration = false;

        private static Dictionary<string, LoggerEx> m_Instances = new Dictionary<string, LoggerEx>();
        private IList<ILogEntryInfo> m_LogEntryInformation = new List<ILogEntryInfo>();
		private ExceptionVerbosityLevels m_ExceptionsVerbosityLevel = ExceptionVerbosityLevels._HIGH;
		private LoggerWriters _LoggerWriters;
		private string _LoggerId;

		private IApplicationConfig _ApplicationConfig;

		protected LoggerEx(string loggerId, LoggerWriters loggerWriters)
            :this(loggerId,loggerWriters,null){}

        protected LoggerEx(string loggerId, LoggerWriters loggerWriters, IApplicationConfig applicationConfig)
            : this(loggerId, loggerWriters, null, applicationConfig) {}

        protected LoggerEx(string loggerId, LoggerWriters loggerWriters, IList<ILogEntryInfo> logEntriesInfo, IApplicationConfig applicationConfig){
			if ((loggerId == null)) {
				throw new NullReferenceException("loggerId can't be null.");
			}

			if ((loggerWriters == null)) {
				throw new NullReferenceException("loggerWriters can't be null.");
			}

			if ((loggerWriters.Count == 0)) {
				throw new Exception("loggerWriters must be at most one loggerwriter.");
			}

            if (applicationConfig == null)
            {
                // Establecemos el archivo de configuración del logger y en base a el, inicializamos la propiedad _Configuration
                applicationConfig = XmlApplicationConfigFactory.Instance(FileLocation.UserApplicationDataFolder);
            }

            m_LogEntryInformation.Add(new ProcessIdLogEntryInfo());
            m_LogEntryInformation.Add(new ProcessRAMLogEntryInfo());
            //m_LogEntryInformation.Add(new AssemblyLogEntryInfo());

            if (logEntriesInfo != null)
            {
                logEntriesInfo.ToList().ForEach(v=>m_LogEntryInformation.Add(v));
            }

			_ApplicationConfig = applicationConfig;

			_LoggerId = loggerId;
			_LoggerWriters = loggerWriters;

			// Inicializamos configuración de logger
			this.InitConfiguration(false);

            // Creamos el registro de inicio si está configurado para ello
            if (RegisterLogInitialization)
            {
                this.WriteInfo("Logger '" + this._LoggerId + "' Initializing...");
                if (RegisterLogConfiguration)
                    this.WriteInfo("Logger '" + this._LoggerId + "' configuration:" + Environment.NewLine + ConfigurationToString(_ApplicationConfig.GetGroup("loggerExConfig_" + this.LoggerId)));
            }
		}

        public IList<ILogEntryInfo> LogEntryInformation {
            get { return m_LogEntryInformation; }
        }

		public string LoggerId {
			get { return _LoggerId; }
		}

		public LoggerWriters LoggerWriters {
			get { return _LoggerWriters; }
		}

		public ExceptionVerbosityLevels ExceptionsVerbosityLevel {
			get { return m_ExceptionsVerbosityLevel; }
			set { this.m_ExceptionsVerbosityLevel = value; }
		}

		public void WriteFatal(string message)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._FATAL, ref callAssembly, "");
		}

		public void WriteFatal(string message, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._FATAL, ref callAssembly, processId);
		}

		public void WriteError(string message)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._ERROR, ref callAssembly, "");
		}

		public void WriteError(string message, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._ERROR, ref callAssembly, processId);
		}

		public void WriteWarning(string message)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._WARN, ref callAssembly, "");
		}

		public void WriteWarning(string message, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._WARN, ref callAssembly, processId);
		}

		public void WriteInfo(string message)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._INFO, ref callAssembly, "");
		}

		public void WriteInfo(string message, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._INFO, ref callAssembly, processId);
		}

		public void WriteDebug(string message)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._DEBUG, ref callAssembly, "");
		}

		public void WriteDebug(string message, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._DEBUG, ref callAssembly, processId);
		}

		public void WriteTrace(string message)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._TRACE, ref callAssembly, "");
		}

		public void WriteTrace(string message, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, LogPriorityLevels._TRACE, ref callAssembly, processId);
		}

		public void Write(string message, LogPriorityLevels logPriority)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, logPriority, ref callAssembly, "");
		}

		public void Write(string message, LogPriorityLevels logPriority, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(" " + message, logPriority, ref callAssembly, processId);
		}

		// Excepciones

		public void WriteException(string message, Exception ex, LogPriorityLevels logPriority, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog((string.IsNullOrEmpty(message) ? "" : message + ":") + LoggerWriter.ExceptionToString(ex, m_ExceptionsVerbosityLevel), logPriority, ref callAssembly, processId);
		}

		public void WriteException(Exception ex, LogPriorityLevels logPriority, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(LoggerWriter.ExceptionToString(ex, m_ExceptionsVerbosityLevel), logPriority, ref callAssembly, processId);
		}

		public void WriteException(string message, Exception exception, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog((string.IsNullOrEmpty(message) ? "" : message + ":") + LoggerWriter.ExceptionToString(exception, m_ExceptionsVerbosityLevel), LogPriorityLevels._ERROR, ref callAssembly, processId);
		}

		public void WriteException(Exception ex, string processId)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(LoggerWriter.ExceptionToString(ex, m_ExceptionsVerbosityLevel), LogPriorityLevels._ERROR, ref callAssembly, processId);
		}


		//Excepciones sin processId pasado

		public void WriteException(string message, Exception ex, LogPriorityLevels logPriority)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog((string.IsNullOrEmpty(message) ? "" : message + ":") + LoggerWriter.ExceptionToString(ex, m_ExceptionsVerbosityLevel), logPriority, ref callAssembly, "");
		}

		public void WriteException(Exception ex, LogPriorityLevels logPriority)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(LoggerWriter.ExceptionToString(ex, m_ExceptionsVerbosityLevel), logPriority, ref callAssembly, "");
		}

		public void WriteException(string message, Exception exception)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog((string.IsNullOrEmpty(message) ? "" : message + ":") + LoggerWriter.ExceptionToString(exception, m_ExceptionsVerbosityLevel), LogPriorityLevels._ERROR, ref callAssembly, "");
		}

		public void WriteException(Exception ex)
		{
			Assembly callAssembly = Assembly.GetCallingAssembly();

			this.WriteLog(LoggerWriter.ExceptionToString(ex, m_ExceptionsVerbosityLevel), LogPriorityLevels._ERROR, ref callAssembly, "");
		}

        public void Flush() {
            _LoggerWriters.Flush();
        }

        public void WriteLogEntry(LogEntry logEntry){
			_LoggerWriters.WriteLog(logEntry, this);
		}

		private void WriteLog(string message, LogPriorityLevels logPriority, ref Assembly callAssembly, string processId)
		{

			string sMsg = message.Replace("\r\n  ", "\r\n");
			sMsg = sMsg.Replace("\r\n", "\r\n  ");

			DateTime logInstant = DateTime.Now;
			Assembly appAssembly =Helper.GetEntryAssembly();

            LogEntry logEntry = new LogEntry(this, logInstant, sMsg, logPriority, callAssembly, appAssembly, processId, this.m_LogEntryInformation);

			this.WriteLogEntry(logEntry);

		}

		private void LoggerWriter_AsyncExceptionHandler(object sender, Exception ex)
		{
			LoggerException loggerEx = (LoggerException)ex;
			LogEntry cLogEntry = (LogEntry)loggerEx.LogEntry.Clone();
			cLogEntry.Message = "Error writing to logger '" + sender.GetType().FullName + "' this log entry: " + cLogEntry.Message;
			if ((_LoggerWriters.Count > 0)) {
				_LoggerWriters[0].WriteLog(cLogEntry);
			}
		}

		private void InitConfiguration(bool update)
		{
			IApplicationConfigGroup loggerExConfigGroup = default(IApplicationConfigGroup);
			if ((_ApplicationConfig.GroupExist("loggerExConfig_" + this.LoggerId))) {
				loggerExConfigGroup = _ApplicationConfig.GetGroup("loggerExConfig_" + this.LoggerId);
			} else {
				loggerExConfigGroup = _ApplicationConfig.AddGroup("loggerExConfig_" + this.LoggerId);
			}

			// Configuración genérica del logger
			if ((loggerExConfigGroup.ConfigurationExist("ExceptionsVerbosityLevel"))) {
				m_ExceptionsVerbosityLevel = loggerExConfigGroup.GetConfiguration<ExceptionVerbosityLevels>("ExceptionsVerbosityLevel").Value;
			} else {
				loggerExConfigGroup.AddConfiguration<ExceptionVerbosityLevels>("ExceptionsVerbosityLevel", m_ExceptionsVerbosityLevel, true);
			}

            // Configuración de cada log entry info
            foreach (ILogEntryInfo logEntryInfo in this.LogEntryInformation)
            {
                IApplicationConfigGroup configGroup = null;
                if ((loggerExConfigGroup.GroupExist(logEntryInfo.GetType().FullName)))
                {
                    configGroup = loggerExConfigGroup.GetGroup(logEntryInfo.GetType().FullName);
                }
                else
                {
                    configGroup = loggerExConfigGroup.AddGroup(logEntryInfo.GetType().FullName);
                    configGroup.GroupInfo = "LogEntryInfo";
                }

                if(update)
                    logEntryInfo.SaveConfiguration(configGroup);
                else
                    logEntryInfo.InitConfiguration(configGroup);
            }

			// Configuración de cada loggerwriter
			foreach (ILoggerWriter logW in _LoggerWriters) {
				IApplicationConfigGroup configGroup = null;
				if ((loggerExConfigGroup.GroupExist(logW.GetType().FullName))) {
					configGroup = loggerExConfigGroup.GetGroup(logW.GetType().FullName);
				} else {
                    configGroup = loggerExConfigGroup.AddGroup(logW.GetType().FullName);
                    configGroup.GroupInfo = "LoggerWriter";
				}
                if (update)
                    logW.SaveConfiguration(configGroup);
                else
                    logW.InitConfiguration(configGroup);
            }

			_ApplicationConfig.Save();
		}

		public void UpdateConfiguration()
		{
            this.InitConfiguration(true);
		}

		private string ConfigurationToString(IApplicationConfigGroup config)
		{
			StringBuilder ret = new StringBuilder();
			ret.AppendLine("- Logger configuration:");
            ret.AppendLine("    - Generic logger configuration:");
            foreach (KeyValuePair<string, Type> cfg in config.ConfigurationKeys) {
				object value = config.GetConfiguration<object>(cfg.Key).Value;
				string sValue = null;
				if ((value == null)) {
					sValue = "null";
				} else {
					sValue = value.ToString();
				}
				ret.AppendLine("      - " + cfg.Key + "(" + cfg.Value.FullName + ") = " + sValue);
			}

            // Configuración de cada entry info
            ret.AppendLine("  - Log entries:");
            foreach (IApplicationConfigGroup cfgGroup in config.Groups.Where(v=>v.GroupInfo== "LogEntryInfo")) {
                ret.AppendLine("    - Log entryI info '" + cfgGroup.GroupName + "' configuration:");
				foreach (KeyValuePair<string, Type> cfg in cfgGroup.ConfigurationKeys) {
					object value = cfgGroup.GetConfiguration(cfg.Key).Value;
					string sValue = null;
					if ((value == null)) {
						sValue = "null";
					} else {
						sValue = value.ToString();
					}
					ret.AppendLine("      - " + cfg.Key + "(" + cfg.Value.FullName + ") = " + sValue);
				}
			}

            // Configuración de cada loggerWriter
            ret.AppendLine("  - Logger writers:");
            foreach (IApplicationConfigGroup cfgGroup in config.Groups.Where(v => v.GroupInfo == "LoggerWriter"))
            {
                ret.AppendLine("    - Logger writer '" + cfgGroup.GroupName + "' configuration:");
                foreach (KeyValuePair<string, Type> cfg in cfgGroup.ConfigurationKeys)
                {
                    object value = cfgGroup.GetConfiguration(cfg.Key).Value;
                    string sValue = null;
                    if ((value == null))
                    {
                        sValue = "null";
                    }
                    else
                    {
                        sValue = value.ToString();
                    }
                    ret.AppendLine("      - " + cfg.Key + "(" + cfg.Value.FullName + ") = " + sValue);
                }
            }

            return ret.ToString();
		}

		private static string ReverseLines(string msg)
		{
			string[] aMsgs = System.Text.RegularExpressions.Regex.Split(msg, "[\\r]\\n");
			Array.Reverse(aMsgs);
			return string.Join("\r\n", aMsgs);
		}

		protected static internal RunLevels GetActualRunLevel()
		{
			RunLevels ret = RunLevels._RUNTIME;

			if(EnvironmentHelper.IsCompiledInDebugMode())
				ret=RunLevels._DEBUGTIME;

			return ret;
		}

		public static ILogger Instance(string loggerId, LoggerWriters loggerWriters){
			return Instance (loggerId, loggerWriters, null);
		}

        public static ILogger Instance(string loggerId, LoggerWriters loggerWriters, IApplicationConfig applicationConfig){
            return Instance(loggerId, loggerWriters, null, applicationConfig);
        }

        public const String DEBUG_DEFAULT_INSTANCE_ID = "__DEBUG_DEFAULT__";
        public const String DEFAULT_INSTANCE_ID = "__DEFAULT__";
        private static Object oInstance = new object();
        public static ILogger Instance(string loggerId, LoggerWriters loggerWriters, IList<ILogEntryInfo> loggerEntriesInfo, IApplicationConfig applicationConfig)
		{
			lock(oInstance){
    			LoggerEx logRet = null;

                if (loggerId == null)
                    loggerId = DEFAULT_INSTANCE_ID;

                if ((m_Instances.ContainsKey(loggerId))) {
    				logRet = m_Instances[loggerId];
    			} else {
    				logRet = new LoggerEx(loggerId, loggerWriters, loggerEntriesInfo, applicationConfig);
    				m_Instances.Add(loggerId, logRet);
    			}

    			return logRet;
			}
		}

		public static ILogger Instance(LoggerWriters loggerWriters)
		{
			return Instance(DEFAULT_INSTANCE_ID, loggerWriters);
		}

		public static ILogger Instance(LoggerWriters loggerWriters, IApplicationConfig applicationConfig)
		{
			return Instance(DEFAULT_INSTANCE_ID, loggerWriters, applicationConfig);
		}

        public static ILogger Instance(LoggerWriters loggerWriters, IList<ILogEntryInfo> loggerEntriesInfo, IApplicationConfig applicationConfig)
        {
            return Instance(DEFAULT_INSTANCE_ID, loggerWriters, loggerEntriesInfo, applicationConfig);
        }

        public static ILogger Instance(string loggerId)
		{
			lock (oInstance)
			{
				if ((m_Instances.ContainsKey(loggerId)))
				{
					return m_Instances[loggerId];
				}
				else
				{
					throw new LoggerException("You before must to call instance with loggerId and loggersWriters list before use it.");
				}
			}
		}

		public static ILogger Instance()
		{
			lock (oInstance)
			{
				if (m_Instances.ContainsKey(DEFAULT_INSTANCE_ID))
				{
					return m_Instances[DEFAULT_INSTANCE_ID];
				}
				else
				{
					if (!m_Instances.ContainsKey(DEBUG_DEFAULT_INSTANCE_ID))
					{
						LoggerWriters debugDefaultLoggerWriters = new LoggerWriters();
						debugDefaultLoggerWriters.Add(DebugLoggerWriter.Instance());
						return Instance(DEBUG_DEFAULT_INSTANCE_ID, debugDefaultLoggerWriters);
					}

					return m_Instances[DEBUG_DEFAULT_INSTANCE_ID];
					//throw new LoggerException("You before must to call instance with loggersWriters list before use it.");
				}
			}
		}

		public static bool InstanceExists(string loggerId)
		{
			lock (oInstance)
			{
				if ((m_Instances.ContainsKey(loggerId)))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}