using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Core.Plugins
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class PluginPropertyAttribute : Attribute
	{
		private String _Key;
		private Object _Value;
		public PluginPropertyAttribute (String key, Object value)
		{
			if (String.IsNullOrEmpty(key))
				throw new ArgumentNullException ("key");
			
			this._Key = key;
			this._Value = value;
		}

		public string Key {
			get { return _Key;}
		}

		public Object Value {
			get { return _Value; }
		}
	}

}
