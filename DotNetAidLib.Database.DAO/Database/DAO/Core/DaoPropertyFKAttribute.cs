using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Database.DAO.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyFKAttribute : DaoPropertyOrderedAttribute
    {
        public DaoPropertyFKAttribute()
            : base(null)
        {
        }

        public DaoPropertyFKAttribute(string columnName)
            : base(columnName)
        {
        }

        public new static IEnumerable<DaoPropertyFKAttribute> GetPropertyAttributes(Type entityType)
        {
            return DaoPropertyAttribute.GetPropertyAttributes(entityType).OfType<DaoPropertyFKAttribute>();
        }
    }
}