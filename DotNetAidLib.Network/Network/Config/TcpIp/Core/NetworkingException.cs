using System;

namespace DotNetAidLib.Network.Config.TcpIp.Core
{
    public class NetworkingException : Exception
    {
        public NetworkingException()
        {
        }

        public NetworkingException(string message) : base(message)
        {
        }

        public NetworkingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}