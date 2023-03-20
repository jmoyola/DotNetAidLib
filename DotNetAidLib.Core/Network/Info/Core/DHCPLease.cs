using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Imp;


namespace DotNetAidLib.Core.Network.Info.Core
{
    public class DHCPLease {

        private NetworkInterfaceInfo networkInterfaceInfo;

        public DHCPLease(NetworkInterfaceInfo networkInterfaceInfo)
            : this(networkInterfaceInfo, DateTime.Now, DateTime.Now, DateTime.Now, null) { }
        
        public DHCPLease(NetworkInterfaceInfo networkInterfaceInfo, DateTime renew, DateTime rebind, DateTime expire, IDictionary<String, Object> options)
        {
            this.networkInterfaceInfo = networkInterfaceInfo;
            this.Renew = renew;
            this.Rebind = rebind;
            this.Expire = expire;
            this.Options = options;
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo
        {
            get { return this.networkInterfaceInfo; }
        }

        public DateTime Renew { get;}
        public DateTime Rebind { get; }
        public DateTime Expire { get; }
        public IDictionary<String, Object> Options { get; }
        public override string ToString(){
            return this.NetworkInterfaceInfo.Name+ " (" + this.Options.ToStringJoin(", ") + "), Renew: " + this.Renew.ToString()+ ", Rebind: " + this.Rebind.ToString()+ ", Expire: " + this.Expire.ToString();
        }
    }
}
