using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyParserBlobAttribute : DaoPropertyParserAttribute
    {
        public override object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            if (row.IsDBNull(propertyAttribute.ColumnName))
                return null;
            return new DaoBlob(row.GetAllBytes(propertyAttribute.ColumnName));
        }

        public override object SerializeBeforeInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return ToDatabase(daoSession, entityAttribute, propertyAttribute, entity);
        }

        public override object SerializeBeforeUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return ToDatabase(daoSession, entityAttribute, propertyAttribute, entity);
        }

        private object ToDatabase(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return ((DaoBlob) propertyAttribute.PropertyInfo.GetValue(entity)).Value;
        }
    }
}