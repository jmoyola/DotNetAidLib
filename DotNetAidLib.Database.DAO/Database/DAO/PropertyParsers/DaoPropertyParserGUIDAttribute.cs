using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    public enum GUIDPropertyParseType
    {
        String,
        Binary
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyParserGUIDAttribute : DaoPropertyParserAttribute
    {
        private readonly GUIDPropertyParseType guidParseType = GUIDPropertyParseType.Binary;

        public DaoPropertyParserGUIDAttribute(GUIDPropertyParseType guidParseType = GUIDPropertyParseType.Binary)
        {
            this.guidParseType = guidParseType;
        }

        public override object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            if (row.IsDBNull(propertyAttribute.ColumnName))
            {
                if (typeof(Guid?).Equals(propertyAttribute.PropertyInfo.PropertyType))
                    return null;
                return Guid.Empty;
            }

            if (guidParseType == GUIDPropertyParseType.Binary)
                return new Guid(row.GetAllBytes(propertyAttribute.ColumnName));
            if (guidParseType == GUIDPropertyParseType.String)
                return new Guid(row.GetString(row.GetOrdinal(propertyAttribute.ColumnName)));
            throw new NotImplementedException();
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
            var value = propertyAttribute.PropertyInfo.GetValue(entity);

            if (value == null) return DBNull.Value;

            if (guidParseType == GUIDPropertyParseType.Binary)
                return ((Guid) value).ToByteArray();
            if (guidParseType == GUIDPropertyParseType.String)
                return ((Guid) value).ToString();
            throw new NotImplementedException();
        }
    }
}