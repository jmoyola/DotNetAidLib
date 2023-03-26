using System;

namespace DotNetAidLib.OperatingSystem.Core
{
    public class OperatingSystemException : Exception
    {
        public OperatingSystemException()
        {
        }

        public OperatingSystemException(string message)
            : base(message)
        {
        }

        public OperatingSystemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}