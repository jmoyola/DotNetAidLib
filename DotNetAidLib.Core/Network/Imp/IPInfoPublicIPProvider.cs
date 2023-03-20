using System;
using System.Net;
using DotNetAidLib.Core.Network.Client;

namespace DotNetAidLib.Core.Network.Imp
{
	public class IPInfoPublicIPProvider:IPublicIPProvider
	{
		public int PreferentOrder {
			get { return 1; }
		}
		public IPAddress Request (){
			IPAddress ret = null;
            
            try
            {
				ret = IPAddress.Parse(Client.WebRequestFactory.RequestString(new Uri("http://ipinfo.io/ip"))
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

