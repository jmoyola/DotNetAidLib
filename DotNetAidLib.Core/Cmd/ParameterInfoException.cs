using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core
{
    public class ParameterInfoException:Exception
    {
        public ParameterInfoException ()
        {
        }

        public ParameterInfoException (string message) : base (message)
        {
        }

        public ParameterInfoException (string message, Exception innerException) : base (message, innerException)
        {
        }

        protected ParameterInfoException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }
}
