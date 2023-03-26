using System;

namespace DotNetAidLib.Database.DAO.Navigation
{
    public class DaoNavigationException : Exception
    {
        public DaoNavigationException()
        {
        }

        public DaoNavigationException(string message) : base(message)
        {
        }

        public DaoNavigationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}