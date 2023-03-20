using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
	public enum KEY_MANAGEMENT_PROTOCOL {
		NONE,
		WPA_PSK,
		WPA_EAP,
		WPA2_PSK,
		WPA2_EAP
	}

	public enum EAP_METHOD
	{
		EAP_MD5,
		EAP_PEAP,
		EAP_TTLS,
		EAP_MSCHAPV2,
		EAP_OTP,
		EAP_GTC,
		EAP_TLS
	}

	public enum AUTH_ALGORITHM
	{
		OPEN,
		SHARED,
		LEAP
	}

	public enum PAIRWISE
	{
		CCMP,
		TKIP,
		NONE
	}

	public enum GROUP
	{
		CCMP,
		TKIP,
		WEP104,
		WEP40
	}

	public enum PROTOCOL
	{
		WPA,
		RSN
	}

	public abstract class WLanNetworkInfo:IWLanNetworkInfo
	{
		private int _Index;
		public WLanNetworkInfo (int index)
		{
			_Index = index;
		}

		public abstract byte[] BSSID { get; set; }
		public abstract String SSID { get; set; }
		public abstract bool HiddenSSID { get; set; }
		public abstract byte Priority { get; set; }
		public int Index {
			get { return _Index;}
		}

		public abstract Object GetProperty(String key);
		public abstract void SetProperty(String key, Object value);

		public abstract bool Enabled { get; set; }

        public abstract void Enable();
        public abstract void Disable();

		public abstract void Select();

		public abstract bool Connected { get;}

		public override string ToString ()
		{
			String ret = "(" + this.Index + ") BSSID=" + (this.BSSID==null?"":this.BSSID.ToHexadecimal(":")) + " SSID=" + this.SSID + " Enabled=" + this.Enabled + " Connected= " + this.Connected;

			return ret;
		}
	}
}

