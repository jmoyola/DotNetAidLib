using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Xml;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.LogEntryInfo;
using DotNetAidLib.Logger.Writers;

namespace DotNetAidLib.Logger.Client
{
    public class Logger : ILogger
    {
        public const string DEBUG_DEFAULT_INSTANCE_ID = "__DEBUG_DEFAULT__";
        public const string DEFAULT_INSTANCE_ID = "__DEFAULT__";
        public static bool RegisterLogInitialization = true;
        public static bool RegisterLogConfiguration = false;

        private static readonly Dictionary<string, Logger> m_Instances = new Dictionary<string, Logger>();
        private static readonly object oInstance = new object();

        private readonly IApplicationConfig _ApplicationConfig;

        protected Logger(string loggerId, LoggerWriters loggerWriters)
            : this(loggerId, loggerWriters, null)
        {
        }

        protected Logger(string loggerId, LoggerWriters loggerWriters, IApplicationConfig applicationConfig)
            : this(loggerId, loggerWriters, null, applicationConfig)
        {
        }

        protected Logger(string loggerId, LoggerWriters loggerWriters, IList<ILogEntryInfo> logEntriesInfo,
            IApplicationConfig applicationConfig)
        {
            if (loggerId == null) throw new NullReferenceException("loggerId can't be null.");

            if (loggerWriters == null) throw new NullReferenceException("loggerWriters can't be null.");

            if (loggerWriters.Count == 0) throw new Exception("loggerWriters must be at most one loggerwriter.");

            if (applicationConfig == null)
                // Establecemos el archivo de configuración del logger y en base a el, inicializamos la propiedad _Configuration
                applicationConfig = XmlApplicationConfigFactory.Instance(FileLocation.UserApplicationDataFolder);

            LogEntryInformation.Add(new ProcessIdLogEntryInfo());
            LogEntryInformation.Add(new ProcessRAMLogEntryInfo());
            //m_LogEntryInformation.Add(new AssemblyLogEntryInfo());

            if (logEntriesInfo != null) logEntriesInfo.ToList().ForEach(v => LogEntryInformation.Add(v));

            _ApplicationConfig = applicationConfig;

            LoggerId = loggerId;
            LoggerWriters = loggerWriters;

            // Inicializamos configuración de logger
            InitConfiguration(false);

            // Creamos el registro de inicio si está configurado para ello
            if (RegisterLogInitialization)
            {
                WriteInfo("Logger '" + LoggerId + "' Initializing...");
                if (RegisterLogConfiguration)
                    WriteInfo("Logger '" + LoggerId + "' configuration:" + Environment.NewLine +
                              ConfigurationToString(_ApplicationConfig.GetGroup("loggerExConfig_" + LoggerId)));
            }
        }

        public IList<ILogEntryInfo> LogEntryInformation { get; } = new List<ILogEntryInfo>();

        public string LoggerId { get; }

        public LoggerWriters LoggerWriters { get; }

        public ExceptionVerbosityLevels ExceptionsVerbosityLevel { get; set; } = ExceptionVerbosityLevels._HIGH;

