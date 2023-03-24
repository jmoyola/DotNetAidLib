using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Collections.Specialized;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.Writers{
	public abstract class NetLoggerWriter : LoggerWriter
	{

		private string _Server = null;

		private int _Port = 0;
		public NetLoggerWriter()
		{
			this.MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ERROR;
			this.MaxLogPriorityVerboseInRunTime = LogPriorityLevels._ERROR;
		}

		public NetLoggerWriter(string server, int port) : this()
		{
			_Server = server;
			_Port = port;
		}

		public string Server {
			get { return _Server; }
			set { _Server = value; }
		}

		public int Port {
			get { return _Port; }
			set { _Port = value; }
		}

		public override void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			base.InitConfiguration(configGroup);

			if ((configGroup.ConfigurationExist("Server"))) {
				_Server = configGroup.GetConfiguration<string>("Server").Value;
			} else {
				configGroup.AddConfiguration<string>("Server", _Server, true);
			}

			if ((configGroup.ConfigurationExist("Port"))) {
				_Port = configGroup.GetConfiguration<int>("Port").Value;
			} else {
				configGroup.AddConfiguration<int>("Port", _Port, true);
			}
		}

		public override void SaveConfiguration(IApplicationConfigGroup configGroup)
		{
			base.SaveConfiguration(configGroup);

			configGroup.AddConfiguration<string>("Server", _Server, true);

			configGroup.AddConfiguration<int>("Port", _Port, true);

		}


		~NetLoggerWriter()
		{
		}

		public override string ToString()
		{
			return this.GetType().Name + ": " + this.Server + ":" + this.Port;
		}

	}
}