using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{

    public enum GUIDPropertyParseType {String, Binary}
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DaoPropertyParserGUIDAttribute : DaoPropertyParserAttribute
	{
        private GUIDPropertyParseType guidParseType = GUIDPropertyParseType.Binary;

        public DaoPropertyParserGUIDAttribute (GUIDPropertyParseType guidParseType=GUIDPropertyParseType.Binary)
        {
            this.guidParseType = guidParseType;
		}

        public override Object Deserialize (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            if (row.IsDBNull (propertyAttribute.ColumnName))
                if (typeof(Nullable<Guid>).Equals(propertyAttribute.PropertyInfo.PropertyType))
                    return null;
                else
                    return Guid.Empty;
            else{
                if (this.guidParseType == GUIDPropertyParseType.Binary)
                    return new Guid(row.GetAllBytes(propertyAttribute.ColumnName));
                else if (this.guidParseType == GUIDPropertyParseType.String)
                    return new Guid(row.GetString(row.GetOrdinal(propertyAttribute.ColumnName)));
                else
                    throw new NotImplementedException();
            }
        }
        public override Object SerializeBeforeInsert (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            return this.ToDatabase(daoSession, entityAttribute, propertyAttribute, entity);
        }

        public override Object SerializeBeforeUpdate (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            return this.ToDatabase(daoSession, entityAttribute, propertyAttribute, entity);

        }

        private Object ToDatabase (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity){
            Object value = propertyAttribute.PropertyInfo.GetValue (entity);

            if (value == null)
                return DBNull.Value;
            else {
                if (this.guidParseType == GUIDPropertyParseType.Binary)
                    return ((Guid)value).ToByteArray ();
                else if (this.guidParseType == GUIDPropertyParseType.String)
                    return ((Guid)value).ToString ();
                else
                    throw new NotImplementedException ();
            }
        }

    }
}
