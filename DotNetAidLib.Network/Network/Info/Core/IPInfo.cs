using System;
using System.Net;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public class IPInfo
    {
        public IPInfo(IPAddress address)
            : this(address, null, null)
        {
        }

        public IPInfo(IPAddress address, IPAddress netmask)
            : this(address, netmask, null)
        {
        }

        public IPInfo(IPAddress address, int netmaskLength)
            : this(address, address.GetNetworkMask(netmaskLength), null)
        {
        }

        public IPInfo(IPAddress address, int netmaskLength, IPAddress broadcast)
            : this(address, address.GetNetworkMask(netmaskLength), broadcast)
        {
        }

        public IPInfo(IPAddress address, IPAddress netmask, IPAddress broadcast)
        {
            Assert.NotNull(address, nameof(address));
            if (netmask == null)
                netmask = address.GetDefaultNetmask();

            if (broadcast != null && netmask == null)
                throw new Exception("You must to set netmask");

            if (broadcast != null && !address.IsInSameSubnet(broadcast, netmask))
                throw new Exception("Broadcast address is not in the same subnet.");

            Address = address;
            Netmask = netmask;
            Broadcast = broadcast;
        }

        public IPAddress Address { get; }

        public int NetmaskLength
        {
            get
            {
                if (Netmask == null)
                    return -1;
                return Netmask.ToNetworkMaskLength();
            }
        }

        public IPAddress Netmask { get; }
        public IPAddress Broadcast { get; }

        public override string ToString()
        {
            return Address + (Netmask != null ? "/" + NetmaskLength : "") +
                   (Broadcast != null ? " (" + Broadcast + ")" : "");
        }
    }
}