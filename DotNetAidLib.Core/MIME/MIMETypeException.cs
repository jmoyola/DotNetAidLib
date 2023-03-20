using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;

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
