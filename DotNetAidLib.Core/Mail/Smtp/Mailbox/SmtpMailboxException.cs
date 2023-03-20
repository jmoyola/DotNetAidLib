using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Mail.Smtp.Mailbox
{
    public class SmtpMailboxException:Exception
    {
        public SmtpMailboxException()
        {
        }

        public SmtpMailboxException(string message) : base(message)
        {
        }

        public SmtpMailboxException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SmtpMailboxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
