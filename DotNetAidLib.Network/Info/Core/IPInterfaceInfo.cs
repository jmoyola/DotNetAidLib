using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Network.Info.Imp;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;


namespace DotNetAidLib.Core.Network.Info.Core
{
    public abstract class IPInterfaceInfo
    {
        private NetworkInterfaceInfo networkInterfaceInfo;
        public IPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo){
            this.networkInterfaceInfo = networkInterfaceInfo;
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo {
            get { return this.networkInterfaceInfo; }
        }

        public abstract IEnumerable<IPInfo> IPInformation { get; }
        public abstract void AddIPInfo(IPInfo ipInfo);
        public abstract void RemoveIPInfo(IPInfo ipInfo);

        public override string ToString(){
            return this.IPInformation.Select(v => v.ToString()).ToStringJoin(", ");
        }

        public static IPInterfaceInfo Instance(NetworkInterfaceInfo networkInterfaceInfo) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            else
                return new PosixIPInterfaceInfo(networkInterfaceInfo);
        }
    }
}
