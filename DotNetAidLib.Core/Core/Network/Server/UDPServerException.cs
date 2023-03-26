using System;

namespace DotNetAidLib.Core.Network.Server
{
    public class UDPServerException : Exception
    {
        public UDPServerException()
        {
        }

        public UDPServerException(string message) : base(message)
        {
        }

        public UDPServerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}