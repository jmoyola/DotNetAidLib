using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Mail.Smtp.Client
{
    public class SmtpException : Exception
    {
        public SmtpException()
        {
        }

        public SmtpException(string message)
            : base(message)
        {
        }

        public SmtpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SmtpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}