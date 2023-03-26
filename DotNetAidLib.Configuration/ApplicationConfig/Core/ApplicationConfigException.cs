using System;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core
{
    public class ApplicationConfigException : Exception
    {
        public ApplicationConfigException()
        {
        }

        public ApplicationConfigException(string msg) : base(msg)
        {
        }

        public ApplicationConfigException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}