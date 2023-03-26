using System;
using System.Net;
using DotNetAidLib.Core.Network.Client.Core;

namespace DotNetAidLib.Core.Network.Client.Imp
{
    public class IPIfyPublicIPProvider : IPublicIPProvider
    {
        public int PreferentOrder => 0;

        public IPAddress Request()
        {
            IPAddress ret = null;

            try
            {
                ret = IPAddress.Parse(WebRequestFactory.RequestString(new Uri("https://api.ipify.org"))
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