using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace DotNetAidLib.Core.Logger.Core{
	public class AssemblyInfo
	{
		private bool _LoadFromGAC;
		private string _FullName;
		private string _Version;
		private DateTime _BuildDate;
		private string _Description;
		private string _Location;
		private string _Culture;
		private string _Architecture;

		private string _CLRVersion;

		private static AssemblyInfo _NoInfoInstance = null;

		private AssemblyInfo()
		{
		}

		private AssemblyInfo(Assembly assembly):this()
		{
			_LoadFromGAC = assembly.GlobalAssemblyCache;
			_FullName = assembly.GetName().Name;
			_Version = assembly.GetName().Version.ToString();
			_BuildDate = System.IO.File.GetLastWriteTime(assembly.Location);
			_Culture = assembly.GetName().CultureInfo.ToString();
			_Architecture = assembly.GetName().ProcessorArchitecture.ToString();
			AssemblyDescriptionAttribute da = (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
			_Description = da==null?"No description":da.Description;
			_Location = assembly.Location;
			_CLRVersion = assembly.ImageRuntimeVersion;
		}

		public bool LoadFromGAC {
			get { return _LoadFromGAC; }
			set { _LoadFromGAC = value; }
		}

		public string FullName {
			get { return _FullName; }
			set { _FullName = value; }
		}
		public string Version {
			get { return _Version; }
			set { _Version = value; }
		}
		public DateTime BuildDate {
			get { return _BuildDate; }
			set { _BuildDate = value; }
		}
		public string Description {
			get { return _Description; }
			set { _Description = value; }
		}
		public string Location {
			get { return _Location; }
			set { _Location = value; }
		}
		public string Culture {
			get { return _Culture; }
			set { _Culture = value; }
		}
		public string Architecture {
			get { return _Architecture; }
			set { _Architecture = value; }
		}
		public string CLRVersion {
			get { return _CLRVersion; }
			set { _CLRVersion = value; }
		}

		public override string ToString()
		{
			return this.FullName + "_" + this.Version + (this.LoadFromGAC ? "G" : "");
		}

		public override bool Equals(object obj)
		{
			bool ret = false;

			if ((typeof(AssemblyInfo).IsAssignableFrom(obj.GetType()))) {
				AssemblyInfo o = (AssemblyInfo)obj;
				if (((o != null))) {
					ret = this.FullName == o.FullName && this.Version == o.Version;
				}
			} else {
				ret = base.Equals(obj);
			}
			return ret;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static AssemblyInfo NoInfo
		{
			get{
				if ((_NoInfoInstance == null)) {
					_NoInfoInstance = new AssemblyInfo();
					_NoInfoInstance.FullName = "";
				}
				return _NoInfoInstance;
			}
		}

		public static AssemblyInfo FromAssembly(Assembly assembly){
			if (assembly == null)
				return NoInfo;
			else
				return new AssemblyInfo (assembly);
		}
	}
}