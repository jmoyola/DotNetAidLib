using System;
using System.Linq;
using System.Collections.Generic;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
	public class WLanNetworkScanInfo
	{
		private byte[] _BSSID;
		private String _SSID;
		private int _Frequency;
		private int _Signal;
		private IEnumerable<String> _Capabilities;
		public WLanNetworkScanInfo (byte[] BSSID, String SSID, int Frequency, int Signal, IEnumerable<String> Capabilities)
		{
			_BSSID = BSSID;
			_SSID = SSID;
			_Frequency = Frequency;
			_Signal = Signal;
			_Capabilities = Capabilities;
		}

		public byte[] BSSID {
			get { return _BSSID;}
		}
		public String SSID
		{
			get { return _SSID; }
		}

		public int Frequency {
			get { return _Frequency;}
		}

		public int Signal {
			get { return _Signal;}
		}

		public int Quality
		{
			get { return 2 * (this.Signal + 100); }
		}

		public IEnumerable<String> Capabilities{
			get { return _Capabilities;}
		}

		public override string ToString ()
		{
			String ret = "BSSID=" + (this.BSSID == null ? "" : this.BSSID.ToHexadecimal(":")) + "\nSSID=" + this.SSID+ "\nFrequency=" + this.Frequency+ "\nSignal=" + this.Signal+ "\nQuality=" + this.Quality+ "\nCapabilities=" + this.Capabilities.ToStringJoin(", ");

			return ret;
		}
        
        public int SignalPercent {
            get {
                try
                {
                    return IEEE80211InterfaceInfo.SignalPercentOfdB(this.Signal);
                }
                catch {
                    return -1;
                }
            } 
        }
	}
}

