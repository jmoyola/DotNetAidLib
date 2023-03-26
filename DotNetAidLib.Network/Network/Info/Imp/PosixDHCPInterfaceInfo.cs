using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixDHCPInterfaceInfo : DHCPInterfaceInfo
    {
        private readonly FileInfo dhclientFileInfo;

        public PosixDHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
            : base(networkInterfaceInfo)
        {
            dhclientFileInfo = EnvironmentHelper.SearchInPath("dhclient");
        }

        public PosixDHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo, AddressFamily addressFamily)
            : base(networkInterfaceInfo, addressFamily)
        {
            Assert.Including(addressFamily, new[] {AddressFamily.InterNetwork, AddressFamily.InterNetworkV6});

            dhclientFileInfo = EnvironmentHelper.SearchInPath("dhclient");
        }

        public override IEnumerable<DHCPLease> Leases
        {
            get
            {
                var leases = new FileInfo("/var/lib/dhcp/dhclient." + NetworkInterfaceInfo.Name + ".leases");
                throw new NotImplementedException();
            }
        }

        public override void Release()
        {
            dhclientFileInfo.CmdExecuteSync("-r " + NetworkInterfaceInfo.Name);
        }

        public override void Stop()
        {
            dhclientFileInfo.CmdExecuteSync("-x " + NetworkInterfaceInfo.Name);
        }

        public override void Start()
        {
            dhclientFileInfo.CmdExecuteSync("-w " + (AddressFamily == AddressFamily.InterNetworkV6 ? " -6" : "-4") +
                                            NetworkInterfaceInfo.Name);
        }
    }
}