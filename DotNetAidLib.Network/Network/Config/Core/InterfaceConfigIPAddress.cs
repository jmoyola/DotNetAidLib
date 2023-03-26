using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Network.Config.TcpIp.Core;

namespace DotNetAidLib.Network.Config.Core
{
    public abstract class InterfaceConfigIPAddress : InterfaceConfigAddress
    {
        public InterfaceConfigIPAddress(AddressFamily addressFamily)
            : this(addressFamily, ProvisioningType.Loopback, new DictionaryList<string, string>())
        {
        }

        public InterfaceConfigIPAddress(AddressFamily addressFamily, ProvisioningType type,
            DictionaryList<string, string> attributes)
            : base(addressFamily, type, attributes)
        {
            if (!AddressFamily.IsAnyOf(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6))
                throw new NetworkingException(
                    "Only address family Internetwork (IPv4) and InternetworkV6 (IPv6) is allowed.");
        }

        public abstract IPAddress IP { get; set; }
        public abstract IPAddress NetMask { get; set; }
        public abstract IPAddress Network { get; set; }
        public abstract IPAddress Gateway { get; set; }
        public abstract IPAddress Broadcast { get; set; }
        public abstract IList<IPAddress> Dns { get; set; }
    }
}