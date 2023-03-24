using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Units;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixIEEE80211InterfaceInfo:IEEE80211InterfaceInfo
    {
        private string iwconfigOutput="";
        private System.IO.FileInfo iwconfigFileInfo = null;

        public PosixIEEE80211InterfaceInfo(NetworkInterfaceInfo networkInterface)
            :base(networkInterface){
            iwconfigFileInfo = EnvironmentHelper.SearchInPath("iwconfig");
            Assert.Exists(iwconfigFileInfo);

            this.Refresh();
        }

        public override void Refresh() {
            this.iwconfigOutput = iwconfigFileInfo.CmdExecuteSync(this.NetworkInterface.Name);
        }

        public override String IEE802X {
            get {
                try{
                    IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"(IEEE\s[^\s]+)", RegexOptions.Multiline);
                    return (ret.Count>1?ret[1]:null);
                }
                catch{
                    return null;
                }
            }
        }

        public override String ESSID{
            get{
                try{
                    IList<String> ret = this.iwconfigOutput.RegexGroupsMatches("ESSID[=:]\\s?\"([^\"]+)\"", RegexOptions.Multiline);
                    return (ret.Count > 1 ? ret[1] : null);
                }
                catch{
                    return null;
                }

            }
        }

        public override String Mode{
            get{
                try{
                    IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"Mode[=:]\s?(\w+)", RegexOptions.Multiline);
                    return (ret.Count > 1 ? ret[1] : null);
                }
                catch{
                    return null;
                }
            }
        }

        public override long Frequency{
            get{
                try{
                    Decimal ret = 0;
                    IList<String> g = this.iwconfigOutput.RegexGroupsMatches(@"Frequency[=:]\s?([\d\.]+\s?\w+)", RegexOptions.Multiline);
                    if (g.Count > 1)
                        ret = new FrequencyUnitScale(g[1]).TotalValue;
                    return Decimal.ToInt64(ret);
                }
                catch{
                    return -1;
                }

            }
        }

        public override byte[] AccessPoint{
            get{
                IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"Access\sPoint[=:]\s?([0-9A-Fa-f:]+)", RegexOptions.Multiline);
                try{
                    return (ret.Count > 1 ? ret[1].HexToByteArray() : null);
                }
                catch { return new byte[0]; }
            }
        }

        public override long BitRate{
            get{
                Decimal ret = 0;
                IList<String> g = this.iwconfigOutput.RegexGroupsMatches(@"Bit\sRate[=:]\s?([\d\.]+\s?[\w/]+)", RegexOptions.Multiline);
                if (g.Count> 1)
                    ret = new MemoryTransferUnitScale(g[1]);

                return Decimal.ToInt64(ret);
            }
        }

        public override int TxPowerDb{
            get{
                IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"Tx-Power[=:]\s?(\d+)", RegexOptions.Multiline);
                try{
                    return (ret.Count > 1 ? Int32.Parse(ret[1]) : 0);
                }
                catch {
                    return 0;
                }
            }
        }

        public override int SignalLevelPercent{
            get{
                IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"Signal\slevel[=:]\s?((-?\d+)\s?((dbm?)|(/(\d+))))", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return (ret.Count > 1? IEEE80211InterfaceInfo.WLanValueToPercent(ret[1]) : -1);
            }
        }

        public override int LinkQualityPercent{
            get{
                IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"Link\sQuality[=:]\s?((-?\d+)\s?((dbm?)|(/(\d+))))", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return (ret.Count > 1 ? IEEE80211InterfaceInfo.WLanValueToPercent(ret[1]) : -1);
            }
        }

        public override int NoiseLevelPercent{
            get
            {
                IList<String> ret = this.iwconfigOutput.RegexGroupsMatches(@"Noise\slevel[=:]\s?((-?\d+)\s?((dbm?)|(/(\d+))))", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return (ret.Count > 1 ? IEEE80211InterfaceInfo.WLanValueToPercent(ret[1]) : -1);
            }
        }

    }
}
