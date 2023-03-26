using System;
using System.Reflection;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public static class DaoHelper
    {
        public static object DaoInstance(Type daoGenericType)
        {
            Assert.NotNull(daoGenericType, nameof(daoGenericType));

            var daoType = typeof(Dao<>).MakeGenericType(daoGenericType);
            var mi = daoType.GetMethod("Instance", BindingFlags.Static | BindingFlags.Public, null, new Type[] { },
                null);
            return mi.Invoke(null, new object[] { });
        }
    }
}