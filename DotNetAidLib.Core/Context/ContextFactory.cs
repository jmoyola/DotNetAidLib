using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;

namespace DotNetAidLib.Core.Context{
	public class ContextFactory : Context
	{
        public static readonly string GLOBAL_CONTEXT = "_GLOBAL_";
		private static Dictionary<string, ContextFactory> m_Instances = new Dictionary<string, ContextFactory>();
		internal ContextFactory(String contextName)
            : base(contextName){}

        private static Object oInstance = new object();
        public static ContextFactory Instance(string contextName)
		{
            lock (oInstance)
            {
                if (m_Instances.ContainsKey(contextName)
                  && m_Instances[contextName].ExpirationTime.Ticks > 0
                  && DateTime.Now > (m_Instances[contextName].CreationTime.Add(m_Instances[contextName].ExpirationTime)))
                    m_Instances.Remove(contextName);

                if (!m_Instances.ContainsKey(contextName))
                    m_Instances.Add(contextName, new ContextFactory(contextName));

                return m_Instances[contextName];
            }
		}

		public static ContextFactory Instance()
		{
			return Instance(GLOBAL_CONTEXT);
		}

		public static void Dispose(string contextKey)
		{
			if ((m_Instances.ContainsKey(contextKey))) {
				m_Instances.Remove(contextKey);
			}
		}

		public static Context GetActiveContext(Component cont)
		{
			Context ret = null;

			if ((typeof(IContextSupport).IsAssignableFrom(cont.GetType()))) {
				ret = ((IContextSupport)cont).Context;
			} else {
				if (((cont.Container != null))) {
					ret = GetActiveContext((Component)cont.Container);
				}

			}

			return ret;
		}

		public static object GetAttribute(string key)
		{

			string[] parameters = key.Split(',');
			object ret = null;

			if ((parameters.Length == 1)) {
				ret = Instance()[key];
			} else if ((parameters.Length > 1)) {
				ret = Instance(parameters[0].Trim())[parameters[1].Trim()];
			}

			return ret;
		}

		public static void SetAttribute(string key, object value)
		{
			string[] parameters = key.Split(',');

			if ((parameters.Length == 1)) {
				Instance()[key] = value;
			} else if ((parameters.Length > 1)) {
				Instance(parameters[0].Trim())[parameters[1].Trim()] = value;
			}
		}
	}
}