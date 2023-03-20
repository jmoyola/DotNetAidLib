using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Configuration;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Xml{
	public class XmlApplicationConfigFactory
	{
		private static List<Type> _KnownTypes=new List<Type>();

		private static Dictionary<String, XmlApplicationConfig> m_Instances = new Dictionary<String, XmlApplicationConfig>();

		public static XmlApplicationConfig Instance()
		{
            return Instance(FileLocation.UserConfigurationDataFolder);
		}

		public static XmlApplicationConfig Instance(FileLocation location)
		{
            String fileName = Path.GetFileNameWithoutExtension(Helper.GetEntryAssembly().GetName().Name) + ".cfg";
            return Instance(location, fileName);
		}

        public static XmlApplicationConfig Instance(FileLocation location, String fileName)
		{

            System.IO.FileInfo file = new System.IO.FileInfo(FileLocations.GetLocation(location).FullName
                                        + Path.DirectorySeparatorChar + fileName);
                                         
            return Instance(file);
		}

        private static Object oInstance = new object();
        public static XmlApplicationConfig Instance(System.IO.FileInfo configFile)
		{
            lock (oInstance)
            {
                Assert.NotNull( configFile, nameof(configFile));

                XmlApplicationConfig ret = null;

                // Si hay ya creada una instancia de ese archivo en la lista de instancias, se devuelve
                if (m_Instances.ContainsKey(configFile.FullName))
                {
                    ret = m_Instances[configFile.FullName];
                    // Si no existe una instancia creada, se crea y se añade a la lista de instancias
                }
                else
                {
                    ret = new XmlApplicationConfig(configFile);
                    m_Instances.Add(configFile.FullName, ret);
                }

                return ret;
            }
		}

		public static List<Type> KnownTypes{
			get{
				return _KnownTypes;
			}
		}

	}
}