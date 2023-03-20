using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Plugins
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class PluginInfoAttribute : Attribute
	{
		private string _Name;
		private string _Description;
		private int _PreferentOrder;

		public PluginInfoAttribute(String name)
            : this(name, null, 0) { }

		public PluginInfoAttribute(String name, String description)
			:this(name, description, 0) {}

		public PluginInfoAttribute (String name, String description, int preferentOrder)
		{
			Assert.NotNullOrEmpty( name, nameof(name));

			this._Name = name;
			this._Description = description;
			this._PreferentOrder = preferentOrder;
		}


		public string Name {
			get { return _Name;}
		}

		public string Description {
			get { return _Description; }
		}

		public int PreferentOrder{
			get{return _PreferentOrder;}
		}
	}

}
