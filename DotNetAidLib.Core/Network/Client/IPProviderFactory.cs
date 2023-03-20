using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Plugins;
using System.Net.Sockets;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Network.Client
{
    public class IPProviderFactory
    {
		
		private static IPProviderFactory instance = null;
		private IEnumerable<PlugIn<IPublicIPProvider>> _Plugins;

		private CachedRequest<String, IPAddress> cachedRequest = new CachedRequest<String, IPAddress>();
        
        protected IPProviderFactory(){
			_Plugins = PluginFactory<IPublicIPProvider>.Instance().PlugIns.OrderBy(v=>v.Info.PreferentOrder);
			cachedRequest = new CachedRequest<String, IPAddress>(50 * 60 * 1000);
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
                ret = Dns.GetHostAddresses(Dns.GetHostName()).Where(a => a.AddressFamily.Equals(AddressFamily.InterNetwork)).ToArray()[adapterNumber];
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
                UdpClient udpToExternalIp = new UdpClient("8.8.8.8", 1);
                ret = ((IPEndPoint)udpToExternalIp.Client.LocalEndPoint).Address;
            }
            catch
            {
                ret = null;
            }
            return ret;
        }

		public IPAddress GetPublic(){
			IPAddress ret = null;

			foreach (PlugIn<IPublicIPProvider> plugin in _Plugins)
            {
                try
                {
                    ret = plugin.Instance().Request();
                    break;
                }
                catch { }
            }

            return ret;
		}

		private Object oGetPublicCached = new Object();
        public IPAddress GetPublicCached()
        {
            lock (oGetPublicCached)
            {
				return cachedRequest.GetValue("instance", (k) => this.GetPublic());
            }
        }

		public int CachedTime{
			get { return this.cachedRequest.CachedTime; }
			set { this.cachedRequest.CachedTime = value;
			}
		}

		public static IPProviderFactory Instance(){
			if (instance == null)
				instance = new IPProviderFactory();
			return instance;
		}
    }
}
