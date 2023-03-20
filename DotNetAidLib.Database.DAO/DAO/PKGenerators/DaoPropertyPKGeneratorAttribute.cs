using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public abstract class DaoPropertyPKGeneratorAttribute : Attribute
	{
		public DaoPropertyPKGeneratorAttribute()
		{ }

		public abstract void PreGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity, IDbConnection cnx);
		public abstract void PostGeneration (DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity, IDbConnection cnx);
	}
}
