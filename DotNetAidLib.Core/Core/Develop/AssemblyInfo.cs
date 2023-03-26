using System;
using System.IO;
using System.Reflection;

namespace DotNetAidLib.Core.Develop
{
    public class AssemblyInfo
    {
        private static AssemblyInfo _NoInfoInstance;

        private AssemblyInfo()
        {
        }

        private AssemblyInfo(Assembly assembly) : this()
        {
            LoadFromGAC = assembly.GlobalAssemblyCache;
            FullName = assembly.GetName().Name;
            Version = assembly.GetName().Version.ToString();
            BuildDate = File.GetLastWriteTime(assembly.Location);
            Culture = assembly.GetName().CultureInfo.ToString();
            Architecture = assembly.GetName().ProcessorArchitecture.ToString();
            var da = (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(assembly,
                typeof(AssemblyDescriptionAttribute));
            Description = da == null ? "No description" : da.Description;
            Location = assembly.Location;
            CLRVersion = assembly.ImageRuntimeVersion;
        }

        public bool LoadFromGAC { get; set; }

        public string FullName { get; set; }

        public string Version { get; set; }

        public DateTime BuildDate { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string Culture { get; set; }

        public string Architecture { get; set; }

        public string CLRVersion { get; set; }

        public static AssemblyInfo NoInfo
        {
            get
            {
                if (_NoInfoInstance == null)
                {
                    _NoInfoInstance = new AssemblyInfo();
                    _NoInfoInstance.FullName = "";
                }

                return _NoInfoInstance;
            }
        }

        public override string ToString()
        {
            return FullName + "_" + Version + (LoadFromGAC ? "G" : "");
        }

        public override bool Equals(object obj)
        {
            var ret = false;

            if (typeof(AssemblyInfo).IsAssignableFrom(obj.GetType()))
            {
                var o = (AssemblyInfo) obj;
                if (o != null) ret = FullName == o.FullName && Version == o.Version;
            }
            else
            {
                ret = base.Equals(obj);
            }

            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static AssemblyInfo FromAssembly(Assembly assembly)
        {
            if (assembly == null)
                return NoInfo;
            return new AssemblyInfo(assembly);
        }
    }
}