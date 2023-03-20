using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.IO.Streams
{
    public class TCPServerStreamException : Exception
    {
        public TCPServerStreamException ()
        {
        }

        public TCPServerStreamException (string message) : base (message)
        {
        }

        public TCPServerStreamException (string message, Exception innerException) : base (message, innerException)
        {
        }

        protected TCPServerStreamException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }
}
