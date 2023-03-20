using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public delegate void DaoSQLEventHandler(Object sender, DaoSQLEventArgs args);

    public class DaoSQLEventArgs:EventArgs
    {
        private Type entityType;
        private String sqlEvent;
        public DaoSQLEventArgs(Type entityType, String sqlEvent)
        {
            Assert.NotNull( entityType, nameof(entityType));
            Assert.NotNull( sqlEvent, nameof(sqlEvent));
            this.sqlEvent = sqlEvent;
            this.entityType = entityType;
        }

        public Type EntityType
        {
            get { return entityType; }
        }

        public String SQLEvent
        {
            get{return sqlEvent;}
        }
    }
}
