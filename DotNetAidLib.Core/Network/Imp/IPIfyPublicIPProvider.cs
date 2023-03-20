using System;
using System.Net;
using DotNetAidLib.Core.Network.Client;

namespace DotNetAidLib.Core.Network.Imp
{
	public class IPIfyPublicIPProvider:IPublicIPProvider
	{
		public int PreferentOrder {
			get { return 0; }
		}
		public IPAddress Request (){
			IPAddress ret = null;
            
            try
            {
				ret = IPAddress.Parse(Client.WebRequestFactory.RequestString(new Uri("https://api.ipify.org"))
				                      .Replace("\r\n", "")
                                      .Replace("\n", "")
				                     );
                return ret;
            }
            catch
            {
                return null;
            }	
		}
	}
}

