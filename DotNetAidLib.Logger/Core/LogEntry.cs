using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using System.Net;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Logger.Client;

namespace DotNetAidLib.Logger.Core{
	public class LogEntry : ICloneable
	{
    
		private DateTime _Instant;
		private string _Application = "";
		private string _Message;
        private LoggerEx _LoggerEx=null;
		private LogPriorityLevels _LogPriority;
		private AssemblyInfo _CallAssembly = AssemblyInfo.NoInfo;
		private AssemblyInfo _AppAssembly = AssemblyInfo.NoInfo;
		private string _ProcessId;
        private IList<ILogEntryInfo> _LogEntryInformation;

        public LogEntry(LoggerEx loggerEx, DateTime instant, string message, LogPriorityLevels logPriority, Assembly callAssembly, Assembly appAssembly, string processId, IList<ILogEntryInfo> logEntryInformation)
            : this(loggerEx, instant, message, logPriority, AssemblyInfo.FromAssembly (callAssembly), AssemblyInfo.FromAssembly (appAssembly), processId,AssemblyInfo.FromAssembly (appAssembly).FullName, logEntryInformation)
		{
		}

        public LogEntry(LoggerEx loggerEx, DateTime instant, string message, LogPriorityLevels logPriority, AssemblyInfo callAssembly, AssemblyInfo appAssembly, string processId, string application, IList<ILogEntryInfo> logEntryInformation)
		{
            _LoggerEx = loggerEx;

			_Instant = instant;
			_Message = message;
			_LogPriority = logPriority;
			_CallAssembly = callAssembly;
			_AppAssembly = appAssembly;
			_ProcessId = processId;
			_Application = application;
            _LogEntryInformation = logEntryInformation;
		}

        public LoggerEx Logger {
            get { return _LoggerEx; }
        }

        public IList<ILogEntryInfo> LogEntryInformation {
            get { return _LogEntryInformation; }
        }

		public DateTime Instant {
			get { return _Instant; }
		}

		public string Application {
			get { return _Application; }
		}

		public string Message {
			get { return _Message; }
            set { _Message=value; }
		}

		public LogPriorityLevels LogPriority {
			get { return _LogPriority; }
		}

		public AssemblyInfo CallAssembly {
			get { return _CallAssembly; }
		}

		public AssemblyInfo AppAssembly {
			get { return _AppAssembly; }
		}

		public string ProcessId {
			get { return _ProcessId; }
		}

		private string ToStringHeader()
		{
			System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess ();
            string ret = this.Instant.ToString("yyyyMMdd\"H\"HHmmss\"'\"fff")
                + " " + this.LogPriority.ToString().Substring(1, 1)
                             + " [" + (_LogEntryInformation == null ? "" : _LogEntryInformation.Select(v=>Helper.TryFunc(()=>v.GetInfo(this),"!E")).ToStringJoin(";")) + "]";
			return ret;
		}

		public override string ToString()
		{
			return this.ToStringHeader() + " " + this.Message;
		}

		public void ToXml(Stream outStream)
		{
			LogEntry.ToXml(outStream, this);
		}

		public byte[] ToXmlByteArray()
		{
			return LogEntry.ToXmlByteArray(this);
		}

		public static void ToXml(Stream outStream, LogEntry value)
		{
			DataContractSerializer ser = new DataContractSerializer(value.GetType());
			ser.WriteObject(outStream, value);
		}

		public static byte[] ToXmlByteArray(LogEntry value)
		{
			byte[] ret = null;

			MemoryStream buf = new MemoryStream();
			ToXml(buf, value);
			ret = buf.ToArray();
			buf.Close();

			return ret;
		}

		public static LogEntry FromXml(Stream inStream)
		{
			DataContractSerializer ser = new DataContractSerializer(typeof(LogEntry));
			return (LogEntry)ser.ReadObject(inStream);
		}

		public static LogEntry FromXmlByteArray(byte[] buffer)
		{
			LogEntry ret = null;

			MemoryStream bufS = new MemoryStream(buffer);
			bufS.Seek(0, SeekOrigin.Begin);
			ret = FromXml(bufS);
			bufS.Close();
			return ret;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}