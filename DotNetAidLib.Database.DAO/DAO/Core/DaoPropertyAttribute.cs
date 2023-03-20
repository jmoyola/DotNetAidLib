using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
    public enum DaoPropertyMode { Auto, Manual}

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DaoPropertyAttribute : Attribute
    {
        public static IDictionary<Type, IEnumerable<DaoPropertyAttribute>> _Instances = new Dictionary<Type, IEnumerable<DaoPropertyAttribute>>();
        private string _columnName;
        private DaoPropertyMode _mode = DaoPropertyMode.Auto;
        private ulong _length;

        public DaoPropertyAttribute()
            : this(null) { }

        public DaoPropertyAttribute(String columnName)
            : this(columnName, 0) { }

        public DaoPropertyAttribute(String columnName, UInt64 length)
            : this(columnName, length, DaoPropertyMode.Auto) { }

        public DaoPropertyAttribute(String columnName, UInt64 length, DaoPropertyMode mode)
        {
            this._columnName = columnName;
            this._length = length;
            this._mode = mode;
        }

        public String ColumnName
        {
            get
            {
                if (String.IsNullOrEmpty(_columnName) && this.PropertyInfo != null)
                    return this.PropertyInfo.Name;
                else
                    return _columnName;
            }
            set
            {
                _columnName = value;
            }
        }

        public DaoPropertyMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        public UInt64 Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        internal PropertyInfo PropertyInfo { get; set; }

        public static IEnumerable<DaoPropertyAttribute> GetPropertyAttributes(Type entityType)
        {
            if (!_Instances.ContainsKey(entityType))
            {
                List<DaoPropertyAttribute> ret = new List<DaoPropertyAttribute>();

                IEnumerable<PropertyInfo> properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(v => v.CanRead && v.CanWrite);
                IEnumerable<PropertyInfo> propsAttributes = properties.Where(v => v.GetCustomAttribute<DaoPropertyAttribute>(true) != null);
                foreach (PropertyInfo pi in properties)
                {
                    DaoPropertyAttribute dpa = pi.GetCustomAttribute<DaoPropertyAttribute>(true);
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