        public void WriteFatal(string message)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._FATAL, ref callAssembly, "");
        }

        public void WriteFatal(string message, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._FATAL, ref callAssembly, processId);
        }

        public void WriteError(string message)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._ERROR, ref callAssembly, "");
        }

        public void WriteError(string message, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._ERROR, ref callAssembly, processId);
        }

        public void WriteWarning(string message)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._WARN, ref callAssembly, "");
        }

        public void WriteWarning(string message, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._WARN, ref callAssembly, processId);
        }

        public void WriteInfo(string message)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._INFO, ref callAssembly, "");
        }

        public void WriteInfo(string message, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._INFO, ref callAssembly, processId);
        }

        public void WriteDebug(string message)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._DEBUG, ref callAssembly, "");
        }

        public void WriteDebug(string message, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._DEBUG, ref callAssembly, processId);
        }

        public void WriteTrace(string message)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._TRACE, ref callAssembly, "");
        }

        public void WriteTrace(string message, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, LogPriorityLevels._TRACE, ref callAssembly, processId);
        }

        public void Write(string message, LogPriorityLevels logPriority)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, logPriority, ref callAssembly, "");
        }

        public void Write(string message, LogPriorityLevels logPriority, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(" " + message, logPriority, ref callAssembly, processId);
        }

        // Excepciones

        public void WriteException(string message, Exception ex, LogPriorityLevels logPriority, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(
                (string.IsNullOrEmpty(message) ? "" : message + ":") +
                LoggerWriter.ExceptionToString(ex, ExceptionsVerbosityLevel), logPriority, ref callAssembly, processId);
        }

        public void WriteException(Exception ex, LogPriorityLevels logPriority, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(LoggerWriter.ExceptionToString(ex, ExceptionsVerbosityLevel), logPriority, ref callAssembly,
                processId);
        }

        public void WriteException(string message, Exception exception, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(
                (string.IsNullOrEmpty(message) ? "" : message + ":") +
                LoggerWriter.ExceptionToString(exception, ExceptionsVerbosityLevel), LogPriorityLevels._ERROR,
                ref callAssembly, processId);
        }

        public void WriteException(Exception ex, string processId)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(LoggerWriter.ExceptionToString(ex, ExceptionsVerbosityLevel), LogPriorityLevels._ERROR,
                ref callAssembly, processId);
        }


        //Excepciones sin processId pasado

        public void WriteException(string message, Exception ex, LogPriorityLevels logPriority)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(
                (string.IsNullOrEmpty(message) ? "" : message + ":") +
                LoggerWriter.ExceptionToString(ex, ExceptionsVerbosityLevel), logPriority, ref callAssembly, "");
        }

        public void WriteException(Exception ex, LogPriorityLevels logPriority)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(LoggerWriter.ExceptionToString(ex, ExceptionsVerbosityLevel), logPriority, ref callAssembly, "");
        }

        public void WriteException(string message, Exception exception)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(
                (string.IsNullOrEmpty(message) ? "" : message + ":") +
                LoggerWriter.ExceptionToString(exception, ExceptionsVerbosityLevel), LogPriorityLevels._ERROR,
                ref callAssembly, "");
        }

        public void WriteException(Exception ex)
        {
            var callAssembly = Assembly.GetCallingAssembly();

            WriteLog(LoggerWriter.ExceptionToString(ex, ExceptionsVerbosityLevel), LogPriorityLevels._ERROR,
                ref callAssembly, "");
        }

        public void Flush()
        {
            LoggerWriters.Flush();
        }

        public void WriteLogEntry(LogEntry logEntry)
        {
            LoggerWriters.WriteLog(logEntry, this);
        }

        public void UpdateConfiguration()
        {
            InitConfiguration(true);
        }

        private void WriteLog(string message, LogPriorityLevels logPriority, ref Assembly callAssembly,
            string processId)
        {
            var sMsg = message.Replace("\r\n  ", "\r\n");
            sMsg = sMsg.Replace("\r\n", "\r\n  ");

            var logInstant = DateTime.Now;
            var appAssembly = Helper.GetEntryAssembly();

            var logEntry = new LogEntry(this, logInstant, sMsg, logPriority, callAssembly, appAssembly, processId,
                LogEntryInformation);

            WriteLogEntry(logEntry);
        }

        private void LoggerWriter_AsyncExceptionHandler(object sender, Exception ex)
        {
            var loggerEx = (LoggerException) ex;
            var cLogEntry = (LogEntry) loggerEx.LogEntry.Clone();
            cLogEntry.Message = "Error writing to logger '" + sender.GetType().FullName + "' this log entry: " +
                                cLogEntry.Message;
            if (LoggerWriters.Count > 0) LoggerWriters[0].WriteLog(cLogEntry);
        }

        private void InitConfiguration(bool update)
        {
            var loggerExConfigGroup = default(IApplicationConfigGroup);
            if (_ApplicationConfig.GroupExist("loggerExConfig_" + LoggerId))
                loggerExConfigGroup = _ApplicationConfig.GetGroup("loggerExConfig_" + LoggerId);
            else
                loggerExConfigGroup = _ApplicationConfig.AddGroup("loggerExConfig_" + LoggerId);

            // Configuración genérica del logger
            if (loggerExConfigGroup.ConfigurationExist("ExceptionsVerbosityLevel"))
                ExceptionsVerbosityLevel = loggerExConfigGroup
                    .GetConfiguration<ExceptionVerbosityLevels>("ExceptionsVerbosityLevel").Value;
            else
                loggerExConfigGroup.AddConfiguration("ExceptionsVerbosityLevel", ExceptionsVerbosityLevel, true);

            // Configuración de cada log entry info
            foreach (var logEntryInfo in LogEntryInformation)
            {
                IApplicationConfigGroup configGroup = null;
                if (loggerExConfigGroup.GroupExist(logEntryInfo.GetType().FullName))
                {
                    configGroup = loggerExConfigGroup.GetGroup(logEntryInfo.GetType().FullName);
                }
                else
                {
                    configGroup = loggerExConfigGroup.AddGroup(logEntryInfo.GetType().FullName);
                    configGroup.GroupInfo = "LogEntryInfo";
                }

                if (update)
                    logEntryInfo.SaveConfiguration(configGroup);
                else
                    logEntryInfo.InitConfiguration(configGroup);
            }

            // Configuración de cada loggerwriter
            foreach (var logW in LoggerWriters)
            {
                IApplicationConfigGroup configGroup = null;
                if (loggerExConfigGroup.GroupExist(logW.GetType().FullName))
                {
                    configGroup = loggerExConfigGroup.GetGroup(logW.GetType().FullName);
                }
                else
                {
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

        private string ConfigurationToString(IApplicationConfigGroup config)
        {
            var ret = new StringBuilder();
            ret.AppendLine("- Logger configuration:");
            ret.AppendLine("    - Generic logger configuration:");
            foreach (var cfg in config.ConfigurationKeys)
            {
                var value = config.GetConfiguration<object>(cfg.Key).Value;
                string sValue = null;
                if (value == null)
                    sValue = "null";
                else
                    sValue = value.ToString();
                ret.AppendLine("      - " + cfg.Key + "(" + cfg.Value.FullName + ") = " + sValue);
            }

            // Configuración de cada entry info
            ret.AppendLine("  - Log entries:");
            foreach (var cfgGroup in config.Groups.Where(v => v.GroupInfo == "LogEntryInfo"))
            {
                ret.AppendLine("    - Log entryI info '" + cfgGroup.GroupName + "' configuration:");
                foreach (var cfg in cfgGroup.ConfigurationKeys)
                {
                    var value = cfgGroup.GetConfiguration(cfg.Key).Value;
                    string sValue = null;
                    if (value == null)
                        sValue = "null";
                    else
                        sValue = value.ToString();
                    ret.AppendLine("      - " + cfg.Key + "(" + cfg.Value.FullName + ") = " + sValue);
                }
            }

            // Configuración de cada loggerWriter
            ret.AppendLine("  - Logger writers:");
            foreach (var cfgGroup in config.Groups.Where(v => v.GroupInfo == "LoggerWriter"))
            {
                ret.AppendLine("    - Logger writer '" + cfgGroup.GroupName + "' configuration:");
                foreach (var cfg in cfgGroup.ConfigurationKeys)
                {
                    var value = cfgGroup.GetConfiguration(cfg.Key).Value;
                    string sValue = null;
                    if (value == null)
                        sValue = "null";
                    else
                        sValue = value.ToString();
                    ret.AppendLine("      - " + cfg.Key + "(" + cfg.Value.FullName + ") = " + sValue);
                }
            }

            return ret.ToString();
        }

        private static string ReverseLines(string msg)
        {
            var aMsgs = Regex.Split(msg, "[\\r]\\n");
            Array.Reverse(aMsgs);
            return string.Join("\r\n", aMsgs);
        }

        protected internal static RunLevels GetActualRunLevel()
        {
            var ret = RunLevels._RUNTIME;

            if (EnvironmentHelper.IsCompiledInDebugMode())
                ret = RunLevels._DEBUGTIME;

            return ret;
        }

        public static ILogger Instance(string loggerId, LoggerWriters loggerWriters)
        {
            return Instance(loggerId, loggerWriters, null);
        }

        public static ILogger Instance(string loggerId, LoggerWriters loggerWriters,
            IApplicationConfig applicationConfig)
        {
            return Instance(loggerId, loggerWriters, null, applicationConfig);
        }

        public static ILogger Instance(string loggerId, LoggerWriters loggerWriters,
            IList<ILogEntryInfo> loggerEntriesInfo, IApplicationConfig applicationConfig)
        {
            lock (oInstance)
            {
                Logger logRet = null;

                if (loggerId == null)
                    loggerId = DEFAULT_INSTANCE_ID;

                if (m_Instances.ContainsKey(loggerId))
                {
                    logRet = m_Instances[loggerId];
                }
                else
                {
                    logRet = new Logger(loggerId, loggerWriters, loggerEntriesInfo, applicationConfig);
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

        public static ILogger Instance(LoggerWriters loggerWriters, IList<ILogEntryInfo> loggerEntriesInfo,
            IApplicationConfig applicationConfig)
        {
            return Instance(DEFAULT_INSTANCE_ID, loggerWriters, loggerEntriesInfo, applicationConfig);
        }

        public static ILogger Instance(string loggerId)
        {
            lock (oInstance)
            {
                if (m_Instances.ContainsKey(loggerId))
                    return m_Instances[loggerId];
                throw new LoggerException(
                    "You before must to call instance with loggerId and loggersWriters list before use it.");
            }
        }

        public static ILogger Instance()
        {
            lock (oInstance)
            {
                if (m_Instances.ContainsKey(DEFAULT_INSTANCE_ID)) return m_Instances[DEFAULT_INSTANCE_ID];

                if (!m_Instances.ContainsKey(DEBUG_DEFAULT_INSTANCE_ID))
                {
                    var debugDefaultLoggerWriters = new LoggerWriters();
                    debugDefaultLoggerWriters.Add(DebugLoggerWriter.Instance());
                    return Instance(DEBUG_DEFAULT_INSTANCE_ID, debugDefaultLoggerWriters);
                }

                return m_Instances[DEBUG_DEFAULT_INSTANCE_ID];
                //throw new LoggerException("You before must to call instance with loggersWriters list before use it.");
            }
        }

        public static bool InstanceExists(string loggerId)
        {
            lock (oInstance)
            {
                if (m_Instances.ContainsKey(loggerId))
                    return true;
                return false;
            }
        }
    }
}