using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Units;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixIEEE80211InterfaceInfo : IEEE80211InterfaceInfo
    {
        private readonly FileInfo iwconfigFileInfo;
        private string iwconfigOutput = "";

        public PosixIEEE80211InterfaceInfo(NetworkInterfaceInfo networkInterface)
            : base(networkInterface)
        {
            iwconfigFileInfo = EnvironmentHelper.SearchInPath("iwconfig");
            Assert.Exists(iwconfigFileInfo);

            Refresh();
        }

        public override string IEE802X
        {
            get
            {
                try
                {
                    var ret = iwconfigOutput.RegexGroupsMatches(@"(IEEE\s[^\s]+)", RegexOptions.Multiline);
                    return ret.Count > 1 ? ret[1] : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public override string ESSID
        {
            get
            {
                try
                {
                    var ret = iwconfigOutput.RegexGroupsMatches("ESSID[=:]\\s?\"([^\"]+)\"", RegexOptions.Multiline);
                    return ret.Count > 1 ? ret[1] : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public override string Mode
        {
            get
            {
                try
                {
                    var ret = iwconfigOutput.RegexGroupsMatches(@"Mode[=:]\s?(\w+)", RegexOptions.Multiline);
                    return ret.Count > 1 ? ret[1] : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public override long Frequency
        {
            get
            {
                try
                {
                    decimal ret = 0;
                    var g = iwconfigOutput.RegexGroupsMatches(@"Frequency[=:]\s?([\d\.]+\s?\w+)",
                        RegexOptions.Multiline);
                    if (g.Count > 1)
                        ret = new FrequencyUnitScale(g[1]).TotalValue;
                    return decimal.ToInt64(ret);
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override byte[] AccessPoint
        {
            get
            {
                var ret = iwconfigOutput.RegexGroupsMatches(@"Access\sPoint[=:]\s?([0-9A-Fa-f:]+)",
                    RegexOptions.Multiline);
                try
                {
                    return ret.Count > 1 ? ret[1].HexToByteArray() : null;
                }
                catch
                {
                    return new byte[0];
                }
            }
        }

        public override long BitRate
        {
            get
            {
                decimal ret = 0;
                var g = iwconfigOutput.RegexGroupsMatches(@"Bit\sRate[=:]\s?([\d\.]+\s?[\w/]+)",
                    RegexOptions.Multiline);
                if (g.Count > 1)
                    ret = new MemoryTransferUnitScale(g[1]);

                return decimal.ToInt64(ret);
            }
        }

        public override int TxPowerDb
        {
            get
            {
                var ret = iwconfigOutput.RegexGroupsMatches(@"Tx-Power[=:]\s?(\d+)", RegexOptions.Multiline);
                try
                {
                    return ret.Count > 1 ? int.Parse(ret[1]) : 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override int SignalLevelPercent
        {
            get
            {
                var ret = iwconfigOutput.RegexGroupsMatches(@"Signal\slevel[=:]\s?((-?\d+)\s?((dbm?)|(/(\d+))))",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return ret.Count > 1 ? WLanValueToPercent(ret[1]) : -1;
            }
        }

        public override int LinkQualityPercent
        {
            get
            {
                var ret = iwconfigOutput.RegexGroupsMatches(@"Link\sQuality[=:]\s?((-?\d+)\s?((dbm?)|(/(\d+))))",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return ret.Count > 1 ? WLanValueToPercent(ret[1]) : -1;
            }
        }

        public override int NoiseLevelPercent
        {
            get
            {
                var ret = iwconfigOutput.RegexGroupsMatches(@"Noise\slevel[=:]\s?((-?\d+)\s?((dbm?)|(/(\d+))))",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return ret.Count > 1 ? WLanValueToPercent(ret[1]) : -1;
            }
        }

        public override void Refresh()
        {
            iwconfigOutput = iwconfigFileInfo.CmdExecuteSync(NetworkInterface.Name);
        }
    }
}