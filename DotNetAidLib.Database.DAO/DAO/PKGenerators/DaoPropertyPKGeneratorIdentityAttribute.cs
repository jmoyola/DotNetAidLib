using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Database.DAO.Core;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoPropertyPKGeneratorIdentityAttribute : DaoPropertyPKGeneratorAttribute
	{
		public DaoPropertyPKGeneratorIdentityAttribute()
		{ }

		public override void PreGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, object entity, IDbConnection cnx)
		{
		}

		public override void PostGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, object entity, IDbConnection cnx)
		{
            ISQLParser sqlParser = SQLParserFactory.Instance(cnx);
            String lii = sqlParser.LastInsertId();
            Object value = null;

            value = cnx.CreateCommand("SELECT " + lii).ExecuteScalar();


			value = Convert.ChangeType(value, propertyAttribute.PropertyInfo.PropertyType);
			propertyAttribute.PropertyInfo.SetValue (entity, value);
		}
	}
}
