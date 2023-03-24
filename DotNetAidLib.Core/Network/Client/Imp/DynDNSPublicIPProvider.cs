using System;
using System.Net;
using DotNetAidLib.Core.Network.Client.Core;

namespace DotNetAidLib.Core.Network.Client.Imp
{
	public class DynDNSPublicIPProvider:IPublicIPProvider
	{
		public int PreferentOrder {
			get { return 2; }
		}
		public IPAddress Request (){
			IPAddress ret = null;
            try
            {            
				ret= IPAddress.Parse(Client.Core.WebRequestFactory.RequestString(new Uri("http://checkip.dyndns.org/"))
				                        .Replace("\r\n","")
				                        .Replace("\n", "")
				                        .Split(':')[1].Trim()
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

