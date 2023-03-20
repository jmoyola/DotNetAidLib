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

    public delegate void DaoEventHandler(Object sender, DaoEventArgs args);

    public class DaoEventArgs:EventArgs
    {
        private Type entityType;
        private Object entity;
        private DaoEventType eventType;
        public DaoEventArgs(Type entityType, Object entity, DaoEventType eventType)
        {
            Assert.NotNull( entityType, nameof(entityType));
            this.entityType = entityType;
            this.entity = entity;
            this.eventType = eventType;
        }

        public Type EntityType{
            get{return entityType;}
        }

        public object Entity {
            get { return entity; }
        }

        public DaoEventType EventType{
            get{return eventType;}
        }
    }
}
