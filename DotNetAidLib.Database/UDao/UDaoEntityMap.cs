using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Database.UDao
{
    public interface UDaoEntityMap
    {
        String TableName (Type entityType);
        IList<PropertyInfo> Properties (Type entityType);
        IList<PropertyInfo> PrimaryKeys (Type entityType);
        String ColumnName (Type entityType, PropertyInfo property);
    }

    public class UDaoDefaultEntityMap : UDaoEntityMap
    {
        private static UDaoEntityMap instance = null;

        private UDaoDefaultEntityMap () { }

        public string TableName (Type entityType) => entityType.Name;
        public string ColumnName (Type entityType, PropertyInfo property) => property.Name;
        public IList<PropertyInfo> Properties (Type entityType) =>
            entityType.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .ToList ();
        public IList<PropertyInfo> PrimaryKeys (Type entityType) =>
            entityType.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .Where (v => new String [] { "id", "identity" }.IndexOf(v.Name, StringComparison.InvariantCultureIgnoreCase) > -1)
            .ToList ();

        public static UDaoEntityMap Instance {
            get {
                if (instance == null)
                    instance = new UDaoDefaultEntityMap ();
                return instance;
            }
        }
    }

    public class UDaoAttributesEntityMap : UDaoEntityMap
    {
        private static UDaoEntityMap instance = null;

        private UDaoAttributesEntityMap () { }

        public string TableName (Type entityType)
        {
            UDaoEntityAttribute entityAtt = entityType.GetCustomAttribute<UDaoEntityAttribute> ();

            if (entityAtt == null)
                throw new UDaoException ("Entity '" + entityType.FullName + "' have not UDaoEntityAttribute.");

            return (String.IsNullOrEmpty (entityAtt.TableName) ? entityType.Name : entityAtt.TableName);
        }

        public string ColumnName (Type entityType, PropertyInfo property)
        {
            UDaoPropertyAttribute propertyAtt = property.GetCustomAttribute<UDaoPropertyAttribute> ();

            if (propertyAtt == null)
                throw new UDaoException ("Entity '" + entityType.FullName + "." + property.Name + "' have not UDaoPropertyAttribute.");

            return (String.IsNullOrEmpty (propertyAtt.ColumnName) ? property.Name : propertyAtt.ColumnName);
        }

        public IList<PropertyInfo> Properties (Type entityType)
        {
            IList<PropertyInfo> ret = entityType.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .Where (v => v.GetCustomAttribute<UDaoPropertyAttribute> () != null)
            .OrderBy (v => v.GetCustomAttribute<UDaoPKAttribute> ().Order)
            .ToList ();

            if (ret.Count == 0)
                throw new UDaoException ("Entity '" + entityType.FullName + "' have not any UDaoPKAttribute.");

            return ret;
        }

        public IList<PropertyInfo> PrimaryKeys (Type entityType)
        {
            IList<PropertyInfo> ret = entityType.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .Where (v => v.GetCustomAttribute<UDaoPKAttribute> () != null)
            .OrderBy (v => v.GetCustomAttribute<UDaoPKAttribute> ().Order)
            .ToList ();

            if (ret.Count == 0)
                throw new UDaoException ("Entity '" + entityType.FullName + "' have not any UDaoPKAttribute.");

            return ret;
        }

        public static UDaoEntityMap Instance {
            get {
                if (instance == null)
                    instance = new UDaoAttributesEntityMap ();
                return instance;
            }
        }
    }

}
