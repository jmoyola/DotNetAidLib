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
    public abstract class DHCPInterfaceInfo
    {
        private NetworkInterfaceInfo networkInterfaceInfo;
        private AddressFamily addressFamily = AddressFamily.InterNetwork;

        public DHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
            : this(networkInterfaceInfo, AddressFamily.InterNetwork){ }
        
        public DHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo, AddressFamily family){
            Assert.NotNull(networkInterfaceInfo, nameof(networkInterfaceInfo));
            this.networkInterfaceInfo = networkInterfaceInfo;
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo {
            get { return this.networkInterfaceInfo; }
        }

        public AddressFamily AddressFamily {
            get { return this.addressFamily; }
        }

        public abstract IEnumerable<DHCPLease> Leases { get; }
        public abstract void Release();
        public abstract void Stop();
        public abstract void Start();
        public void Restart() {
            this.Stop();
            this.Start();
        }
        public override string ToString(){
            return this.Leases.Select(v => v.ToString()).ToStringJoin("; ");
        }

        public static DHCPInterfaceInfo Instance(NetworkInterfaceInfo networkInterfaceInfo) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            else
                return new PosixDHCPInterfaceInfo(networkInterfaceInfo);
        }
    }
}
