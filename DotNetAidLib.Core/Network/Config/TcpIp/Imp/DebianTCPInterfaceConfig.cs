using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.IO;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;


namespace DotNetAidLib.Core.Network.Config.TcpIp.Imp
{

	public class DebianTCPInterfaceConfig:TCPInterfaceConfig
	{


		public DebianTCPInterfaceConfig (AddressFamily addressFamily)
			:base(addressFamily)
		{

		}


	}
}

