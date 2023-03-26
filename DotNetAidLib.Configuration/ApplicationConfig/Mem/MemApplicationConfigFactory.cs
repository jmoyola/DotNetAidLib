using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Configuration.ApplicationConfig.Mem
{
    public class MemApplicationConfigFactory
    {
        private static readonly Dictionary<string, MemApplicationConfig> m_Instances =
            new Dictionary<string, MemApplicationConfig>();

        private static readonly object oInstance = new object();

        public static List<Type> KnownTypes { get; } = new List<Type>();

        public static MemApplicationConfig Instance()
        {
            return Instance("__DEFAULT__");
        }

        public static MemApplicationConfig Instance(string key)
        {
            lock (oInstance)
            {
                Assert.NotNull(key, nameof(key));

                MemApplicationConfig ret = null;

                // Si hay ya creada una instancia de ese archivo en la lista de instancias, se devuelve
                if (m_Instances.ContainsKey(key))
                {
                    ret = m_Instances[key];
                    // Si no existe una instancia creada, se crea y se añade a la lista de instancias
                }
                else
                {
                    ret = new MemApplicationConfig();
                    m_Instances.Add(key, ret);
                    ret.KnownTypes.AddRange(KnownTypes);
                }

                return ret;
            }
        }
    }
}