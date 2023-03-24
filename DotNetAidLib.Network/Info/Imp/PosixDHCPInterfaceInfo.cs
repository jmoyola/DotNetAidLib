using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Imp;


namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixDHCPInterfaceInfo:DHCPInterfaceInfo
    {
        private System.IO.FileInfo dhclientFileInfo = null;
        public PosixDHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
            : base(networkInterfaceInfo){
            dhclientFileInfo = EnvironmentHelper.SearchInPath("dhclient");
        }

        public PosixDHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo, AddressFamily addressFamily)
            :base(networkInterfaceInfo, addressFamily){
            Assert.Including<AddressFamily>(addressFamily, new AddressFamily[] { AddressFamily.InterNetwork, AddressFamily.InterNetworkV6 });

            dhclientFileInfo = EnvironmentHelper.SearchInPath("dhclient");
        }

        public override IEnumerable<DHCPLease> Leases {
            get {
                System.IO.FileInfo leases = new System.IO.FileInfo("/var/lib/dhcp/dhclient." + this.NetworkInterfaceInfo.Name + ".leases");
                throw new NotImplementedException();
            }
        }

        public override void Release()
        {
            dhclientFileInfo.CmdExecuteSync("-r " + this.NetworkInterfaceInfo.Name);
        }

        public override void Stop() {
            dhclientFileInfo.CmdExecuteSync("-x " + this.NetworkInterfaceInfo.Name);
        }

        public override void Start() {
            dhclientFileInfo.CmdExecuteSync("-w " +(this.AddressFamily==AddressFamily.InterNetworkV6?" -6": "-4") + this.NetworkInterfaceInfo.Name);
        }
    }
}
