using System;

namespace DotNetAidLib.Database.DAO.Navigation
{
    public class DaoNavigationException:Exception
    {
        public DaoNavigationException() : base(){}
        public DaoNavigationException(String message) : base(message) { }
        public DaoNavigationException(String message, Exception innerException) : base(message, innerException) { }
    }
}
