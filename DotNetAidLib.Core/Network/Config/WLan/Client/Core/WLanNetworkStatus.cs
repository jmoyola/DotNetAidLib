using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
	public enum WLanNetworkStatus{
		DISCONNECTED,
		INTERFACE_DISABLED,
		INACTIVE,
		SCANNING,
		AUTHENTICATING,
		ASSOCIATING,
		ASSOCIATED,
		FOUR_WAY_HANDSHAKE,
		GROUP_HANDSHAKE,
		CONNECTED
	}
}
