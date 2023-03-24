using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Client;
using DotNetAidLib.Core.Network.Config.Route.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Network.Client.Core;

namespace DotNetAidLib.Core.Network.Config.TcpIp.Core
{
	public enum TCPInterfaceConfigType{
		Dhcp,
		Static,
		Manual,
		Loopback
	}

    public abstract class TCPInterfaceConfig
    {
        private bool _changed;
        private AddressFamily _AddressFamily;
        private IPAddress _IP;
        private IPAddress _NetMask;
        private IPAddress _Network;
        private IPAddress _Gateway;
        private IPAddress _Broadcast;
        private IList<IPAddress> _Dns = new List<IPAddress>();
        private IList<KeyValue<String, Object>> _Attributes = new List<KeyValue<String, Object>>();
        private bool _enabled;
        private bool _hotPlug;
        private TCPInterfaceConfigType _type;
        private string _name;

        public TCPInterfaceConfig(AddressFamily addressFamily)
        {
            if (!new AddressFamily[] { AddressFamily.InterNetwork, AddressFamily.InterNetworkV6 }.Any(v => v.Equals(addressFamily)))
                throw new NetworkingException("Only address family Internetwork (IPv4) and InternetworkV6 (IPv6) is allowed.");
            _AddressFamily = addressFamily;
        }

        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _name = value;
                this._changed = true;
            }
        }

        public bool Changed
        {
            get
            {
                return this._changed;
            }
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo
        {
            get
            {
                IEnumerable<NetworkInterfaceInfo> nis;
                nis = NetworkInterfaceInfo.GetAllNetworkInterfaces();
                return nis.FirstOrDefault(v => v.Name == this.Name);
            }
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                this._changed = true;
            }
        }
        public bool HotPlug
        {
            get
            {
                return _hotPlug;
            }
            set
            {
                _hotPlug = value;
                this._changed = true;
            }
        }
        public TCPInterfaceConfigType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                this._changed = true;
            }
        }

        public virtual AddressFamily AddressFamily
        {
            get { return _AddressFamily; }
        }

        public virtual IPAddress IP
        {
            get { return _IP; }
            set
            {
                if (value != null && !value.AddressFamily.Equals(this.AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _IP = value;
                this._changed = true;
            }
        }

        public virtual IPAddress NetMask
        {
            get { return _NetMask; }
            set
            {
                if (value != null && !value.AddressFamily.Equals(this.AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _NetMask = value;
                this._changed = true;
            }
        }

        public virtual IPAddress Network
        {
            get { return _Network; }
            set
            {
                if (value != null && !value.AddressFamily.Equals(this.AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _Network = value;
                this._changed = true;
            }
        }

        public virtual IPAddress Gateway
        {
            get { return _Gateway; }
            set
            {
                if (value != null && !value.AddressFamily.Equals(this.AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _Gateway = value;
            }
        }

        public virtual IPAddress Broadcast
        {
            get { return _Broadcast; }
            set
            {
                if (value != null && !value.AddressFamily.Equals(this.AddressFamily))
                    throw new NetworkingException("Address family is wrong.");
                _Broadcast = value;
                this._changed = true;
            }

        }

        public virtual IList<IPAddress> Dns
        {
            get
            {
                return _Dns;
            }
        }

        public IList<KeyValue<String, Object>> Attributes
        {
            get
            {
                return _Attributes;
            }
        }

        public static bool NetworkAvailable()
        {
            RouteEntry gatewayRoute = RouteTable.Instance().Entries
                .FirstOrDefault(v => v.DefaultGateway = true);
            if (gatewayRoute == null)
                throw new NetworkingException("The are not gateway set in configuration.");

            if (IPAddress.IsLoopback(gatewayRoute.To))
                return false;
            else
                return (ICMPClient.Ping(gatewayRoute.To.ToString()).Status.Equals(IPStatus.Success));
        }

        public static bool InternetAvailable()
        {

            RouteEntry gatewayRoute = RouteTable.Instance().Entries
                .FirstOrDefault(v => v.DefaultGateway = true);

            if (gatewayRoute == null)
                return false;

            try
            {
                IPAddress googleIPAddress = System.Net.Dns.GetHostEntry("www.google.com").AddressList[0];
                return ICMPClient.Ping(googleIPAddress.ToString()).Status.Equals(IPStatus.Success);
            }
            catch
            {
                return false;
            }

        }
    }
}

