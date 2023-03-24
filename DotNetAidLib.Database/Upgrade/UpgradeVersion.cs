using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.Upgrade
{
    [DataContract]
    public class UpgradeVersion:IComparable
    {

        public UpgradeVersion()
        {
            this.Major = 0;
            this.Minor = 0;
            this.Build = 0;
            this.Revision = 0;
        }

        public UpgradeVersion(int major, int minor, int build, int revision)
        {
            this.Major = major;
            this.Minor = minor;
            this.Build = build;
            this.Revision = revision;
        }

        public UpgradeVersion(int major, int minor) : this(major, minor, 0, 0) { }
        public UpgradeVersion(int major, int minor, int build) : this(major, minor, build, 0) { }

        [DataMember(Order = 0)]
        private int Major { get; set; }

        [DataMember(Order = 1)]
        private int Minor { get; set; }

        [DataMember(Order = 2)]
        private int Build { get; set; }

        [DataMember(Order = 3)]
        private int Revision { get; set; }

        public override string ToString()
        {
            String ret = "" + Major
                         + "." + Minor
                         + "." + Build
                         + "." + Revision;
            return ret;
        }

        public int CompareTo(Object obj) {
            int ret=-1;

            if (obj == null)
                return ret;

            if (typeof(UpgradeVersion).IsAssignableFrom(obj.GetType())){
                UpgradeVersion ot=(UpgradeVersion)obj;
                ret =this.Major-ot.Major;
                ret = (ret == 0 ? this.Minor - ot.Minor : ret);
                ret = (ret == 0 ? this.Build - ot.Build : ret);
                ret = (ret == 0 ? this.Revision - ot.Revision : ret);
            }

            return ret;
        }

        public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj == null)
                return ret;

            if (typeof(UpgradeVersion).IsAssignableFrom(obj.GetType())){
                UpgradeVersion ot=(UpgradeVersion)obj;
                ret = (this.Major == ot.Major) && (this.Minor == ot.Minor) && (this.Build == ot.Build) && (this.Revision == ot.Revision);
            }
            else
                ret = base.Equals(obj);

            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static UpgradeVersion Parse(String s) {
            UpgradeVersion ret = null;
            try
            {
                String[] arrS=s.Split('.');
                ret = new UpgradeVersion(Int32.Parse(arrS[0]), Int32.Parse(arrS[1]));
                if (arrS.Length > 2)
                    ret.Build = Int32.Parse(arrS[2]);
                if (arrS.Length > 3)
                    ret.Revision = Int32.Parse(arrS[3]);
                return ret;
            }
            catch {
                throw new UpgradeException("Error parsin '" + s + "' to UpgradeVersion");
            }
        }

        public static implicit operator Version(UpgradeVersion value)
        {
            return new Version(value.Major,value.Minor,value.Build,value.Revision);
        }

        public static implicit operator UpgradeVersion(Version value)
        {
            return new UpgradeVersion(value.Major, value.Minor, value.Build, value.Revision);
        }

		public static bool operator <(UpgradeVersion a, UpgradeVersion b)
		{
			return a.CompareTo(b)<0;
		}

		public static bool operator >(UpgradeVersion a, UpgradeVersion b)
		{
			return a.CompareTo(b)>0;
		}

		public static bool operator ==(UpgradeVersion a, UpgradeVersion b)
		{
			if (Object.ReferenceEquals (a, null) || Object.ReferenceEquals (b, null))
				return Object.ReferenceEquals (a, b);
			else
				return a.Equals (b);
		}

		public static bool operator !=(UpgradeVersion a, UpgradeVersion b)
		{
			return !(a==b);
		}
    }
}
