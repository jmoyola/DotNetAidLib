using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Network.Config.Route.Core;
using DotNetAidLib.OperatingSystem.Imp;

namespace DotNetAidLib.Network.Config.Route.Imp
{
    public class LinuxRouteTable : RouteTable
    {
        private static RouteTable _Instance;
        private readonly FileInfo _IpFileInfo;
        private readonly FileInfo _RouteFileInfo;

        private LinuxRouteTable()
        {
            _RouteFileInfo = EnvironmentHelper.SearchInPath("route");
            Assert.NotNull(_RouteFileInfo);

            _IpFileInfo = EnvironmentHelper.SearchInPath("ip");
            Assert.NotNull(_IpFileInfo);
        }

        public override IList<RouteEntry> Entries
        {
            get
            {
                IList<RouteEntry> ret = new List<RouteEntry>();
                var sRouteTable = _RouteFileInfo.CmdExecuteSync("-n");
                var r = new Regex(
                    @"((\d+\.){3}\d+)\s+((\d+\.){3}\d+)\s+((\d+\.){3}\d+)\s+([A-Z]+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(.+)");
                var mc = r.Matches(sRouteTable);
                foreach (Match m in mc)
                {
                    var ro = new RouteEntry();
                    ro.To = IPAddress.Parse(m.Groups[1].Value);
                    ro.Gateway = IPAddress.Parse(m.Groups[3].Value);
                    ro.Mask = IPAddress.Parse(m.Groups[5].Value);
                    var flags = m.Groups[7].Value;
                    ro.Enable = flags.IndexOf("U") > -1;
                    ro.DefaultGateway = flags.IndexOf("G") > -1;
                    ro.RouteToHost = flags.IndexOf("H") > -1;
                    ro.CreatedByRedirect = flags.IndexOf("D") > -1;
                    ro.ModifiedByRedirect = flags.IndexOf("M") > -1;
                    ro.Metric = uint.Parse(m.Groups[8].Value);
                    ro.Reference = uint.Parse(m.Groups[9].Value);
                    ro.Use = uint.Parse(m.Groups[10].Value);
                    ro.Interface = NetworkInterfaceInfo
                        .GetAllNetworkInterfaces()
                        .FirstOrDefault(v => v.Name.Equals(m.Groups[11].Value));
                    ret.Add(ro);
                }

                return ret;
            }
        }

        public override void Add(RouteEntry entry)
        {
            var parameters = "ip route add";
            if (entry.DefaultGateway)
                parameters += " default";
            else
                parameters += " " + entry.To;
            parameters += " via " + entry.Gateway;
            if (entry.Interface != null)
                parameters += " dev " + entry.Interface.Name;
            if (!string.IsNullOrEmpty(Name))
                parameters += " table " + Name;

            LinuxOperatingSystem.Instance().AdminExecute(_IpFileInfo, parameters);
        }

        public override void Update(RouteEntry entry)
        {
            var parameters = "ip route change";
            if (entry.DefaultGateway)
                parameters += " default";
            else
                parameters += " " + entry.To;
            parameters += " via " + entry.Gateway;
            if (entry.Interface != null)
                parameters += " dev " + entry.Interface.Name;
            if (!string.IsNullOrEmpty(Name))
                parameters += " table " + Name;

            LinuxOperatingSystem.Instance().AdminExecute(_IpFileInfo, parameters);
        }

        public override void Delete(RouteEntry entry)
        {
            var parameters = "ip route del";
            if (entry.DefaultGateway)
                parameters += " default";
            else
                parameters += " " + entry.To;

            LinuxOperatingSystem.Instance().AdminExecute(_IpFileInfo, parameters);
        }

        public new static RouteTable Instance()
        {
            if (_Instance == null)
                _Instance = new LinuxRouteTable();

            return _Instance;
        }
    }
}