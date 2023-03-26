using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DotNetAidLib.Network.Config.TcpIp.Imp;

namespace DotNetAidLib.Network.Config.TcpIp.Core
{
    public abstract class TCPNetworkingConfig : List<TCPInterfaceConfig>
    {
        public abstract void Save();
        public abstract void Load();
        public abstract void ApplyAll();
        public abstract void Apply(TCPInterfaceConfig tcpInterface);

        public static TCPNetworkingConfig Instance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotImplementedException();

            if (new FileInfo("/etc/debian_version").Exists)
                return DebianTCPNetworkingConfig.Instance();
            throw new NotImplementedException();
        }
    }
}