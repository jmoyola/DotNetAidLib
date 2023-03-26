using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Helpers
{
    [Serializable]
    public class HelperException : Exception
    {
        protected HelperException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HelperException(string message) : base(message)
        {
        }

        public HelperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}