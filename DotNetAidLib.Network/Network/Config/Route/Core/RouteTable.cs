using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DotNetAidLib.Network.Config.Route.Imp;

namespace DotNetAidLib.Network.Config.Route.Core
{
    public class RouteTable
    {
        private static RouteTable _Instance;

        protected RouteTable()
        {
        }

        public string Name { get; set; } = null;

        public virtual IList<RouteEntry> Entries { get; }

        public virtual void Add(RouteEntry entry)
        {
        }

        public virtual void Update(RouteEntry entry)
        {
        }

        public virtual void Delete(RouteEntry entry)
        {
        }

        public static RouteTable Instance()
        {
            if (_Instance == null)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _Instance = LinuxRouteTable.Instance();
                else
                    throw new NotImplementedException();
            }

            return _Instance;
        }
    }
}