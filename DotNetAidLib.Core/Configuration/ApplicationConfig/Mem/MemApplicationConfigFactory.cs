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
using System.Runtime.CompilerServices;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Mem{
	public class MemApplicationConfigFactory
	{
		private static List<Type> _KnownTypes=new List<Type>();

		private static Dictionary<String, MemApplicationConfig> m_Instances = new Dictionary<String, MemApplicationConfig>();

		public static MemApplicationConfig Instance()
		{
            return Instance("__DEFAULT__");
		}

        private static Object oInstance = new object();
        public static MemApplicationConfig Instance(String key)
		{
            lock (oInstance)
            {
                Assert.NotNull( key, nameof(key));

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

		public static List<Type> KnownTypes{
			get{
				return _KnownTypes;
			}
		}

	}
}