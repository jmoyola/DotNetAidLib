using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Config.Route.Imp;

namespace DotNetAidLib.Core.Network.Config.Route.Core
{
	public class RouteTable
	{
		private static RouteTable _Instance = null;

		protected RouteTable()
		{
		}

        public String Name { get; set; } = null;

		public virtual IList<RouteEntry> Entries { get; }
		public virtual void Add(RouteEntry entry) { }
		public virtual void Update(RouteEntry entry) { }
		public virtual void Delete(RouteEntry entry) { }

		public static RouteTable Instance() {
			if (_Instance == null) {
				if (!Helpers.Helper.IsWindowsSO())
					_Instance = LinuxRouteTable.Instance();
				else
					throw new NotImplementedException();
			}
			return _Instance;
		}
	}
}

