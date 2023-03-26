using System;
using System.Net;
using DotNetAidLib.Core.Network.Client.Core;

namespace DotNetAidLib.Core.Network.Client.Imp
{
    public class IPInfoPublicIPProvider : IPublicIPProvider
    {
        public int PreferentOrder => 1;

        public IPAddress Request()
        {
            IPAddress ret = null;

            try
            {
                ret = IPAddress.Parse(WebRequestFactory.RequestString(new Uri("http://ipinfo.io/ip"))
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