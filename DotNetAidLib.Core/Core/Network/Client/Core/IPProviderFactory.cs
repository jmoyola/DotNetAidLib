using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Plugins;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public class IPProviderFactory
    {
        private static IPProviderFactory instance;
        private readonly IEnumerable<PlugIn<IPublicIPProvider>> _Plugins;

        private readonly CachedRequest<string, IPAddress> cachedRequest = new CachedRequest<string, IPAddress>();

        private readonly object oGetPublicCached = new object();

        protected IPProviderFactory()
        {
            _Plugins = PluginFactory<IPublicIPProvider>.Instance().PlugIns.OrderBy(v => v.Info.PreferentOrder);
            cachedRequest = new CachedRequest<string, IPAddress>(50 * 60 * 1000);
        }

        public int CachedTime
        {
            get => cachedRequest.CachedTime;
            set => cachedRequest.CachedTime = value;
        }

        public IPAddress GetPrivate()
        {
            return GetPrivate(0);
        }

        public IPAddress GetPrivate(int adapterNumber)
        {
            IPAddress ret = null;

            try
            {
                ret = Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(a => a.AddressFamily.Equals(AddressFamily.InterNetwork)).ToArray()[adapterNumber];
            }
            catch
            {
                ret = null;
            }

            return ret;
        }

        public IPAddress GetPrivateFromPublicIp()
        {
            IPAddress ret = null;
            try
            {
                // Determinamos la ip del adaptador principal que tiene salida a internet
                var udpToExternalIp = new UdpClient("8.8.8.8", 1);
                ret = ((IPEndPoint) udpToExternalIp.Client.LocalEndPoint).Address;
            }
            catch
            {
                ret = null;
            }

            return ret;
        }

        public IPAddress GetPublic()
        {
            IPAddress ret = null;

            foreach (var plugin in _Plugins)
                try
                {
                    ret = plugin.Instance().Request();
                    break;
                }
                catch
                {
                }

            return ret;
        }

        public IPAddress GetPublicCached()
        {
            lock (oGetPublicCached)
            {
                return cachedRequest.GetValue("instance", k => GetPublic());
            }
        }

        public static IPProviderFactory Instance()
        {
            if (instance == null)
                instance = new IPProviderFactory();
            return instance;
        }
    }
}