using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Database.UDao
{
    public interface UDaoEntityMap
    {
        string TableName(Type entityType);
        IList<PropertyInfo> Properties(Type entityType);
        IList<PropertyInfo> PrimaryKeys(Type entityType);
        string ColumnName(Type entityType, PropertyInfo property);
    }

    public class UDaoDefaultEntityMap : UDaoEntityMap
    {
        private static UDaoEntityMap instance;

        private UDaoDefaultEntityMap()
        {
        }

        public static UDaoEntityMap Instance
        {
            get
            {
                if (instance == null)
                    instance = new UDaoDefaultEntityMap();
                return instance;
            }
        }

        public string TableName(Type entityType)
        {
            return entityType.Name;
        }

        public string ColumnName(Type entityType, PropertyInfo property)
        {
            return property.Name;
        }

        public IList<PropertyInfo> Properties(Type entityType)
        {
            return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .ToList();
        }

        public IList<PropertyInfo> PrimaryKeys(Type entityType)
        {
            return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(v => new[] {"id", "identity"}.IndexOf(v.Name, StringComparison.InvariantCultureIgnoreCase) > -1)
                .ToList();
        }
    }

    public class UDaoAttributesEntityMap : UDaoEntityMap
    {
        private static UDaoEntityMap instance;

        private UDaoAttributesEntityMap()
        {
        }

        public static UDaoEntityMap Instance
        {
            get
            {
                if (instance == null)
                    instance = new UDaoAttributesEntityMap();
                return instance;
            }
        }

        public string TableName(Type entityType)
        {
            var entityAtt = entityType.GetCustomAttribute<UDaoEntityAttribute>();

            if (entityAtt == null)
                throw new UDaoException("Entity '" + entityType.FullName + "' have not UDaoEntityAttribute.");

            return string.IsNullOrEmpty(entityAtt.TableName) ? entityType.Name : entityAtt.TableName;
        }

        public string ColumnName(Type entityType, PropertyInfo property)
        {
            var propertyAtt = property.GetCustomAttribute<UDaoPropertyAttribute>();

            if (propertyAtt == null)
                throw new UDaoException("Entity '" + entityType.FullName + "." + property.Name +
                                        "' have not UDaoPropertyAttribute.");

            return string.IsNullOrEmpty(propertyAtt.ColumnName) ? property.Name : propertyAtt.ColumnName;
        }

        public IList<PropertyInfo> Properties(Type entityType)
        {
            IList<PropertyInfo> ret = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(v => v.GetCustomAttribute<UDaoPropertyAttribute>() != null)
                .OrderBy(v => v.GetCustomAttribute<UDaoPKAttribute>().Order)
                .ToList();

            if (ret.Count == 0)
                throw new UDaoException("Entity '" + entityType.FullName + "' have not any UDaoPKAttribute.");

            return ret;
        }

        public IList<PropertyInfo> PrimaryKeys(Type entityType)
        {
            IList<PropertyInfo> ret = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(v => v.GetCustomAttribute<UDaoPKAttribute>() != null)
                .OrderBy(v => v.GetCustomAttribute<UDaoPKAttribute>().Order)
                .ToList();

            if (ret.Count == 0)
                throw new UDaoException("Entity '" + entityType.FullName + "' have not any UDaoPKAttribute.");

            return ret;
        }
    }
}