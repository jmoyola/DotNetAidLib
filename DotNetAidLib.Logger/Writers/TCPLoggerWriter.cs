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
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.Writers{
	public class TCPLoggerWriter : NetLoggerWriter
	{

		public TCPLoggerWriter() : base()
		{
		}

		public TCPLoggerWriter(string server, int port) : base(server, port)
		{
		}

		public override void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			base.InitConfiguration(configGroup);
		}

		private static object oWriteLog = new object();
		public override void WriteLog(LogEntry logEntry)
		{
			lock (oWriteLog) {
				TcpClient cli = null;
				try {
					cli = new TcpClient();
					cli.Connect(this.Server, this.Port);
					Stream cliSt = cli.GetStream();

					logEntry.ToXml(cliSt);
					cliSt.WriteByte(4);
					// Escribimos la marca de eot

					cliSt.Flush();
					cliSt.Close();

				} catch (Exception ex) {
					throw new LoggerException("Error writing log to TCP log server", ex, logEntry);
				} finally {
					try {
						cli.Close();
					} catch {
					}
				}
			}
		}


		~TCPLoggerWriter()
		{
		}

	}
}