using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Plugins
{
	public class PlugIn<T>
	{
		private Type _PlugInType;

		private PluginInfoAttribute defaultPluginInfo = null;
		public PlugIn (Type plugInType)
		{
			Assert.NotNull(plugInType, nameof(plugInType));

			if(!typeof(T).IsAssignableFrom(plugInType))
				throw new ArgumentException ("'" + plugInType.Name + "' is not plugin of '" + typeof(T).Name + "'", nameof(plugInType));
			
			this._PlugInType = plugInType;
			defaultPluginInfo = new PluginInfoAttribute(_PlugInType.FullName);
		}

		public Type PlugInType { 
			get { return this._PlugInType;}
		}

		public T Instance() {
			return Instance(new Object[] { });
		}

		public T Instance (object [] constructorParameters)
		{
			Type [] constructorTypes = Type.GetTypeArray (constructorParameters);
			if (_PlugInType.GetConstructor (constructorTypes) == null)
				throw new PluginException("Type '" + _PlugInType.Name
				                          + "' don't have a constructor with ("
				                          + constructorTypes.Select(v=>v.Name).ToStringJoin(", ") + ") arguments." );
			return (T)Activator.CreateInstance (_PlugInType, constructorParameters);
		}

		public PluginInfoAttribute Info
		{
			get {
				PluginInfoAttribute ret = null;
				ret = (PluginInfoAttribute)_PlugInType.GetCustomAttributes (typeof (PluginInfoAttribute), true).FirstOrDefault ();
				if (ret == null)
					ret = defaultPluginInfo;
				return ret;
			}
		}

		public IEnumerable<PluginPropertyAttribute> Properties {
			get {
				return _PlugInType.GetCustomAttributes (typeof (PluginPropertyAttribute), true).Cast<PluginPropertyAttribute>();
			}
		}

        public IDictionary<String, Object> PropertiesKeyValues {
            get {
                return this.Properties.ToDictionary(k=>k.Key, v=>v.Value);
            }
        }

        public IEnumerable<ParameterInfo[]> Constructors
		{
			get {
				List<ParameterInfo []> ret = new List<ParameterInfo []> ();
				foreach (ConstructorInfo ci in _PlugInType.GetConstructors ())
					ret.Add (ci.GetParameters ());

				return ret;
			}
		}
	}
}
