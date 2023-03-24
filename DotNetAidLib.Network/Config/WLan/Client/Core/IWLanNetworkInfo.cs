using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
	public interface IWLanNetworkInfo
	{
		int Index{get;}
		byte Priority { get; set;}
		byte[] BSSID { get; set; }
		String SSID { get; set; }
		Object GetProperty(String key);
		void SetProperty(String key, Object value);
		bool Enabled { get; set; }
        void Enable();
        void Disable();
		bool Connected { get; }
		void Select();
	}
}

