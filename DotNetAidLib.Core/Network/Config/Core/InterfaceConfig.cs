using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Client;
using DotNetAidLib.Core.Network.Config.Route.Core;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.Core
{
    public class InterfaceConfig
    {
        private string _name;
        protected IList<InterfaceConfigAddress> addresses=new List<InterfaceConfigAddress>();

        public InterfaceConfig(String name)
        {
            this.Name = name;
        }

        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                Assert.NotNullOrEmpty(nameof(value), value);
                _name = value;
            }
        }

        public IList<InterfaceConfigAddress> Addresses
        {
            get { return this.addresses; }
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

