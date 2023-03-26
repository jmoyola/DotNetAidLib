using System;
using System.Data;

namespace DotNetAidLib.Database.DAO.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DaoPropertyParserAttribute : Attribute
    {
        public abstract object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row);

        public abstract object SerializeBeforeInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity);

        public virtual void SerializeAfterInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
        }

        public abstract object SerializeBeforeUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity);

        public virtual void SerializeAfterUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
        }

        public virtual void RemoveBeforeDelete(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
        }

        public virtual void RemoveAfterDelete(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
        }
    }
}