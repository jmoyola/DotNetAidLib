using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Network.Info.Imp;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public abstract class DHCPInterfaceInfo
    {
        public DHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
            : this(networkInterfaceInfo, AddressFamily.InterNetwork)
        {
        }

        public DHCPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo, AddressFamily family)
        {
            Assert.NotNull(networkInterfaceInfo, nameof(networkInterfaceInfo));
            NetworkInterfaceInfo = networkInterfaceInfo;
        }

        public NetworkInterfaceInfo NetworkInterfaceInfo { get; }

        public AddressFamily AddressFamily { get; } = AddressFamily.InterNetwork;

        public abstract IEnumerable<DHCPLease> Leases { get; }
        public abstract void Release();
        public abstract void Stop();
        public abstract void Start();

        public void Restart()
        {
            Stop();
            Start();
        }

        public override string ToString()
        {
            return Leases.Select(v => v.ToString()).ToStringJoin("; ");
        }

        public static DHCPInterfaceInfo Instance(NetworkInterfaceInfo networkInterfaceInfo)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            return new PosixDHCPInterfaceInfo(networkInterfaceInfo);
        }
    }
}