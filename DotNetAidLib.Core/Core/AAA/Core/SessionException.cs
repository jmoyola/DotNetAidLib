using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.AAA
{
    public class SessionException : Exception
    {
        public SessionException(string message) : base(message)
        {
        }

        public SessionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SessionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}