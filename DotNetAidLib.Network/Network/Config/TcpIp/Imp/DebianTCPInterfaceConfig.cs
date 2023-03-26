using System.Net.Sockets;
using DotNetAidLib.Network.Config.TcpIp.Core;

namespace DotNetAidLib.Network.Config.TcpIp.Imp
{
    public class DebianTCPInterfaceConfig : TCPInterfaceConfig
    {
        public DebianTCPInterfaceConfig(AddressFamily addressFamily)
            : base(addressFamily)
        {
        }
    }
}