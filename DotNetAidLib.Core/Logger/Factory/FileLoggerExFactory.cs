using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using DotNetAidLib.Core.Logger.Client;
using DotNetAidLib.Core.Logger.Core;
using DotNetAidLib.Core.Logger.Writers;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Xml;
using DotNetAidLib.Core.Files;


namespace DotNetAidLib.Core.Logger.Factory{
	public class FileLoggerExFactory
	{
		public static ILogger Instance(FileLoggerWriter fileLoggerWriter, IApplicationConfig applicationConfig)
		{

			ILogger ret;

			if ((LoggerEx.InstanceExists(fileLoggerWriter.LogFile.FullName))) {
				ret = LoggerEx.Instance(fileLoggerWriter.LogFile.FullName);
			} else {
				LoggerWriters loggerWriters = new LoggerWriters();
                loggerWriters.Add(DebugLoggerWriter.Instance());
                loggerWriters.Add(fileLoggerWriter);
				ret = LoggerEx.Instance(fileLoggerWriter.LogFile.FullName, loggerWriters, applicationConfig);
			}

			return ret;
		}

		public static ILogger Instance()
		{
			return Instance(FileLoggerWriter.Instance());
		}

		public static ILogger Instance(FileLoggerWriter fileLoggerWriter)
		{
			IApplicationConfig cfg = null;
			cfg = XmlApplicationConfigFactory.Instance (FileLocation.UserApplicationDataFolder);
			return Instance(fileLoggerWriter, cfg);
		}

		public static ILogger Instance(IApplicationConfig applicationConfig)
		{
			return Instance(FileLoggerWriter.Instance(),applicationConfig);
		}
	}
}