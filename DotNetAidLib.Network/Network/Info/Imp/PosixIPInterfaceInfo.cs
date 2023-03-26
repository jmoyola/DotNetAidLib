using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Network.Config.TcpIp.Core;

namespace DotNetAidLib.Core.Network.Info.Imp
{
    public class PosixIPInterfaceInfo : IPInterfaceInfo
    {
        private readonly FileInfo ipFile;

        public PosixIPInterfaceInfo(NetworkInterfaceInfo networkInterfaceInfo)
            : base(networkInterfaceInfo)
        {
            ipFile = EnvironmentHelper.SearchInPath("ip");
            Assert.Exists(ipFile);
        }

        public override IEnumerable<IPInfo> IPInformation
        {
            get
            {
                try
                {
                    IList<IPInfo> ret = new List<IPInfo>();
                    var cmdRet = ipFile.CmdExecuteSync("-o addr show " + NetworkInterfaceInfo.Name);
                    var r = new Regex(@"^(\d+):\s([^\s]+)\s+(inet\d?)\s([^/]+)/([^\s]+)", RegexOptions.Multiline);
                    foreach (Match m in r.Matches(cmdRet))
                    {
                        IPInfo tcpInfo;
                        var ipVersion = m.Groups[3].Value;
                        if (string.IsNullOrEmpty(m.Groups[7].Value))
                            tcpInfo = new IPInfo(IPAddress.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value));
                        else
                            tcpInfo = new IPInfo(IPAddress.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value),
                                IPAddress.Parse(m.Groups[7].Value));

                        ret.Add(tcpInfo);
                    }

                    return ret;
                }
                catch (Exception ex)
                {
                    throw new NetworkingException("Error retrieving IP values from interfaces: " + ex.Message, ex);
                }
            }
        }

        public override void AddIPInfo(IPInfo ipInfo)
        {
            ipFile.CmdExecuteSync("addr add " + ipInfo.Address + "/" + ipInfo.Netmask.ToNetworkMaskLength() + " dev " +
                                  NetworkInterfaceInfo.Name);
        }

        public override void RemoveIPInfo(IPInfo ipInfo)
        {
            ipFile.CmdExecuteSync("addr del " + ipInfo.Address + "/" + ipInfo.Netmask.ToNetworkMaskLength() + " dev " +
                                  NetworkInterfaceInfo.Name);
        }
    }
}