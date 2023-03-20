using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
	public interface IWLanClient{

        NetworkInterfaceInfo WlanInterface{ get;}
		IEnumerable<WLanNetworkScanInfo> ScanList();
		IEnumerable<IWLanNetworkInfo> NetworkList();
		IWLanNetworkInfo AddNetwork();
		void RemoveNetwork(IWLanNetworkInfo network);
		void SaveConfig();
	}
}
