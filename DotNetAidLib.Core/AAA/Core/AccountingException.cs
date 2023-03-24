using System;
using System.Runtime.Serialization;

namespace Library.AAA.Core
{
    public class AccountingException:Exception
    {
        public AccountingException()
        {
        }

        public AccountingException(string message) : base (message)
        {
        }

        public AccountingException(string message, Exception innerException) : base (message, innerException)
        {
        }

        protected AccountingException(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }
}
