using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Core;
                 
namespace DotNetAidLib.Database.DAO.Navigation
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public abstract class DaoNavigationPropertyPKAttribute : DaoNavigationPropertyAttribute
	{
		public DaoNavigationPropertyPKAttribute()
			: base() { }		
	}
}
