using System;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Network.Config.WLan.Client.Core
{
    public enum KEY_MANAGEMENT_PROTOCOL
    {
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

    public abstract class WLanNetworkInfo : IWLanNetworkInfo
    {
        public WLanNetworkInfo(int index)
        {
            Index = index;
        }

        public abstract bool HiddenSSID { get; set; }

        public abstract byte[] BSSID { get; set; }
        public abstract string SSID { get; set; }
        public abstract byte Priority { get; set; }

        public int Index { get; }

        public abstract object GetProperty(string key);
        public abstract void SetProperty(string key, object value);

        public abstract bool Enabled { get; set; }

        public abstract void Enable();
        public abstract void Disable();

        public abstract void Select();

        public abstract bool Connected { get; }

        public override string ToString()
        {
            var ret = "(" + Index + ") BSSID=" + (BSSID == null ? "" : BSSID.ToHexadecimal(":")) + " SSID=" + SSID +
                      " Enabled=" + Enabled + " Connected= " + Connected;

            return ret;
        }
    }
}