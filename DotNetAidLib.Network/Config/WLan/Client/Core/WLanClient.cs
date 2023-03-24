using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Config.WLan.Client.Imp;
using DotNetAidLib.Core.Network.Info.Core;
using NetworkInterfaceType = DotNetAidLib.Core.Network.Info.Core.NetworkInterfaceType;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
	public abstract class WLanClient:IWLanClient{
        private NetworkInterfaceInfo _WlanInterface;
        public WLanClient(NetworkInterfaceInfo wlanInterface){
			if(wlanInterface==null)
				throw new ArgumentNullException ("WlanInterface can't be null.");
            if(!wlanInterface.Type.Equals(NetworkInterfaceType.IEEE80211))
				throw new WLanClientException ("Interface '" + wlanInterface.Name + "' is not wireless compliant.");

			this._WlanInterface = wlanInterface;
		}

        public NetworkInterfaceInfo WlanInterface{
			get{ return _WlanInterface;}
		}

		public abstract IEnumerable<WLanNetworkScanInfo> ScanList ();
		public abstract IEnumerable<IWLanNetworkInfo> NetworkList();
		public abstract IWLanNetworkInfo AddNetwork();
		public abstract void RemoveNetwork(IWLanNetworkInfo network);
		public abstract void SaveConfig();

        public static IEnumerable<NetworkInterfaceInfo> Adapters(){
            return NetworkInterfaceInfo.GetAllNetworkInterfaces().Where(
                v=>v.Type.Equals(NetworkInterfaceType.IEEE80211));
		}

        public static IWLanClient Instance(NetworkInterfaceInfo wlanInterface){
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				throw new NotImplementedException ();
			else {
				FileInfo f = EnvironmentHelper.SearchInPath("wpa_supplicant");
				if (f != null)
					return new WpaSupplicantClient (wlanInterface);

				throw new WLanClientException("wpa_supplicant program is missing.");
			}
		}
	}
}
