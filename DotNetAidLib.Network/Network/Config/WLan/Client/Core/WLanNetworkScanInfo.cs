using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Network.Config.WLan.Client.Core
{
    public class WLanNetworkScanInfo
    {
        public WLanNetworkScanInfo(byte[] BSSID, string SSID, int Frequency, int Signal,
            IEnumerable<string> Capabilities)
        {
            this.BSSID = BSSID;
            this.SSID = SSID;
            this.Frequency = Frequency;
            this.Signal = Signal;
            this.Capabilities = Capabilities;
        }

        public byte[] BSSID { get; }

        public string SSID { get; }

        public int Frequency { get; }

        public int Signal { get; }

        public int Quality => 2 * (Signal + 100);

        public IEnumerable<string> Capabilities { get; }

        public int SignalPercent
        {
            get
            {
                try
                {
                    return IEEE80211InterfaceInfo.SignalPercentOfdB(Signal);
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override string ToString()
        {
            var ret = "BSSID=" + (BSSID == null ? "" : BSSID.ToHexadecimal(":")) + "\nSSID=" + SSID + "\nFrequency=" +
                      Frequency + "\nSignal=" + Signal + "\nQuality=" + Quality + "\nCapabilities=" +
                      Capabilities.ToStringJoin(", ");

            return ret;
        }
    }
}