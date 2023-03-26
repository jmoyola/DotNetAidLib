using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public delegate void DaoSQLEventHandler(object sender, DaoSQLEventArgs args);

    public class DaoSQLEventArgs : EventArgs
    {
        public DaoSQLEventArgs(Type entityType, string sqlEvent)
        {
            Assert.NotNull(entityType, nameof(entityType));
            Assert.NotNull(sqlEvent, nameof(sqlEvent));
            SQLEvent = sqlEvent;
            EntityType = entityType;
        }

        public Type EntityType { get; }

        public string SQLEvent { get; }
    }
}