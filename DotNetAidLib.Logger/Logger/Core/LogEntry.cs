using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Logger.Core
{
    public class LogEntry : ICloneable
    {
        public LogEntry(Client.Logger logger, DateTime instant, string message, LogPriorityLevels logPriority,
            Assembly callAssembly, Assembly appAssembly, string processId, IList<ILogEntryInfo> logEntryInformation)
            : this(logger, instant, message, logPriority, AssemblyInfo.FromAssembly(callAssembly),
                AssemblyInfo.FromAssembly(appAssembly), processId, AssemblyInfo.FromAssembly(appAssembly).FullName,
                logEntryInformation)
        {
        }

        public LogEntry(Client.Logger logger, DateTime instant, string message, LogPriorityLevels logPriority,
            AssemblyInfo callAssembly, AssemblyInfo appAssembly, string processId, string application,
            IList<ILogEntryInfo> logEntryInformation)
        {
            Logger = logger;

            Instant = instant;
            Message = message;
            LogPriority = logPriority;
            CallAssembly = callAssembly;
            AppAssembly = appAssembly;
            ProcessId = processId;
            Application = application;
            LogEntryInformation = logEntryInformation;
        }

        public Client.Logger Logger { get; }

        public IList<ILogEntryInfo> LogEntryInformation { get; }

        public DateTime Instant { get; }

        public string Application { get; } = "";

        public string Message { get; set; }

        public LogPriorityLevels LogPriority { get; }

        public AssemblyInfo CallAssembly { get; } = AssemblyInfo.NoInfo;

        public AssemblyInfo AppAssembly { get; } = AssemblyInfo.NoInfo;

        public string ProcessId { get; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        private string ToStringHeader()
        {
            var process = Process.GetCurrentProcess();
            var ret = Instant.ToString("yyyyMMdd\"H\"HHmmss\"'\"fff")
                      + " " + LogPriority.ToString().Substring(1, 1)
                      + " [" + (LogEntryInformation == null
                          ? ""
                          : LogEntryInformation.Select(v => Helper.TryFunc(() => v.GetInfo(this), "!E"))
                              .ToStringJoin(";")) + "]";
            return ret;
        }

        public override string ToString()
        {
            return ToStringHeader() + " " + Message;
        }

        public void ToXml(Stream outStream)
        {
            ToXml(outStream, this);
        }

        public byte[] ToXmlByteArray()
        {
            return ToXmlByteArray(this);
        }

        public static void ToXml(Stream outStream, LogEntry value)
        {
            var ser = new DataContractSerializer(value.GetType());
            ser.WriteObject(outStream, value);
        }

        public static byte[] ToXmlByteArray(LogEntry value)
        {
            byte[] ret = null;

            var buf = new MemoryStream();
            ToXml(buf, value);
            ret = buf.ToArray();
            buf.Close();

            return ret;
        }

        public static LogEntry FromXml(Stream inStream)
        {
            var ser = new DataContractSerializer(typeof(LogEntry));
            return (LogEntry) ser.ReadObject(inStream);
        }

        public static LogEntry FromXmlByteArray(byte[] buffer)
        {
            LogEntry ret = null;

            var bufS = new MemoryStream(buffer);
            bufS.Seek(0, SeekOrigin.Begin);
            ret = FromXml(bufS);
            bufS.Close();
            return ret;
        }
    }
}