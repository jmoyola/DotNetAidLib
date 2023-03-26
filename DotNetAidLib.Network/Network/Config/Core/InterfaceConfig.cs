using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Network.Client.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Network.Config.Route.Core;
using DotNetAidLib.Network.Config.TcpIp.Core;

namespace DotNetAidLib.Network.Config.Core
{
    public class InterfaceConfig
    {
        private string _name;
        protected IList<InterfaceConfigAddress> addresses = new List<InterfaceConfigAddress>();

        public InterfaceConfig(string name)
        {
            Name = name;
        }

        public string Name
        {
            get => _name;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _name = value;
            }
        }

        public IList<InterfaceConfigAddress> Addresses => addresses;

        public NetworkInterfaceInfo NetworkInterfaceInfo
        {
            get
            {
                IEnumerable<NetworkInterfaceInfo> nis;
                nis = NetworkInterfaceInfo.GetAllNetworkInterfaces();
                return nis.FirstOrDefault(v => v.Name == Name);
            }
        }

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
                var googleIPAddress = Dns.GetHostEntry("www.google.com").AddressList[0];
                return ICMPClient.Ping(googleIPAddress.ToString()).Status.Equals(IPStatus.Success);
            }
            catch
            {
                return false;
            }
        }
    }
}