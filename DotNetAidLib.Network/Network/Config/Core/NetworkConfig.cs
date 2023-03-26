using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Network.Config.Imp;

namespace DotNetAidLib.Network.Config.Core
{
    public abstract class NetworkConfig : List<InterfaceConfig>
    {
        public abstract void Save();
        public abstract void Load();
        public abstract void ApplyAll();
        public abstract void Apply(InterfaceConfig networkInterface);

        public static NetworkConfig Instance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotImplementedException();

            if (new FileInfo("/etc/debian_version").Exists)
                return DebianNetworkConfig.Instance();
            throw new NotImplementedException();
        }
    }
}