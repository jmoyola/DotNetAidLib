using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Configuration.ApplicationConfig.Xml
{
    public class XmlApplicationConfigFactory
    {
        private static readonly Dictionary<string, XmlApplicationConfig> m_Instances =
            new Dictionary<string, XmlApplicationConfig>();

        private static readonly object oInstance = new object();

        public static List<Type> KnownTypes { get; } = new List<Type>();

        public static XmlApplicationConfig Instance()
        {
            return Instance(FileLocation.UserConfigurationDataFolder);
        }

        public static XmlApplicationConfig Instance(FileLocation location)
        {
            var fileName = Path.GetFileNameWithoutExtension(Helper.GetEntryAssembly().GetName().Name) + ".cfg";
            return Instance(location, fileName);
        }

        public static XmlApplicationConfig Instance(FileLocation location, string fileName)
        {
            var file = new FileInfo(FileLocations.GetLocation(location).FullName
                                    + Path.DirectorySeparatorChar + fileName);

            return Instance(file);
        }

        public static XmlApplicationConfig Instance(FileInfo configFile)
        {
            lock (oInstance)
            {
                Assert.NotNull(configFile, nameof(configFile));

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
    }
}