using System;

namespace DotNetAidLib.Core.Network.Server
{
    public class TCPServerException : Exception
    {
        public TCPServerException()
        {
        }

        public TCPServerException(string message) : base(message)
        {
        }

        public TCPServerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}