using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public enum DaoEventType
    {
        BeforeRetrieve,
        AfterRetrieve,
        BeforeAdd,
        AfterAdd,
        BeforeUpdate,
        AfterUpdate,
        BeforeRemove,
        AfterRemove
    }

    public delegate void DaoEventHandler(object sender, DaoEventArgs args);

    public class DaoEventArgs : EventArgs
    {
        public DaoEventArgs(Type entityType, object entity, DaoEventType eventType)
        {
            Assert.NotNull(entityType, nameof(entityType));
            EntityType = entityType;
            Entity = entity;
            EventType = eventType;
        }

        public Type EntityType { get; }

        public object Entity { get; }

        public DaoEventType EventType { get; }
    }
}