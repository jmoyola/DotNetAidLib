using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Network.Config.TcpIp.Imp;

namespace DotNetAidLib.Core.Network.Config.TcpIp.Core
{
	public abstract class TCPNetworkingConfig:List<TCPInterfaceConfig>
	{

		public abstract void Save();
		public abstract void Load();
		public abstract void ApplyAll();
		public abstract void Apply(TCPInterfaceConfig tcpInterface);

		public static TCPNetworkingConfig Instance(){
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            else
            {
                if(new FileInfo("/etc/debian_version").Exists)
                    return DebianTCPNetworkingConfig.Instance();
                else
                    throw new NotImplementedException();
            }
		}
	}
}

