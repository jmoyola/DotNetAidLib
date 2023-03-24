using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Logger.Core{
	public abstract class LoggerWriter : ILoggerWriter
	{

		protected LogPriorityLevels _MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ALL;
		protected LogPriorityLevels _MaxLogPriorityVerboseInRunTime = LogPriorityLevels._INFO;

		protected bool _LastLoggerIfNotError = false;

		private AsyncExceptionHandler _AsyncExceptionHandler = null;
		public LogPriorityLevels MaxLogPriorityVerboseInDebugTime {
			get { return _MaxLogPriorityVerboseInDebugTime; }
			set { _MaxLogPriorityVerboseInDebugTime = value; }
		}

		public LogPriorityLevels MaxLogPriorityVerboseInRunTime {
			get { return _MaxLogPriorityVerboseInRunTime; }
			set { _MaxLogPriorityVerboseInRunTime = value; }
		}

		public AsyncExceptionHandler AsyncExceptionHandler {
			get { return _AsyncExceptionHandler; }
			set { _AsyncExceptionHandler = value; }
		}

		public bool LastLoggerIfNotError {
			get { return _LastLoggerIfNotError; }
			set { _LastLoggerIfNotError = value; }
		}

		public ILoggerWriter SetLastLoggerIfNotError()
		{
			_LastLoggerIfNotError = true;
			return this;
		}

		public abstract void WriteLog(LogEntry logEntry);


        public virtual void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			if ((configGroup.ConfigurationExist("MaxLogPriorityVerboseInDebugTime"))) {
				_MaxLogPriorityVerboseInDebugTime = configGroup.GetConfiguration<LogPriorityLevels>("MaxLogPriorityVerboseInDebugTime", LogPriorityLevels._ALL).Value;
			} else {
				configGroup.AddConfiguration<LogPriorityLevels>("MaxLogPriorityVerboseInDebugTime", _MaxLogPriorityVerboseInDebugTime, true);
			}

			if ((configGroup.ConfigurationExist("MaxLogPriorityVerboseInRunTime"))) {
				_MaxLogPriorityVerboseInRunTime = configGroup.GetConfiguration<LogPriorityLevels>("MaxLogPriorityVerboseInRunTime", LogPriorityLevels._INFO).Value;
			} else {
				configGroup.AddConfiguration<LogPriorityLevels>("MaxLogPriorityVerboseInRunTime", _MaxLogPriorityVerboseInRunTime, true);
			}
		}

		public virtual void SaveConfiguration(IApplicationConfigGroup configGroup)
		{
			configGroup.AddConfiguration<LogPriorityLevels>("MaxLogPriorityVerboseInDebugTime", _MaxLogPriorityVerboseInDebugTime, true);
			configGroup.AddConfiguration<LogPriorityLevels>("MaxLogPriorityVerboseInRunTime", _MaxLogPriorityVerboseInRunTime, true);
		}

		public static String ExceptionToString(Exception exception, ExceptionVerbosityLevels exceptionsVerbosityLevel)
		{
			try
			{
				return exception.ToString();//.Indent(1, 80);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in ExceptionParser.", ex);
			}
		}

		public static void ExceptionParser(Exception ex, ExceptionVerbosityLevels exceptionsVerbosityLevel, StreamWriter sw, int exceptionlevel=1)
		{
			Assert.NotNull( sw, nameof(sw));

			// Exception.Message
			sw.WriteLine(ex.Message.Indent(exceptionlevel));
			
			// Exception.Source
			if (ex.Source != null)
				sw.WriteLine(("Source: " + ex.Source).Indent(exceptionlevel));
			
			// Exception.TargetSite
			if (ex.TargetSite != null)
				sw.WriteLine(("Target: " + ex.TargetSite.ToString()).Indent(exceptionlevel));
			
			if ((int)exceptionsVerbosityLevel > 2 && exceptionlevel == 1) {
				sw.WriteLine("StackTrace -------------------------------------------------------------------".Indent(exceptionlevel));
				//ret = ret & vbCrLf & ReverseLines(ex.StackTrace)
				StackTrace sTrace = new StackTrace(ex, true);
				if (sTrace.GetFrames () != null) {
					try{
						for (int i = 0; i <= sTrace.GetFrames ().Length - 1; i++) {
							StackFrame sFrame = sTrace.GetFrame (i);
							if (sFrame.GetMethod () != null)
								sw.WriteLine(("   " + i + ".- " + sFrame.GetMethod ().DeclaringType.FullName + ", " + sFrame.GetMethod ().Module.Assembly.GetName ().Name + " (" + sFrame.GetMethod ().Module.Assembly.GetName ().Version.ToString () + ") " + sFrame.GetMethod ().Name + "(" + GetParameterTypes (sFrame.GetMethod ()) + ") " + sFrame.GetFileName () + " " + sFrame.GetFileLineNumber () + ":" + sFrame.GetFileColumnNumber ()).Indent(exceptionlevel));
							else
								sw.WriteLine(("   " + i + ".- " + sFrame.ToString ()).Indent(exceptionlevel));
						}
					}
					catch{
					}
				}
			}
			sw.WriteLine("-------------------------------------------------------------------".Indent(exceptionlevel));			
			if ((int)exceptionsVerbosityLevel > 1 && (ex.InnerException != null)) {
				sw.WriteLine(exceptionlevel == 1 ? ("InnerExceptions -------------------------------------------------------------------").Indent(exceptionlevel) : "");
				ExceptionParser(ex.InnerException, exceptionsVerbosityLevel, sw, exceptionlevel + 1);
			}

		}

		public static string ExceptionToString_(Exception ex, int innerExceptionlevel, ExceptionVerbosityLevels exceptionsVerbosityLevel)
		{
			string ret = (innerExceptionlevel == 1 ? " " : "\r\n   ") + ex.Message;
			ret = ret + (innerExceptionlevel == 1 ? "\r\n  " : " ") + "Exception";

			if (ex.StackTrace != null) {
				RegexOptions regOps = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;
				Regex regex = default(Regex);

				regex = new Regex ("\\s*en\\s(?<namespace>[^\\s]+)\\sen\\s(?<path>.+)\\:\\w+\\s(?<line>\\d+)", regOps);
				Match m = regex.Match (ex.StackTrace);

				if ((!string.IsNullOrEmpty (m.Groups ["namespace"].Value)
					&& !string.IsNullOrEmpty (m.Groups ["line"].Value)
					&& !string.IsNullOrEmpty (m.Groups ["path"].Value)))
					ret = ret + " in '" + m.Groups ["namespace"].Value + ":" + m.Groups ["line"].Value + " (" + m.Groups ["path"].Value + ")'";				

				if(ex.TargetSite!=null)
					ret = ret + " calling to function '" + ex.TargetSite.Name + "' in '" + ex.Source + "'";
			}

			if (((int)exceptionsVerbosityLevel > 1 && (ex.InnerException != null))) {
				ret = ret + (innerExceptionlevel == 1 ? "\r\n  - InnerExceptions -------------------------------------------------------------------" : "");
				ret = ret + ExceptionToString_(ex.InnerException, innerExceptionlevel + 1, exceptionsVerbosityLevel);
			}
			if (((int)exceptionsVerbosityLevel > 2 && innerExceptionlevel == 1)) {
				ret = ret + "\r\n  - StackTrace -------------------------------------------------------------------";
				//ret = ret & vbCrLf & ReverseLines(ex.StackTrace)
				StackTrace sTrace = new StackTrace(ex, true);
				if (sTrace.GetFrames () != null) {
					try{
						for (int i = 0; i <= sTrace.GetFrames ().Length - 1; i++) {
							StackFrame sFrame = sTrace.GetFrame (i);
							if (sFrame.GetMethod () != null)
								ret = ret + "\r\n   " + i + ".- " + sFrame.GetMethod ().DeclaringType.FullName + ", " + sFrame.GetMethod ().Module.Assembly.GetName ().Name + " (" + sFrame.GetMethod ().Module.Assembly.GetName ().Version.ToString () + ") " + sFrame.GetMethod ().Name + "(" + GetParameterTypes (sFrame.GetMethod ()) + ") " + sFrame.GetFileName () + " " + sFrame.GetFileLineNumber () + ":" + sFrame.GetFileColumnNumber ();
							else
								ret = ret + "\r\n   " + i + ".- " + sFrame.ToString ();
						}
					}
					catch{
					}
				}
			}
			return ret;
		}

		private static string GetParameterTypes(MethodBase method)
		{
			string ret = "";
			foreach (ParameterInfo p in method.GetParameters()) {
				ret = ret + ", " + (p.IsOptional ? "[" : "") + p.ParameterType.Name + " " + p.Name;
				if (((p.DefaultValue != null))) {
					ret = ret + "=" + p.DefaultValue.ToString();
				}
				ret = ret + (p.IsOptional ? "]" : "");
			}
			if (ret.Length > 0) {
				ret = ret.Substring(2);
			}

			return ret;
		}

		public static string AssemblyBuildName(Assembly assembly)
		{
			return assembly.GetName().Name;
		}

		public static string AssemblyBuildVersion(Assembly assembly)
		{
			return assembly.GetName().Version.ToString();
		}

		public static DateTime AssemblyBuildDate(Assembly assembly)
		{
			return System.IO.File.GetLastWriteTime(assembly.Location);
		}

		public static string AssemblyBuildDescription(Assembly assembly)
		{
			return ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;
		}

		public override string ToString()
		{
			return this.GetType().Name;
		}
	}
}