using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Navigation;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoEntityInfo
    {
        private static readonly IDictionary<Type, DaoEntityInfo> instances = new Dictionary<Type, DaoEntityInfo>();

        private readonly Type type;

        public DaoEntityInfo(Type type)
        {
            this.type = type;
            DaoEntityAttribute = DaoEntityAttribute.GetAttribute(type);
            DaoPropertyAttributes = DaoPropertyAttribute.GetPropertyAttributes(type);
            DaoPropertyNavigatorAttributes = DaoNavigationPropertyAttribute.GetNavigatorAttributes(type);

            if (DaoEntityAttribute == null)
                throw new Exception("Type '" + type.Name + "' have not DaoEntity attribute.");

            if (!DaoPropertyAttributes.Any(v => v is DaoPropertyPKAttribute))
                throw new Exception("Entity '" + type.Name + "' missing any primary key in DaoProperty attribute.");
        }

        public DaoEntityAttribute DaoEntityAttribute { get; }

        public IEnumerable<DaoPropertyAttribute> DaoPropertyAttributes { get; }

        public IEnumerable<DaoNavigationPropertyAttribute> DaoPropertyNavigatorAttributes { get; }

        public string MemberInfoToDaoMember(MemberInfo member)
        {
            // Si la clase que declara el miembro es del tipo de esta clase Dao, se devuelve su columnName
            if (member.DeclaringType.Equals(type))
            {
                var daoProperty = DaoPropertyAttributes.FirstOrDefault(v => v.PropertyInfo.Name == member.Name);
                if (daoProperty == null)
                    throw new DaoException("Error in linq predicate: Member '" + member.Name +
                                           "' is not marked with [DaoProperty] attribute.");

                return daoProperty.ColumnName;
            }

            // Si no, se devuelve su name
            return member.Name;
        }

        public static DaoEntityInfo Instance<T>() where T : class
        {
            if (!instances.ContainsKey(typeof(T)))
                instances.Add(typeof(T), new DaoEntityInfo(typeof(T)));

            return instances[typeof(T)];
        }
    }
}