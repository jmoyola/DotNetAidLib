using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Core.Network.Config.Route.Core
{
	public class RouteEntry
	{
		public RouteEntry ()
		{
		}
		public IPAddress To{ get; set;}
		public IPAddress Gateway{ get; set;}
		public IPAddress Mask{ get; set;}
		public bool Enable{ get; set;}
		public bool DefaultGateway{ get; set;}
		public bool RouteToHost{ get; set;}
		public bool CreatedByRedirect{ get; set;}
		public bool ModifiedByRedirect{ get; set;}
        public uint Metric { get; set; } = 1;
		public uint Reference{ get; set;}
		public uint Use{ get; set;}
		public NetworkInterfaceInfo Interface{ get; set;}
	}
}

