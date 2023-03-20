using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Configuration.Dictionary.Core
{
    public class DictionaryConfigException:Exception
    {
        public DictionaryConfigException ()
        {
        }

        public DictionaryConfigException (string message) : base (message)
        {
        }

        public DictionaryConfigException (string message, Exception innerException) : base (message, innerException)
        {
        }

        protected DictionaryConfigException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }
}
