using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Imp;


namespace DotNetAidLib.Core.Network.Info.Core
{
    public class IPInfo {
        public IPInfo(IPAddress address)
            : this(address, null, null) { }
        public IPInfo(IPAddress address, IPAddress netmask)
            :this(address, netmask, null){}
        public IPInfo(IPAddress address, int netmaskLength)
            : this(address, address.GetNetworkMask(netmaskLength), null) { }
        public IPInfo(IPAddress address, int netmaskLength, IPAddress broadcast)
            : this(address, address.GetNetworkMask(netmaskLength), broadcast) { }
        public IPInfo(IPAddress address,IPAddress netmask, IPAddress broadcast) {
            Assert.NotNull(address, nameof(address));
            if (netmask == null)
                netmask = address.GetDefaultNetmask();
            
            if(broadcast!=null && netmask==null)
                throw new Exception("You must to set netmask");

            if (broadcast != null && !address.IsInSameSubnet(broadcast, netmask))
                throw new Exception("Broadcast address is not in the same subnet.");

            this.Address = address;
            this.Netmask = netmask;
            this.Broadcast = broadcast;
        }

        public IPAddress Address { get;}
        public int NetmaskLength {
            get {
                if (this.Netmask == null)
                    return -1;
                else
                    return this.Netmask.ToNetworkMaskLength();
            }
        }
        public IPAddress Netmask { get;}
        public IPAddress Broadcast { get;}

        public override string ToString(){
            return this.Address.ToString() + (this.Netmask != null ? "/" + this.NetmaskLength : "") + (this.Broadcast != null ? " (" + this.Broadcast.ToString()+ ")" : "");
        }
    }
}
