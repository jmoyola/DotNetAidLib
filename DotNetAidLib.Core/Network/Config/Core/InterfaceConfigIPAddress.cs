using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;
using DotNetAidLib.Core.Network.Config.Route.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Network.Client;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.Core
{
    public abstract class InterfaceConfigIPAddress : InterfaceConfigAddress
    {
        public InterfaceConfigIPAddress(AddressFamily addressFamily)
            : this(addressFamily, ProvisioningType.Loopback, new DictionaryList<string, String>()){}

        public InterfaceConfigIPAddress(AddressFamily addressFamily, ProvisioningType type, DictionaryList<String, String> attributes)
                :base(addressFamily, type, attributes){

            if (!this.AddressFamily.IsAnyOf(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6))
                throw new NetworkingException("Only address family Internetwork (IPv4) and InternetworkV6 (IPv6) is allowed.");
        }

        public abstract IPAddress IP { get; set; }
        public abstract IPAddress NetMask { get; set; }
        public abstract IPAddress Network { get; set; }
        public abstract IPAddress Gateway { get; set; }
        public abstract IPAddress Broadcast { get; set; }
        public abstract IList<IPAddress> Dns { get; set; }
    }
}

