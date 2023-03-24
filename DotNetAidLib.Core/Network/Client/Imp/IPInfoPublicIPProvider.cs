using System;
using System.Net;
using DotNetAidLib.Core.Network.Client.Core;

namespace DotNetAidLib.Core.Network.Client.Imp
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
				ret = IPAddress.Parse(Client.Core.WebRequestFactory.RequestString(new Uri("http://ipinfo.io/ip"))
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

