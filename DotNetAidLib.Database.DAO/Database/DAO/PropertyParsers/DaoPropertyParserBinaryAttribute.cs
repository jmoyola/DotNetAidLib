using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyParserBinaryAttribute : DaoPropertyParserAttribute
    {
        public override object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            if (row.IsDBNull(propertyAttribute.ColumnName))
                return null;
            return row.GetAllBytes(propertyAttribute.ColumnName);
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
            return (byte[]) propertyAttribute.PropertyInfo.GetValue(entity);
        }
    }
}