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
	public class DaoPropertyPKGeneratorSequenceAttribute : DaoPropertyPKGeneratorAttribute
	{
		public DaoPropertyPKGeneratorSequenceAttribute (String sequenceName)
		{
			this.SequenceName = sequenceName;
		}

		public String SequenceName { get; set;}
		public override void PreGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity, IDbConnection cnx) {
		}
		public override void PostGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity, IDbConnection cnx) {
            ISQLParser sqlParser = SQLParserFactory.Instance(cnx);

            Object value = cnx.CreateCommand("SELECT " + sqlParser.SequenceNextValue(this.SequenceName)).ExecuteScalar();
			value = Convert.ChangeType(value, propertyAttribute.PropertyInfo.PropertyType);
			propertyAttribute.PropertyInfo.SetValue(entity, value);
		}

	}
}
