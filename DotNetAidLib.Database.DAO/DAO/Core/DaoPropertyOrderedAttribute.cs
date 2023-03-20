using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public abstract class DaoPropertyOrderedAttribute : DaoPropertyAttribute
	{
		public DaoPropertyOrderedAttribute()
			: base(null) { }

		public DaoPropertyOrderedAttribute(String columnName)
			: base(columnName) { }

		public UInt32 Order { get; set;}
	}
}
