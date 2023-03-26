using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.IO.Streams
{
    public class UDPBroadcastServerStreamException : Exception
    {
        public UDPBroadcastServerStreamException()
        {
        }

        public UDPBroadcastServerStreamException(string message) : base(message)
        {
        }

        public UDPBroadcastServerStreamException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected UDPBroadcastServerStreamException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}