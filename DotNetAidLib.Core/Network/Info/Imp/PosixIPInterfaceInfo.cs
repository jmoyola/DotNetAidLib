using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using System.Net;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixIPInterfaceInfo:IPInterfaceInfo
    {
        private System.IO.FileInfo ipFile;

        public PosixIPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
            :base(networkInterfaceInfo){
            ipFile = new System.IO.FileInfo(".")
                .FromPathEnvironmentVariable("ip");
            Assert.Exists(ipFile);
        }

        public override IEnumerable<Core.IPInfo> IPInformation {
            get {
                try
                {
					IList<Core.IPInfo> ret = new List<Core.IPInfo>();
                    String cmdRet=ipFile.CmdExecuteSync("-o addr show " + this.NetworkInterfaceInfo.Name);
                    Regex r = new Regex(@"^(\d+):\s([^\s]+)\s+(inet\d?)\s([^/]+)/([^\s]+)", RegexOptions.Multiline);
                    foreach (Match m in r.Matches(cmdRet)){
						Core.IPInfo tcpInfo;
                        String ipVersion = m.Groups[3].Value;
                        if (String.IsNullOrEmpty(m.Groups[7].Value))
                            tcpInfo = new Core.IPInfo(IPAddress.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value));
                        else
                            tcpInfo = new Core.IPInfo(IPAddress.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value), IPAddress.Parse(m.Groups[7].Value));

                        ret.Add(tcpInfo);
                    }
                    return ret;
                }
                catch (Exception ex) {
                    throw new NetworkingException("Error retrieving IP values from interfaces: " + ex.Message, ex);
                }
            }
        }
        public override void AddIPInfo(Core.IPInfo ipInfo) {
            DotNetAidLib.Core.OperatingSystem.Core.OperatingSystem.Instance()
                   .AdminExecute(ipFile, "addr add " + ipInfo.Address.ToString() + "/" + ipInfo.Netmask.ToNetworkMaskLength() + " dev " + this.NetworkInterfaceInfo.Name);
        }
        public override void RemoveIPInfo(Core.IPInfo ipInfo) {
            DotNetAidLib.Core.OperatingSystem.Core.OperatingSystem.Instance()
                   .AdminExecute(ipFile, "addr del " + ipInfo.Address.ToString() + "/" + ipInfo.Netmask.ToNetworkMaskLength() + " dev " + this.NetworkInterfaceInfo.Name);
        }
    }
}
