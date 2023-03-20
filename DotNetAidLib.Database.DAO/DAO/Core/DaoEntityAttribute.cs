using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;


namespace DotNetAidLib.Database.DAO.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DaoEntityAttribute : Attribute
	{
		private String _TableName;

		public DaoEntityAttribute()
		{ }

		public DaoEntityAttribute(String tableName)
		{
			this.TableName = tableName;
		}

		public String TableName {
			get {
                if (_TableName == null && this.EntityType!=null)
                    return this.EntityType.Name;
                else
                    return _TableName;
			}

			set {
				_TableName = value;
			}
		}

        internal Type EntityType { get; set; }

        public static DaoEntityAttribute GetAttribute(Type entityType) {
			
			DaoEntityAttribute ret = entityType.GetCustomAttribute<DaoEntityAttribute>();

			if (ret == null)
				throw new DaoException("This type is not a DaoEntity (you must to put DaoEntity attribute in class type)");

			ret.EntityType = entityType;

			return ret;
		}

		public static bool EntityEquals(Object a, Object b) {
			bool ret = false;

			if(!a.Equals(null) && !b.Equals(null)) { // Si no son nulos los dos
				DaoEntityAttribute deaA = DaoEntityAttribute.GetAttribute(a.GetType ());
				DaoEntityAttribute deaB = DaoEntityAttribute.GetAttribute(b.GetType ());
				if(deaA!=null && deaB!=null // Si tienen cada uno un atricuto de Entity y su nombre de tabla es la misma
					&& deaA.TableName==deaB.TableName){
					IList<DaoPropertyPKAttribute> aPks = DaoPropertyPKAttribute.GetPropertyAttributes (a.GetType ()).ToList();
					IList<DaoPropertyPKAttribute> bPks = DaoPropertyPKAttribute.GetPropertyAttributes (b.GetType ()).ToList();
					if (aPks.Count > 0 && aPks.Count == bPks.Count) { // Si tienen la misma cantidad de atributos pk y al menos uno
						ret = true;
						foreach(DaoPropertyPKAttribute aPk in aPks){ // por cada atributo pk
							DaoPropertyPKAttribute bPk = bPks.FirstOrDefault (v=>v.Order==aPk.Order); // Obtenemos el atributo pk de b con mismo orden
							ret = ret && (bPk != null);										// Si hay atributo con mismo orden
							ret = ret && (aPk.ColumnName == bPk.ColumnName);				// Si sus nombres de colunas son iguales
							ret = ret && (aPk.PropertyInfo.Name == bPk.PropertyInfo.Name);	// Si sus propiedades se llaman igual
							ret = ret && (aPk.PropertyInfo.PropertyType == bPk.PropertyInfo.PropertyType);	// Si sus propiedades son del mismo tipo
							ret = ret && (aPk.PropertyInfo.GetValue (a).Equals (bPk.PropertyInfo.GetValue (b)));
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
