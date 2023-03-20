using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Data.Common;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Ado{
	public class AdoApplicationConfigFactory
	{
		private static List<Type> _KnownTypes=new List<Type>();

		private static Dictionary<string, AdoApplicationConfig> m_Instances = new Dictionary<string, AdoApplicationConfig>();
		public static AdoApplicationConfig Instance(DbProviderFactory DbProvider, string connectionString)
		{
			return Instance(DbProvider, connectionString, ConfigUserLevel._APPLICATION_ALL_USERS);
		}

		public static AdoApplicationConfig Instance(DbProviderFactory DbProvider, string connectionString, ConfigUserLevel UserLevel)
		{
			return Instance(DbProvider, connectionString, UserLevel.ToString(), Environment.UserName);
		}

		public static AdoApplicationConfig Instance(DbProviderFactory DbProvider, string connectionString, string CustomUserLevel, string userName)
		{
			AdoApplicationConfig ret = null;

			string key = userName + "_" + CustomUserLevel;

			// Si hay ya creada una instancia de ese archivo en la lista de instancias, se devuelve
			if ((m_Instances.ContainsKey(key))) {
				ret = m_Instances[key];
				// Si no existe una instancia creada, se crea y se añade a la lista de instancias
			} else {
				ret = new AdoApplicationConfig(DbProvider, connectionString, CustomUserLevel);
				m_Instances.Add(key, ret);
			}

			return ret;
		}

		public static List<Type> KnownTypes{
			get{
				return _KnownTypes;
			}
		}
	}
}