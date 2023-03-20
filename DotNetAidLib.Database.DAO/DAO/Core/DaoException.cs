using System;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoException:Exception
    {
        public DaoException() : base(){}
        public DaoException(String message) : base(message) { }
        public DaoException(String message, Exception innerException) : base(message, innerException) { }
    }
}
