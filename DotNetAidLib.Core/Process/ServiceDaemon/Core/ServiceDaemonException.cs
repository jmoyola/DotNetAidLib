using System;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Core
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

