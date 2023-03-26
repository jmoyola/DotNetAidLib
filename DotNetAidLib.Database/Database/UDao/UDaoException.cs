using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.UDao
{
    public class UDaoException : Exception
    {
        public UDaoException()
        {
        }

        public UDaoException(string message) : base(message)
        {
        }

        public UDaoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UDaoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}