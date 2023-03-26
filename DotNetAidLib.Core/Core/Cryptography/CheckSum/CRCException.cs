using System;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CRCException : Exception
    {
        public CRCException()
        {
        }

        public CRCException(string message) : base(message)
        {
        }

        public CRCException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}