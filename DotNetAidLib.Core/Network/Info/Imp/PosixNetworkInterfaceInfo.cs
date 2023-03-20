using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using NetworkInterfaceType = DotNetAidLib.Core.Network.Info.Core.NetworkInterfaceType;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixNetworkInterfaceInfo:NetworkInterfaceInfo
    {
        private System.IO.FileInfo ifupFile;
        private System.IO.FileInfo ifdownFile;
        private System.IO.FileInfo ifconfigFile;

        private PosixNetworkInterfaceInfo(String name)
            :base(name){
            ifupFile = new System.IO.FileInfo(".")
                .FromPathEnvironmentVariable("ifup");
            ifdownFile = new System.IO.FileInfo(".")
                .FromPathEnvironmentVariable("ifdown");
            ifconfigFile = new System.IO.FileInfo(".")
                .FromPathEnvironmentVariable("ifconfig");
        }

        private String GetInfo(String path) {
            String ret = null;
            System.IO.FileInfo fi = new System.IO.FileInfo("/sys/class/net/" + this.Name + "/" + path);
            if (fi.Exists){
                ret = fi.OpenText().ReadToEnd(true);
                ret = ret.Substring(0, ret.Length - 1);
            }

            return ret;
        }

        public override NetworkInterfaceType Type {
            get {
                try{
                    if (new DirectoryInfo("/sys/class/net/" + this.Name + "/wireless").Exists)
                        return Core.NetworkInterfaceType.IEEE80211;
                    else
                        return (NetworkInterfaceType)Int32.Parse(GetInfo("type"));
                }
                catch {
                    return NetworkInterfaceType.UNKNOW;
                }
            }
        }

        public override NetworkInterfaceOperationState OperationalState{
            get{
                try{
                    return GetInfo("operstate").ToEnum<NetworkInterfaceOperationState>(true);
                }
                catch
                {
                    return NetworkInterfaceOperationState.UNKNOWN;
                }
            }
        }

        public override byte[] Address{
            get{
                try{
                    return GetInfo("address").HexToByteArray();
                }
                catch{
                    return null;
                }
            }
        }

        public override byte[] Broadcast{
            get{
                try{
                    return GetInfo("broadcast").HexToByteArray();
                }
                catch{
                    return null;
                }
            }
        }

        public override bool Dormant{
            get{
                try
                {
                    return GetInfo("dormant").Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                catch {
                    return false;
                }
            }
        }

        public override bool FullDuplex{
            get{
                try{
                    return GetInfo("duplex").Equals("full", StringComparison.InvariantCultureIgnoreCase);
                }
                catch{
                    return false;
                }
            }
        }

        public override String Alias{
            get{
                try{
                    return GetInfo("ifalias");
                }
                catch {
                    return null;
                }
            }
        }

        public override int Index
        {
            get{
                try{
                    return Int32.Parse(GetInfo("ifindex"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override decimal MTU{
            get{
                try{
                    return Decimal.Parse(GetInfo("mtu"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override long Speed{
            get{
                try{
                    return Int64.Parse(GetInfo("speed"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override bool Linked
        {
            get{
                try{
                    return GetInfo("carrier").Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        public override void Enable()
        {
            DotNetAidLib.Core.OperatingSystem.Core.OperatingSystem.Instance()
                           .AdminExecute(ifconfigFile, this.Name + " up");
            DotNetAidLib.Core.OperatingSystem.Core.OperatingSystem.Instance()
                           .AdminExecute(ifupFile, "--ignore-errors " + this.Name);
        }

        public override void Disable()
        {
            DotNetAidLib.Core.OperatingSystem.Core.OperatingSystem.Instance()
                           .AdminExecute(ifdownFile, "--ignore-errors " + this.Name);
            DotNetAidLib.Core.OperatingSystem.Core.OperatingSystem.Instance()
                            .AdminExecute(ifconfigFile, this.Name + " down");
        }

        public override bool IsEnabled()
        {
            return new FileInfo("/sys/class/net/" + this.Name + "/operstate")
                 .OpenText()
                .ReadToEnd(true)
                .StartsWith("up", StringComparison.InvariantCultureIgnoreCase);
        }

        public static new IEnumerable<PosixNetworkInterfaceInfo> GetAllNetworkInterfaces() {
            DirectoryInfo di = new DirectoryInfo("/sys/class/net");
            return di.GetDirectories().Select(v=>new PosixNetworkInterfaceInfo(v.Name));
        }
    }
}
