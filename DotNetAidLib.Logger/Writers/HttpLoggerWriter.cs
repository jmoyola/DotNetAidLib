using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Collections.Specialized;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using System.Linq;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.LogEntryInfo;

namespace DotNetAidLib.Logger.Writers{
	public class HttpLoggerWriter : NetLoggerWriter
	{

		public HttpLoggerWriter()
		{
			this.MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ERROR;
		}

		public HttpLoggerWriter(string server, int port) : base(server, port)
		{
		}

		public override void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			base.InitConfiguration(configGroup);
		}

		public override void WriteLog(LogEntry logEntry)
		{
			try {
				WebClient wc = new WebClient();
				NameValueCollection getAttributes = new NameValueCollection();
                ClientLogEntryInfo clientLogEntryInfo = (ClientLogEntryInfo)logEntry.LogEntryInformation.FirstOrDefault(v => v is ClientLogEntryInfo);

                if (clientLogEntryInfo != null)
                    getAttributes.Add("ClientInfo", clientLogEntryInfo.ClientInfo);
                else
                    getAttributes.Add("ClientInfo", "");

                getAttributes.Add("Instant", logEntry.Instant.ToString());
				getAttributes.Add("LogPriority", logEntry.LogPriority.ToString());
				getAttributes.Add("ProcessId", logEntry.ProcessId);
				NameValueCollection postAttributes = new NameValueCollection();
				postAttributes.Add("Message", logEntry.ToString());
				wc.QueryString = getAttributes;

				wc.UploadValues(new UriBuilder("http", this.Server, this.Port).Uri, "POST", postAttributes);
			} catch (Exception ex) {
				throw new LoggerException("Error writing log to http log server", ex, logEntry);
			}
		}


		~HttpLoggerWriter()
		{
		}
	}
}