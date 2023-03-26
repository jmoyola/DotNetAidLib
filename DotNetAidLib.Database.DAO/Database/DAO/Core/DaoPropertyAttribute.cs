using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
    public enum DaoPropertyMode
    {
        Auto,
        Manual
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyAttribute : Attribute
    {
        public static IDictionary<Type, IEnumerable<DaoPropertyAttribute>> _Instances =
            new Dictionary<Type, IEnumerable<DaoPropertyAttribute>>();

        private string _columnName;

        public DaoPropertyAttribute()
            : this(null)
        {
        }

        public DaoPropertyAttribute(string columnName)
            : this(columnName, 0)
        {
        }

        public DaoPropertyAttribute(string columnName, ulong length)
            : this(columnName, length, DaoPropertyMode.Auto)
        {
        }

        public DaoPropertyAttribute(string columnName, ulong length, DaoPropertyMode mode)
        {
            _columnName = columnName;
            Length = length;
            Mode = mode;
        }

        public string ColumnName
        {
            get
            {
                if (string.IsNullOrEmpty(_columnName) && PropertyInfo != null)
                    return PropertyInfo.Name;
                return _columnName;
            }
            set => _columnName = value;
        }

        public DaoPropertyMode Mode { get; set; } = DaoPropertyMode.Auto;

        public ulong Length { get; set; }

        internal PropertyInfo PropertyInfo { get; set; }

        public static IEnumerable<DaoPropertyAttribute> GetPropertyAttributes(Type entityType)
        {
            if (!_Instances.ContainsKey(entityType))
            {
                var ret = new List<DaoPropertyAttribute>();

                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(v => v.CanRead && v.CanWrite);
                var propsAttributes = properties.Where(v => v.GetCustomAttribute<DaoPropertyAttribute>(true) != null);
                foreach (var pi in properties)
                {
                    var dpa = pi.GetCustomAttribute<DaoPropertyAttribute>(true);
                    if (dpa != null)
                    {
                        dpa.PropertyInfo = pi;
                        if (dpa.ColumnName == null)
                            dpa.ColumnName = pi.Name;
                        ret.Add(dpa);
                    }
                }

                _Instances.Add(entityType, ret);
            }

            return _Instances[entityType];
        }
    }
}