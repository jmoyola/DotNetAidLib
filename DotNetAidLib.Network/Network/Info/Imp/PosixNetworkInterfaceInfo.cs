using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixNetworkInterfaceInfo : NetworkInterfaceInfo
    {
        private readonly FileInfo ifconfigFile;
        private readonly FileInfo ifdownFile;
        private readonly FileInfo ifupFile;

        private PosixNetworkInterfaceInfo(string name)
            : base(name)
        {
            ifupFile = EnvironmentHelper.SearchInPath("ifup");
            ifdownFile = EnvironmentHelper.SearchInPath("ifdown");
            ifconfigFile = EnvironmentHelper.SearchInPath("ifconfig");
        }

        public override NetworkInterfaceType Type
        {
            get
            {
                try
                {
                    if (new DirectoryInfo("/sys/class/net/" + Name + "/wireless").Exists)
                        return NetworkInterfaceType.IEEE80211;
                    return (NetworkInterfaceType) int.Parse(GetInfo("type"));
                }
                catch
                {
                    return NetworkInterfaceType.UNKNOW;
                }
            }
        }

        public override NetworkInterfaceOperationState OperationalState
        {
            get
            {
                try
                {
                    return GetInfo("operstate").ToEnum<NetworkInterfaceOperationState>(true);
                }
                catch
                {
                    return NetworkInterfaceOperationState.UNKNOWN;
                }
            }
        }

        public override byte[] Address
        {
            get
            {
                try
                {
                    return GetInfo("address").HexToByteArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        public override byte[] Broadcast
        {
            get
            {
                try
                {
                    return GetInfo("broadcast").HexToByteArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        public override bool Dormant
        {
            get
            {
                try
                {
                    return GetInfo("dormant").Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        public override bool FullDuplex
        {
            get
            {
                try
                {
                    return GetInfo("duplex").Equals("full", StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        public override string Alias
        {
            get
            {
                try
                {
                    return GetInfo("ifalias");
                }
                catch
                {
                    return null;
                }
            }
        }

        public override int Index
        {
            get
            {
                try
                {
                    return int.Parse(GetInfo("ifindex"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override decimal MTU
        {
            get
            {
                try
                {
                    return decimal.Parse(GetInfo("mtu"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override long Speed
        {
            get
            {
                try
                {
                    return long.Parse(GetInfo("speed"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override bool Linked
        {
            get
            {
                try
                {
                    return GetInfo("carrier").Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        private string GetInfo(string path)
        {
            string ret = null;
            var fi = new FileInfo("/sys/class/net/" + Name + "/" + path);
            if (fi.Exists)
            {
                ret = fi.OpenText().ReadToEnd(true);
                ret = ret.Substring(0, ret.Length - 1);
            }

            return ret;
        }

        public override void Enable()
        {
            ifconfigFile.CmdExecuteSync(Name + " up");
            ifupFile.CmdExecuteSync("--ignore-errors " + Name);
        }

        public override void Disable()
        {
            ifdownFile.CmdExecuteSync("--ignore-errors " + Name);
            ifconfigFile.CmdExecuteSync(Name + " down");
        }

        public override bool IsEnabled()
        {
            return new FileInfo("/sys/class/net/" + Name + "/operstate")
                .OpenText()
                .ReadToEnd(true)
                .StartsWith("up", StringComparison.InvariantCultureIgnoreCase);
        }

        public new static IEnumerable<PosixNetworkInterfaceInfo> GetAllNetworkInterfaces()
        {
            var di = new DirectoryInfo("/sys/class/net");
            return di.GetDirectories().Select(v => new PosixNetworkInterfaceInfo(v.Name));
        }
    }
}