using System;

namespace DotNetAidLib.Network.Config.WLan.Client.Core
{
    public class WLanClientException : Exception
    {
        public WLanClientException()
        {
        }

        public WLanClientException(string message) : base(message)
        {
        }

        public WLanClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}