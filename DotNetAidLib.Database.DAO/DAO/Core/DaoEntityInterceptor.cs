using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Database.DAO.Navigation;

namespace DotNetAidLib.Database.DAO.Core
{
	public class DaoEntityInterceptor<T> : IInterceptor where T : class
	{
        private static DaoEntityInterceptor<T> instance;

        private DaoEntityInterceptor () { }

        public void Intercept(IInvocation invocation)
		{
            Object entityInstance = invocation.InvocationTarget;
            bool mustcloseSession = false;
            if (typeof (IDaoInstance).IsAssignableFrom (entityInstance.GetType ())) {
                IDaoInstance daoInstance = (IDaoInstance)entityInstance;

                // Si se ha invocado el método get de una propiedad
                if (invocation.Method.Name.StartsWith ("get_", StringComparison.InvariantCulture)
                    && invocation.Method.Name!="get___DaoInfo") {
                    // Si su carga es retardada
                    if (daoInstance.__DaoInfo.LoadType == DaoEntityLoadType.Lazy) {
                        if(!daoInstance.__DaoInfo.Session.IsOpen) {
                            if ((bool)daoInstance.__DaoInfo.Session.Context.Properties.GetValueIfExists("lazyAutoCreateSessions", false)) {
                                daoInstance.__DaoInfo.Session = daoInstance.__DaoInfo.Session.Context.NewSession();
                                mustcloseSession = true;
                            }
                            else
                                throw new DaoException("DaoSession is closed in lazy loading entity " + entityInstance.GetType().Name + "." + invocation.Method.Name);
                        }
                        String propertyName = invocation.Method.Name.Substring (4);
                        PropertyInfo propertyInfo = entityInstance.GetType().GetProperty(propertyName);

                        MethodInfo mi = this.GetType().GetMethod("Load");
                        mi = mi.MakeGenericMethod(propertyInfo.PropertyType);
                        mi.Invoke(this, new object[] { daoInstance.__DaoInfo.Session, (T)entityInstance, propertyInfo, null, daoInstance.__DaoInfo});

                        if (mustcloseSession)
                            daoInstance.__DaoInfo.Session.Dispose();

                        //this.Load<Object> (daoInstance.__DaoInfo.Session , (T)entityInstance, propertyInfo, null, daoInstance.__DaoInfo);
                    }
                }
            }
            // Seguimos el curso de la invocación
            invocation.Proceed ();
		}

        public Object Load<S> (DaoSession daoSession, T entityInstance, PropertyInfo propertyInfo, Expression<Func<S, bool>> predicate=null, DaoLoadInfo daoLoadInfo = null) where S:class {
            Dao<T> dao = Dao<T>.Instance();

            if (daoSession == null && typeof (IDaoInstance).IsAssignableFrom (entityInstance.GetType ()))
                    daoSession = ((IDaoInstance)entityInstance).__DaoInfo.Session;

            if (daoSession == null)
                throw new DaoException ("Can't take a valid open daoSession is null and entityInstance is not IDaoInstance");

            if (daoSession != null && !daoSession.IsOpen)
                throw new DaoException("Can't take a valid daoSession opened.");

            if (!typeof(T).IsAssignableFrom(propertyInfo.DeclaringType))
                throw new DaoException("Instance '" + typeof(T).Name + "' dont have a property '" + propertyInfo.Name + "'");

            DaoNavigationPropertyAttribute daoPropertyNavigatorAttribute =
                        DaoEntityInfo.Instance<T> ().DaoPropertyNavigatorAttributes
                            .FirstOrDefault (v => v.PropertyInfo.Name == propertyInfo.Name);

            // y la propiedad tiene un atributo DaoPropertyNavigatorAttribute
            if (daoPropertyNavigatorAttribute != null) {
                // Seteamos el valor del navegador a su propiedad
                if (daoPropertyNavigatorAttribute is DaoNavigationPropertyPKAttribute)
                    return dao.SetNavigatorProperty (daoSession, entityInstance, daoPropertyNavigatorAttribute.PropertyInfo.Name, predicate, daoLoadInfo);
                else if (daoPropertyNavigatorAttribute is DaoNavigationPropertyFKAttribute)
                    return dao.SetReverseNavigatorProperty (daoSession, entityInstance, daoPropertyNavigatorAttribute.PropertyInfo.Name, predicate, daoLoadInfo);
                else
                    return null;
            }
            else
                throw new DaoException ("Entity '" + typeof(T).Name  + "' have not a property Navigation Attribute with name '" + propertyInfo.Name + "'");
        }

        public static DaoEntityInterceptor<T> Instance() {
            if (instance == null)
                instance = new DaoEntityInterceptor<T> ();

            return instance;
        }
    }
}
