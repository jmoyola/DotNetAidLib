using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Network.Client.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Network.Config.Route.Core;

namespace DotNetAidLib.Network.Config.TcpIp.Core
{
    public enum TCPInterfaceConfigType
    {
        Dhcp,
        Static,
        Manual,
        Loopback
    }

    public abstract class TCPInterfaceConfig
    {
        private IPAddress _Broadcast;
        private bool _enabled;
        private IPAddress _Gateway;
        private bool _hotPlug;
        private IPAddress _IP;
        private string _name;
        private IPAddress _NetMask;
        private IPAddress _Network;
        private TCPInterfaceConfigType _type;

        public TCPInterfaceConfig(AddressFamily addressFamily)
        {
            if (!new[] {AddressFamily.InterNetwork, AddressFamily.InterNetworkV6}.Any(v => v.Equals(addressFamily)))
                throw new NetworkingException(
                    "Only address family Internetwork (IPv4) and InternetworkV6 (IPv6) is allowed.");
            AddressFamily = addressFamily;
        }

        public string Name
        {
            get => _name;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _name = value;
                Changed = true;
            }
        }

        public bool Changed { get; private set; }

        public NetworkInterfaceInfo NetworkInterfaceInfo
        {
            get
            {
                IEnumerable<NetworkInterfaceInfo> nis;
                nis = NetworkInterfaceInfo.GetAllNetworkInterfaces();
                return nis.FirstOrDefault(v => v.Name == Name);
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                Changed = true;
            }
        }

        public bool HotPlug
        {
            get => _hotPlug;
            set
            {
                _hotPlug = value;
                Changed = true;
            }
        }

        public TCPInterfaceConfigType Type
        {
            get => _type;
            set
            {
                _type = value;
                Changed = true;
            }
        }

        public virtual AddressFamily AddressFamily { get; }

        public virtual IPAddress IP
        {
            get => _IP;
            set
            {
                if (value != null && !value.AddressFamily.Equals(AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _IP = value;
                Changed = true;
            }
        }

        public virtual IPAddress NetMask
        {
            get => _NetMask;
            set
            {
                if (value != null && !value.AddressFamily.Equals(AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _NetMask = value;
                Changed = true;
            }
        }

        public virtual IPAddress Network
        {
            get => _Network;
            set
            {
                if (value != null && !value.AddressFamily.Equals(AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _Network = value;
                Changed = true;
            }
        }

        public virtual IPAddress Gateway
        {
            get => _Gateway;
            set
            {
                if (value != null && !value.AddressFamily.Equals(AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _Gateway = value;
            }
        }

        public virtual IPAddress Broadcast
        {
            get => _Broadcast;
            set
            {
                if (value != null && !value.AddressFamily.Equals(AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _Broadcast = value;
                Changed = true;
            }
        }

        public virtual IList<IPAddress> Dns { get; } = new List<IPAddress>();

        public IList<KeyValue<string, object>> Attributes { get; } = new List<KeyValue<string, object>>();

        public static bool NetworkAvailable()
        {
            var gatewayRoute = RouteTable.Instance().Entries
                .FirstOrDefault(v => v.DefaultGateway = true);
            if (gatewayRoute == null)
                throw new NetworkingException("The are not gateway set in configuration.");

            if (IPAddress.IsLoopback(gatewayRoute.To))
                return false;
            return ICMPClient.Ping(gatewayRoute.To.ToString()).Status.Equals(IPStatus.Success);
        }

        public static bool InternetAvailable()
        {
            var gatewayRoute = RouteTable.Instance().Entries
                .FirstOrDefault(v => v.DefaultGateway = true);

            if (gatewayRoute == null)
                return false;

            try
            {
                var googleIPAddress = System.Net.Dns.GetHostEntry("www.google.com").AddressList[0];
                return ICMPClient.Ping(googleIPAddress.ToString()).Status.Equals(IPStatus.Success);
            }
            catch
            {
                return false;
            }
        }
    }
}