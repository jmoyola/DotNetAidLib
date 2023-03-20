using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public abstract class DaoPropertyParserAttribute : Attribute
	{
		public DaoPropertyParserAttribute()
		{ }

		public abstract Object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, IDataReader row);
		public abstract Object SerializeBeforeInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity);
        public virtual void SerializeAfterInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity){ }
        public abstract Object SerializeBeforeUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity);
        public virtual void SerializeAfterUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity) { }
        public virtual void RemoveBeforeDelete(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity) { }
        public virtual void RemoveAfterDelete(DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity) { }
    }
}
