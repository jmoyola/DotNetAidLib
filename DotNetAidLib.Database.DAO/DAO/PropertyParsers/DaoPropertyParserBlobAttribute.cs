﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DaoPropertyParserBlobAttribute : DaoPropertyParserAttribute
	{
        public DaoPropertyParserBlobAttribute()
		{
		}

        public override Object Deserialize (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            if (row.IsDBNull(propertyAttribute.ColumnName))
                return null;
            else
                return new DaoBlob(row.GetAllBytes(propertyAttribute.ColumnName));
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
            return ((DaoBlob)propertyAttribute.PropertyInfo.GetValue(entity)).Value;
        }

    }
}
