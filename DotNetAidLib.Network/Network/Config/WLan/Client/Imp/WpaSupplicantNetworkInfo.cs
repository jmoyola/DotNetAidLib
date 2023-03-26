using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Network.Config.WLan.Client.Core;

namespace DotNetAidLib.Network.Config.WLan.Client.Imp
{
    public class WpaSupplicantNetworkInfo : WLanNetworkInfo
    {
        private readonly WpaSupplicantClient _WpaSupplicantClient;

        public WpaSupplicantNetworkInfo(WpaSupplicantClient wpaSupplicantClient, int Index)
            : base(Index)
        {
            _WpaSupplicantClient = wpaSupplicantClient;
        }

        public override byte[] BSSID
        {
            get => (byte[]) GetProperty("bssid");
            set => SetProperty("bssid", value);
        }

        public override string SSID
        {
            get => (string) GetProperty("ssid");
            set => SetProperty("ssid", value);
        }

        public override bool HiddenSSID
        {
            get => (bool) GetProperty("scan_ssid");
            set => SetProperty("scan_ssid", value);
        }

        public override byte Priority
        {
            get => (byte) GetProperty("priority");
            set => SetProperty("priority", value);
        }

        public override bool Enabled
        {
            get => !(bool) GetProperty("disabled");
            set => SetProperty("disabled", !value);
        }

        public override bool Connected
        {
            get
            {
                var ret = false;

                string result;
                result = _WpaSupplicantClient.WpaCliCmd("status");
                var regex = new Regex(@"^([^=]+)=(.+)$", RegexOptions.Multiline);
                var mc = regex.Matches(result);
                var mIndex = mc.Cast<Match>()
                    .FirstOrDefault(v => v.Groups[1].Value == "id");
                if (mIndex != null && Index.ToString().Equals(mIndex.Groups[2].Value))
                {
                    var sState = mc.Cast<Match>()
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

        public override object GetProperty(string key)
        {
            object ret = null;

            string result;

            result = _WpaSupplicantClient.WpaCliCmd("get_network " + Index + " " + key);

            if (result.Equals("FAIL"))
                ret = null;
            else if (string.IsNullOrEmpty(result))
                ret = null;
            else if (result.RegexIsMatch("^\"([^\"]+)\"$")) // Si el texto está entre comillas
                ret = result.RegexGroupsMatches("^\"([^\"]+)\"$")[1];
            else if (result.RegexIsMatch("^([0-9a-fA-F][0-9a-fA-F]:){5}[0-9a-fA-F][0-9a-fA-F]$"))
                ret = result.Replace(":", "").HexToByteArray();
            else if (result.RegexIsMatch(@"^[01]$"))
                ret = result.Equals("1", StringComparison.InvariantCulture);
            else
                ret = result.Split(' ');
            return ret;
        }

        public override void SetProperty(string key, object value)
        {
            string result;
            string sValue;
            if (value == null)
                sValue = "";
            else if (value.GetType().IsEnum)
                sValue = value.ToString().Replace("_", "-");
            else if (value is string)
                sValue = "\"" + value + "\"";
            else if (value is byte[])
                sValue = ((byte[]) value).ToHexadecimal(":");
            else if (value is bool)
                sValue = (bool) value ? "1" : "0";
            else if
                (typeof(IEnumerable)
                 .IsAssignableFrom(value
                     .GetType())) // Tiene que estar después de string y byte[], ya que string es ienumerable
                sValue = ((IEnumerable) value).ToStringJoin(" ");
            else
                sValue = value.ToString();
            var aux = "set_network " + Index + " " + key.ToLower() + " " + sValue;
            result = _WpaSupplicantClient.WpaCliCmd(aux);

            if (result.Equals("FAIL"))
                throw new WLanClientException("Error setting network '" + Index + "', variable '" + key +
                                              "' with value '" + sValue + "': " + result);
        }

        public override void Enable()
        {
            string result;

            result = _WpaSupplicantClient.WpaCliCmd("enable_network " + Index);

            if (result.Equals("FAIL"))
                throw new WLanClientException("Error enabling network '" + Index + "': " + result);
        }

        public override void Disable()
        {
            string result;

            result = _WpaSupplicantClient.WpaCliCmd("disable_network " + Index);

            if (result.Equals("FAIL"))
                throw new WLanClientException("Error disabling network '" + Index + "': " + result);
        }

        public override void Select()
        {
            string result;

            result = _WpaSupplicantClient.WpaCliCmd("select_network " + Index);

            if (result.Equals("FAIL"))
                throw new WLanClientException("Error selecting network '" + Index + "': " + result);
        }

        public void SetOpenNetwork()
        {
            SetProperty("key_mgmt", "NONE");
        }

        public void SetWEPNetwork(string wepKey)
        {
            //if(wepKey.Length!=13)

            SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.NONE);
            SetProperty("wep_key0", wepKey);
            SetProperty("wep_tx_keyidx", 0);
        }

        public void SetWPANetwork(string wpaKey)
        {
            SetProperty("psk", wpaKey);
        }

        public void SetWPA1Network(string wpaKey)
        {
            SetWPANetwork(wpaKey);
            SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA_PSK);
        }

        public void SetWPA2Network(string wpaKey)
        {
            SetWPANetwork(wpaKey);
            SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA2_PSK);
        }

        public void SetWPA_EAPNetwork(string identity, string password)
        {
            SetProperty("identity", identity);
            SetProperty("password", password);
        }

        public void SetWPA1_EAPNetwork(string identity, string password)
        {
            SetWPA_EAPNetwork(identity, password);
            SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA_EAP);
        }

        public void SetWPA2_EAPNetwork(string identity, string password)
        {
            SetWPA_EAPNetwork(identity, password);
            SetProperty("key_mgmt", KEY_MANAGEMENT_PROTOCOL.WPA2_EAP);
        }
    }
}