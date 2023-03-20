using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Config.Route.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.Route.Imp
{
	public class LinuxRouteTable:RouteTable
	{
		private static RouteTable _Instance = null;
		private FileInfo _RouteFileInfo = null;
		private FileInfo _IpFileInfo = null;
		private LinuxRouteTable (){
			_RouteFileInfo = new FileInfo(".").FromPathEnvironmentVariable("route");
			Assert.NotNull(_RouteFileInfo);

			_IpFileInfo = new FileInfo(".").FromPathEnvironmentVariable("ip");
			Assert.NotNull(_IpFileInfo);
		}

		public override IList<RouteEntry> Entries{
			get
			{
				IList<RouteEntry> ret = new List<RouteEntry>();
				String sRouteTable = _RouteFileInfo.CmdExecuteSync("-n");
				Regex r = new Regex(@"((\d+\.){3}\d+)\s+((\d+\.){3}\d+)\s+((\d+\.){3}\d+)\s+([A-Z]+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(.+)");
				MatchCollection mc = r.Matches(sRouteTable);
				foreach (Match m in mc)
				{
					RouteEntry ro = new RouteEntry();
					ro.To = IPAddress.Parse(m.Groups[1].Value);
					ro.Gateway = IPAddress.Parse(m.Groups[3].Value);
					ro.Mask = IPAddress.Parse(m.Groups[5].Value);
					String flags = m.Groups[7].Value;
					ro.Enable = (flags.IndexOf("U") > -1);
					ro.DefaultGateway = (flags.IndexOf("G") > -1);
					ro.RouteToHost = (flags.IndexOf("H") > -1);
					ro.CreatedByRedirect = (flags.IndexOf("D") > -1);
					ro.ModifiedByRedirect = (flags.IndexOf("M") > -1);
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

		public override void Add(RouteEntry entry) {
			String parameters = "ip route add";
			if (entry.DefaultGateway)
				parameters += " default";
			else
				parameters += " " + entry.To.ToString();
			parameters += " via " + entry.Gateway.ToString();
			if(entry.Interface!=null)
				parameters += " dev " + entry.Interface.Name;
			if (!String.IsNullOrEmpty(this.Name))
				parameters += " table " + this.Name;
			
			OperatingSystem.Core.OperatingSystem.Instance().AdminExecute(_IpFileInfo, parameters);
		}

		public override void Update(RouteEntry entry) {
			String parameters = "ip route change";
			if (entry.DefaultGateway)
				parameters += " default";
			else
				parameters += " " + entry.To.ToString();
			parameters += " via " + entry.Gateway.ToString();
			if (entry.Interface != null)
				parameters += " dev " + entry.Interface.Name;
			if (!String.IsNullOrEmpty(this.Name))
				parameters += " table " + this.Name;

			OperatingSystem.Core.OperatingSystem.Instance().AdminExecute(_IpFileInfo, parameters);
		}

		public override void Delete(RouteEntry entry) {
			String parameters = "ip route del";
			if (entry.DefaultGateway)
				parameters += " default";
			else
				parameters += " " + entry.To.ToString();

			OperatingSystem.Core.OperatingSystem.Instance().AdminExecute(_IpFileInfo, parameters);
		}

		public static new RouteTable Instance()
		{
			if (_Instance == null)
				_Instance = new LinuxRouteTable();
			
			return _Instance;
		}
	}
}

