using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Logger.Core;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Logger.Writers{
	public class UDPLoggerWriter : NetLoggerWriter
	{

		public UDPLoggerWriter() : base()
		{
		}

		public UDPLoggerWriter(string server, int port) : base(server, port)
		{
		}

		public override void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			base.InitConfiguration(configGroup);
		}

		public override void WriteLog(LogEntry logEntry)
		{
			UdpClient cli = null;
			try {
				cli = new UdpClient();

				cli.Connect(this.Server, this.Port);
				byte[] aBuf = logEntry.ToXmlByteArray();
				cli.Send(aBuf, aBuf.Length);
				cli.Close();
			} catch (Exception ex) {
				throw new LoggerException("Error writing log to UDP log server", ex, logEntry);
			}
		}


		~UDPLoggerWriter()
		{
		}

	}
}