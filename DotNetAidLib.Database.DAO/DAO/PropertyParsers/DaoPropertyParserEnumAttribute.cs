using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy.Internal;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    public enum EnumPropertyParseType { String, Integer,}
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoPropertyParserEnumAttribute : DaoPropertyParserAttribute
	{
        private EnumPropertyParseType enumParseType;

        public DaoPropertyParserEnumAttribute(EnumPropertyParseType enumParseType= EnumPropertyParseType.String)
		{
            this.enumParseType = enumParseType;
		}

		public override Object Deserialize (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
			Object ret = null;

			Object databaseValue = row.GetValue(propertyAttribute.ColumnName);
			if (databaseValue != DBNull.Value) {
                Type enumType = propertyAttribute.PropertyInfo.PropertyType;
                if (Nullable.GetUnderlyingType(enumType)!=null)
                    enumType = enumType.GenericTypeArguments[0];

                if (databaseValue is String)
					ret = Enum.Parse (enumType, databaseValue.ToString (), true);
				else
					ret = Enum.ToObject (enumType, databaseValue);
			}
			return ret;
		}

		public override Object SerializeBeforeInsert (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            return this.ToDatabase(daoSession, entityAttribute, propertyAttribute, entity);
        }

        public override Object SerializeBeforeUpdate (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            return this.ToDatabase(daoSession, entityAttribute, propertyAttribute, entity);
        }

        private Object ToDatabase (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            Object objectValue = propertyAttribute.PropertyInfo.GetValue(entity);
            if (objectValue != null)
            {
                if (this.enumParseType == EnumPropertyParseType.String)
                    return objectValue.ToString();
                else
                    return (int)objectValue;
            }
            else
                return null;
        }

    }
}
