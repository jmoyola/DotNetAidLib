using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Network.Info.Imp;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public abstract class IPInterfaceInfo
    {
        public IPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
        {
            NetworkInterfaceInfo = networkInterfaceInfo;
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo { get; }

        public abstract IEnumerable<IPInfo> IPInformation { get; }
        public abstract void AddIPInfo(IPInfo ipInfo);
        public abstract void RemoveIPInfo(IPInfo ipInfo);

        public override string ToString()
        {
            return IPInformation.Select(v => v.ToString()).ToStringJoin(", ");
        }

        public static IPInterfaceInfo Instance(NetworkInterfaceInfo networkInterfaceInfo)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            return new PosixIPInterfaceInfo(networkInterfaceInfo);
        }
    }
}