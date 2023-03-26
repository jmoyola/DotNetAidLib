using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.Upgrade
{
    [DataContract]
    public class UpgradeVersion : IComparable
    {
        public UpgradeVersion()
        {
            Major = 0;
            Minor = 0;
            Build = 0;
            Revision = 0;
        }

        public UpgradeVersion(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public UpgradeVersion(int major, int minor) : this(major, minor, 0, 0)
        {
        }

        public UpgradeVersion(int major, int minor, int build) : this(major, minor, build, 0)
        {
        }

        [DataMember(Order = 0)] private int Major { get; set; }

        [DataMember(Order = 1)] private int Minor { get; set; }

        [DataMember(Order = 2)] private int Build { get; set; }

        [DataMember(Order = 3)] private int Revision { get; set; }

        public int CompareTo(object obj)
        {
            var ret = -1;

            if (obj == null)
                return ret;

            if (typeof(UpgradeVersion).IsAssignableFrom(obj.GetType()))
            {
                var ot = (UpgradeVersion) obj;
                ret = Major - ot.Major;
                ret = ret == 0 ? Minor - ot.Minor : ret;
                ret = ret == 0 ? Build - ot.Build : ret;
                ret = ret == 0 ? Revision - ot.Revision : ret;
            }

            return ret;
        }

        public override string ToString()
        {
            var ret = "" + Major
                         + "." + Minor
                         + "." + Build
                         + "." + Revision;
            return ret;
        }

        public override bool Equals(object obj)
        {
            var ret = false;

            if (obj == null)
                return ret;

            if (typeof(UpgradeVersion).IsAssignableFrom(obj.GetType()))
            {
                var ot = (UpgradeVersion) obj;
                ret = Major == ot.Major && Minor == ot.Minor && Build == ot.Build && Revision == ot.Revision;
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

        public static UpgradeVersion Parse(string s)
        {
            UpgradeVersion ret = null;
            try
            {
                var arrS = s.Split('.');
                ret = new UpgradeVersion(int.Parse(arrS[0]), int.Parse(arrS[1]));
                if (arrS.Length > 2)
                    ret.Build = int.Parse(arrS[2]);
                if (arrS.Length > 3)
                    ret.Revision = int.Parse(arrS[3]);
                return ret;
            }
            catch
            {
                throw new UpgradeException("Error parsin '" + s + "' to UpgradeVersion");
            }
        }

        public static implicit operator Version(UpgradeVersion value)
        {
            return new Version(value.Major, value.Minor, value.Build, value.Revision);
        }

        public static implicit operator UpgradeVersion(Version value)
        {
            return new UpgradeVersion(value.Major, value.Minor, value.Build, value.Revision);
        }

        public static bool operator <(UpgradeVersion a, UpgradeVersion b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(UpgradeVersion a, UpgradeVersion b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator ==(UpgradeVersion a, UpgradeVersion b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return ReferenceEquals(a, b);
            return a.Equals(b);
        }

        public static bool operator !=(UpgradeVersion a, UpgradeVersion b)
        {
            return !(a == b);
        }
    }
}