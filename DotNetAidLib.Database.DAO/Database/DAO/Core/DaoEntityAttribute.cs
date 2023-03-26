using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DaoEntityAttribute : Attribute
    {
        private string _TableName;

        public DaoEntityAttribute()
        {
        }

        public DaoEntityAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName
        {
            get
            {
                if (_TableName == null && EntityType != null)
                    return EntityType.Name;
                return _TableName;
            }

            set => _TableName = value;
        }

        internal Type EntityType { get; set; }

        public static DaoEntityAttribute GetAttribute(Type entityType)
        {
            var ret = entityType.GetCustomAttribute<DaoEntityAttribute>();

            if (ret == null)
                throw new DaoException(
                    "This type is not a DaoEntity (you must to put DaoEntity attribute in class type)");

            ret.EntityType = entityType;

            return ret;
        }

        public static bool EntityEquals(object a, object b)
        {
            var ret = false;

            if (!a.Equals(null) && !b.Equals(null))
            {
                // Si no son nulos los dos
                var deaA = GetAttribute(a.GetType());
                var deaB = GetAttribute(b.GetType());
                if (deaA != null &&
                    deaB != null // Si tienen cada uno un atricuto de Entity y su nombre de tabla es la misma
                    && deaA.TableName == deaB.TableName)
                {
                    IList<DaoPropertyPKAttribute> aPks = DaoPropertyPKAttribute.GetPropertyAttributes(a.GetType())
                        .ToList();
                    IList<DaoPropertyPKAttribute> bPks = DaoPropertyPKAttribute.GetPropertyAttributes(b.GetType())
                        .ToList();
                    if (aPks.Count > 0 && aPks.Count == bPks.Count)
                    {
                        // Si tienen la misma cantidad de atributos pk y al menos uno
                        ret = true;
                        foreach (var aPk in aPks)
                        {
                            // por cada atributo pk
                            var bPk = bPks.FirstOrDefault(v =>
                                v.Order == aPk.Order); // Obtenemos el atributo pk de b con mismo orden
                            ret = ret && bPk != null; // Si hay atributo con mismo orden
                            ret = ret && aPk.ColumnName == bPk.ColumnName; // Si sus nombres de colunas son iguales
                            ret = ret && aPk.PropertyInfo.Name ==
                                bPk.PropertyInfo.Name; // Si sus propiedades se llaman igual
                            ret = ret && aPk.PropertyInfo.PropertyType ==
                                bPk.PropertyInfo.PropertyType; // Si sus propiedades son del mismo tipo
                            ret = ret && aPk.PropertyInfo.GetValue(a).Equals(bPk.PropertyInfo.GetValue(b));
                            if (!ret)
                                break;
                        }
                    }
                }
            }

            return ret;
        }
    }
}