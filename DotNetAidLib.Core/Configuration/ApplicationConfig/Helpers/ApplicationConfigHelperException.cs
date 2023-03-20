using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Helpers
{
    public class ApplicationConfigHelperException:Exception
    {
        public ApplicationConfigHelperException()
        {
        }

        public ApplicationConfigHelperException(string message) : base(message)
        {
        }

        public ApplicationConfigHelperException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApplicationConfigHelperException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
