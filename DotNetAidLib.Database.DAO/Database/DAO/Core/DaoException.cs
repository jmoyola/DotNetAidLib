using System;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoException : Exception
    {
        public DaoException()
        {
        }

        public DaoException(string message) : base(message)
        {
        }

        public DaoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}