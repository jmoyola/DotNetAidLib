using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Navigation;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoEntityInfo
    {
        private static IDictionary<Type, DaoEntityInfo> instances = new Dictionary<Type, DaoEntityInfo>();

        private Type type;
        private DaoEntityAttribute _DaoEntityAttribute;
        private IEnumerable<DaoPropertyAttribute> _DaoPropertyAttributes;
        private IEnumerable<DaoNavigationPropertyAttribute> _DaoPropertyNavigatorAttributes;

        public DaoEntityInfo(Type type)
        {
            this.type = type;
            this._DaoEntityAttribute = DaoEntityAttribute.GetAttribute(type);
            this._DaoPropertyAttributes = DaoPropertyAttribute.GetPropertyAttributes(type);
            this._DaoPropertyNavigatorAttributes = DaoNavigationPropertyAttribute.GetNavigatorAttributes(type);

            if (this._DaoEntityAttribute ==null)
                throw new Exception("Type '" + type.Name + "' have not DaoEntity attribute.");

            if (!this._DaoPropertyAttributes.Any(v => v is DaoPropertyPKAttribute))
                throw new Exception("Entity '" + type.Name + "' missing any primary key in DaoProperty attribute.");
        }

        public DaoEntityAttribute DaoEntityAttribute { get => this._DaoEntityAttribute;}
        public IEnumerable<DaoPropertyAttribute> DaoPropertyAttributes { get => this._DaoPropertyAttributes;}
        public IEnumerable<DaoNavigationPropertyAttribute> DaoPropertyNavigatorAttributes { get => this._DaoPropertyNavigatorAttributes;}

        public String MemberInfoToDaoMember(MemberInfo member)
        {
            // Si la clase que declara el miembro es del tipo de esta clase Dao, se devuelve su columnName
            if (member.DeclaringType.Equals(this.type))
            {
                DaoPropertyAttribute daoProperty = this._DaoPropertyAttributes.FirstOrDefault(v => v.PropertyInfo.Name == member.Name);
                if (daoProperty == null)
                    throw new DaoException("Error in linq predicate: Member '" + member.Name + "' is not marked with [DaoProperty] attribute.");

                return daoProperty.ColumnName;
            }
            else// Si no, se devuelve su name
                return member.Name;
        }

        public static DaoEntityInfo Instance<T>() where T : class{
            if (!instances.ContainsKey(typeof(T)))
                instances.Add(typeof(T), new DaoEntityInfo(typeof(T)));

            return instances[typeof(T)];
        }
    }
}
