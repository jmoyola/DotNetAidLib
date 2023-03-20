using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoPropertyPKGeneratorGUIDAttribute : DaoPropertyPKGeneratorAttribute
	{
		public DaoPropertyPKGeneratorGUIDAttribute()
		{ }

		public override void PreGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity, IDbConnection cnx) {
            Guid actualguid= (Guid)propertyAttribute.PropertyInfo.GetValue(entity);
            if(Guid.Empty.Equals(actualguid))
                propertyAttribute.PropertyInfo.SetValue (entity, Guid.NewGuid ());
		}
		public override void PostGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity, IDbConnection cnx) { 
		}

	}
}
