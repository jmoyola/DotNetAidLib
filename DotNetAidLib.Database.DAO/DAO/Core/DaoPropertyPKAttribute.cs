using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoPropertyPKAttribute : DaoPropertyOrderedAttribute
	{
		public DaoPropertyPKAttribute()
			: base(null) { }

		public DaoPropertyPKAttribute(String columnName)
			: base(columnName) { }

		public new static IEnumerable<DaoPropertyPKAttribute> GetPropertyAttributes(Type entityType) {
			return DaoPropertyAttribute.GetPropertyAttributes(entityType).OfType<DaoPropertyPKAttribute>();
		}
	}
}
