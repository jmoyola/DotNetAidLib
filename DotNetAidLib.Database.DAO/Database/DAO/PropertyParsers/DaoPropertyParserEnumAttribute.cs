using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    public enum EnumPropertyParseType
    {
        String,
        Integer
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyParserEnumAttribute : DaoPropertyParserAttribute
    {
        private readonly EnumPropertyParseType enumParseType;

        public DaoPropertyParserEnumAttribute(EnumPropertyParseType enumParseType = EnumPropertyParseType.String)
        {
            this.enumParseType = enumParseType;
        }

        public override object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            object ret = null;

            var databaseValue = row.GetValue(propertyAttribute.ColumnName);
            if (databaseValue != DBNull.Value)
            {
                var enumType = propertyAttribute.PropertyInfo.PropertyType;
                if (Nullable.GetUnderlyingType(enumType) != null)
                    enumType = enumType.GenericTypeArguments[0];

                if (databaseValue is string)
                    ret = Enum.Parse(enumType, databaseValue.ToString(), true);
                else
                    ret = Enum.ToObject(enumType, databaseValue);
            }

            return ret;
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
            var objectValue = propertyAttribute.PropertyInfo.GetValue(entity);
            if (objectValue != null)
            {
                if (enumParseType == EnumPropertyParseType.String)
                    return objectValue.ToString();
                return (int) objectValue;
            }

            return null;
        }
    }
}