using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.MIME
{
    public class MIMETypeException : Exception
    {
        public MIMETypeException()
        {
        }

        public MIMETypeException(string message) : base(message)
        {
        }

        public MIMETypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MIMETypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}