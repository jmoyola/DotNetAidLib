using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoPropertyFKAttribute : DaoPropertyOrderedAttribute
	{
		public DaoPropertyFKAttribute()
			: base(null) { }

		public DaoPropertyFKAttribute(String columnName)
			: base(columnName) { }

		public new static IEnumerable<DaoPropertyFKAttribute> GetPropertyAttributes(Type entityType)
		{
			return DaoPropertyAttribute.GetPropertyAttributes(entityType).OfType<DaoPropertyFKAttribute>();
		}
	}
}
