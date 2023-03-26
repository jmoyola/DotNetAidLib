using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Database.DAO.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyPKAttribute : DaoPropertyOrderedAttribute
    {
        public DaoPropertyPKAttribute()
            : base(null)
        {
        }

        public DaoPropertyPKAttribute(string columnName)
            : base(columnName)
        {
        }

        public new static IEnumerable<DaoPropertyPKAttribute> GetPropertyAttributes(Type entityType)
        {
            return DaoPropertyAttribute.GetPropertyAttributes(entityType).OfType<DaoPropertyPKAttribute>();
        }
    }
}