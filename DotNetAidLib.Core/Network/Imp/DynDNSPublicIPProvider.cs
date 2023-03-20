using System;
using System.Net;
using DotNetAidLib.Core.Network.Client;

namespace DotNetAidLib.Core.Network.Imp
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
				ret= IPAddress.Parse(Client.WebRequestFactory.RequestString(new Uri("http://checkip.dyndns.org/"))
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

