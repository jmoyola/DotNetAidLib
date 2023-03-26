using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public class DHCPLease
    {
        public DHCPLease(NetworkInterfaceInfo networkInterfaceInfo)
            : this(networkInterfaceInfo, DateTime.Now, DateTime.Now, DateTime.Now, null)
        {
        }

        public DHCPLease(NetworkInterfaceInfo networkInterfaceInfo, DateTime renew, DateTime rebind, DateTime expire,
            IDictionary<string, object> options)
        {
            NetworkInterfaceInfo = networkInterfaceInfo;
            Renew = renew;
            Rebind = rebind;
            Expire = expire;
            Options = options;
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo { get; }

        public DateTime Renew { get; }
        public DateTime Rebind { get; }
        public DateTime Expire { get; }
        public IDictionary<string, object> Options { get; }

        public override string ToString()
        {
            return NetworkInterfaceInfo.Name + " (" + Options.ToStringJoin(", ") + "), Renew: " + Renew + ", Rebind: " +
                   Rebind + ", Expire: " + Expire;
        }
    }
}