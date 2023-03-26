using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Services.Core
{
    public class ServiceDaemonException : Exception
    {
        public ServiceDaemonException()
        {
        }

        public ServiceDaemonException(string message) : base(message)
        {
        }

        public ServiceDaemonException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ServiceDaemonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}