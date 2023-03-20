using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using DotNetAidLib.Core.Network.Config.WLan.Client.Core;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Imp
{
	public class WpaSupplicantNetworkInfo:WLanNetworkInfo
	{
		private WpaSupplicantClient _WpaSupplicantClient;

		public WpaSupplicantNetworkInfo(WpaSupplicantClient wpaSupplicantClient, int Index)
			: base(Index)
		{
			this._WpaSupplicantClient = wpaSupplicantClient;
		}

		public override byte[] BSSID {
			get { return (byte[])this.GetProperty("bssid");}
			set { this.SetProperty("bssid", value);}
		}

		public override String SSID {
			get { return (String)this.GetProperty("ssid"); }
			set { this.SetProperty("ssid", value); }
		}

		public override bool HiddenSSID
		{
			get { return (bool)this.GetProperty("scan_ssid"); }
			set { this.SetProperty("scan_ssid", value); }
		}

		public override byte Priority
		{
			get { return (byte)this.GetProperty("priority"); }
			set { this.SetProperty("priority", value); }
		}

		public override Object GetProperty(String key) {
			Object ret=null;

			String result;

			result = _WpaSupplicantClient.WpaCliCmd("get_network " + this.Index + " " + key);

			if (result.Equals("FAIL"))
				ret = null;
			else if (String.IsNullOrEmpty(result))
				ret = null;
			else if (result.RegexIsMatch("^\"([^\"]+)\"$")) // Si el texto está entre comillas
				ret = result.RegexGroupsMatches("^\"([^\"]+)\"$")[1];
			else if (result.RegexIsMatch("^([0-9a-fA-F][0-9a-fA-F]:){5}[0-9a-fA-F][0-9a-fA-F]$"))
				ret = result.Replace(":", "").HexToByteArray();
			else if (result.RegexIsMatch(@"^[01]$"))
				ret = result.Equals("1", StringComparison.InvariantCulture);
			else
			{
				ret = result.Split(' ');
			}
			return ret;
		}

		public override void SetProperty(String key, Object value){
			String result;
			String sValue;
			if (value == null)
				sValue = "";
			else if (value.GetType().IsEnum)
				sValue = value.ToString().Replace("_","-");
			else if (value is String)
				sValue = "\"" + value.ToString() + "\"";
			else if (value is byte[])
				sValue = ((byte[])value).ToHexadecimal(":");
			else if (value is bool)
				sValue = ((bool)value?"1":"0");
			else if (typeof(IEnumerable).IsAssignableFrom(value.GetType())) // Tiene que estar después de string y byte[], ya que string es ienumerable
				sValue = ((IEnumerable)value).ToStringJoin(" ");
			else
				sValue = value.ToString();
			String aux = "set_network " + this.Index + " " + key.ToLower() + " " + sValue;
			result = _WpaSupplicantClient.WpaCliCmd(aux);

			if (result.Equals("FAIL"))
				throw new WLanClientException("Error setting network '" + this.Index + "', variable '" + key + "' with value '" + sValue + "': " + result);
		}

		public override bool Enabled {
			get {
				return !(bool)this.GetProperty("disabled");
			}
			set {
                this.SetProperty("disabled", !value);
			}
		}

        public override void Enable() {
            String result;

            result = _WpaSupplicantClient.WpaCliCmd("enable_network " + this.Index);

            if (result.Equals("FAIL"))
                throw new WLanClientException("Error enabling network '" + this.Index + "': " + result);
        }

        public override void Disable() {
            String result;

            result = _WpaSupplicantClient.WpaCliCmd("disable_network " + this.Index);

            if (result.Equals("FAIL"))
                throw new WLanClientException("Error disabling network '" + this.Index + "': " + result);            
        }

		public override void Select() {
			String result;

			result = _WpaSupplicantClient.WpaCliCmd("select_network " + this.Index);

			if (result.Equals("FAIL"))
				throw new WLanClientException("Error selecting network '" + this.Index + "': " + result);
		}

		public override Boolean Connected
		{
			get
			{
				bool ret = false;

				String result;
				result = _WpaSupplicantClient.WpaCliCmd("status");
				Regex regex = new Regex(@"^([^=]+)=(.+)$", RegexOptions.Multiline);
				MatchCollection mc = regex.Matches(result);
				Match mIndex = mc.Cast<Match>()
								   .FirstOrDefault(v => v.Groups[1].Value == "id");
				if (mIndex != null && this.Index.ToString().Equals(mIndex.Groups[2].Value))
				{
					String sState = mc.Cast<Match>()
									   .FirstOrDefault(v => v.Groups[1].Value == "wpa_state")
									   .Groups[2].Value;
					ret = sState.Equals("COMPLETED", StringComparison.InvariantCultureIgnoreCase);
				}



				/* address=00:e1:b0:10:10:50
 uuid = 2553cce2 - 52db - 5c5c - ae55 - dc1a745887d9

"wpa_state = INACTIVE"


 bssid=c4:12:f5:aa:6e:60
freq=2437
ssid=aseproda
id=0
mode=station
pairwise_cipher=CCMP
group_cipher=TKIP
key_mgmt=WPA2-PSK




 */
				return ret;
			}
		}

        public void SetOpenNetwork()
        {
            this.SetProperty("key_mgmt", "NONE");
        }

        public void SetWEPNetwork(String wepKey) {
            //if(wepKey.Length!=13)
                
            this.SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.NONE);
            this.SetProperty("wep_key0", wepKey);
            this.SetProperty("wep_tx_keyidx", 0);
        }

        public void SetWPANetwork(String wpaKey){
            this.SetProperty("psk", wpaKey);
        }

        public void SetWPA1Network(String wpaKey){
            this.SetWPANetwork(wpaKey);
            this.SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA_PSK);
        }

        public void SetWPA2Network(String wpaKey){
            this.SetWPANetwork(wpaKey);
            this.SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA2_PSK);
        }

        public void SetWPA_EAPNetwork(String identity, String password){
            this.SetProperty("identity", identity);
            this.SetProperty("password", password);
        }

        public void SetWPA1_EAPNetwork(String identity, String password){
            this.SetWPA_EAPNetwork(identity, password);
            this.SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA_EAP);
        }

        public void SetWPA2_EAPNetwork(String identity, String password){
            this.SetWPA_EAPNetwork(identity, password);
            this.SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA2_EAP);
        }

	}
}

