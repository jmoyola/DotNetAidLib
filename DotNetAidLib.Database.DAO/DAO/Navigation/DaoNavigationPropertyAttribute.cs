using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Navigation
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public abstract class DaoNavigationPropertyAttribute : Attribute
	{
		public static IDictionary<Type, IEnumerable<DaoNavigationPropertyAttribute>> _Instances=new Dictionary<Type, IEnumerable<DaoNavigationPropertyAttribute>>();

		public DaoNavigationPropertyAttribute()
		{}

		internal PropertyInfo PropertyInfo { get; set; }

		public static IEnumerable<DaoNavigationPropertyAttribute> GetNavigatorAttributes(Type entityType)
		{
			if(!_Instances.ContainsKey(entityType)){
				
				List<DaoNavigationPropertyAttribute> ret = new List<DaoNavigationPropertyAttribute>();

				IEnumerable<PropertyInfo> properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(v => v.CanRead && v.CanWrite);
				IEnumerable<PropertyInfo> propsAttributes = properties.Where(v => v.GetCustomAttribute<DaoNavigationPropertyAttribute>() != null);
				foreach (PropertyInfo pi in properties)
				{
					DaoNavigationPropertyAttribute dpa = pi.GetCustomAttribute<DaoNavigationPropertyAttribute>();
					if (dpa != null)
					{
						dpa.PropertyInfo = pi;
						ret.Add(dpa);
					}
				}
				_Instances.Add (entityType, ret);
			}

			return _Instances[entityType];
		}
	}
}
