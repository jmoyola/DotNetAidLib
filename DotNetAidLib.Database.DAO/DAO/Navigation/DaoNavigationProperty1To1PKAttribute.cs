using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Core;
                 
namespace DotNetAidLib.Database.DAO.Navigation
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoNavigationProperty1To1PKAttribute : DaoNavigationPropertyPKAttribute
	{
		public DaoNavigationProperty1To1PKAttribute()
			: base() { }
	}
}
