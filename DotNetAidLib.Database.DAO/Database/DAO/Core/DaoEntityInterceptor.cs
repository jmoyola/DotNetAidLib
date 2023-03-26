﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Database.DAO.Navigation;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoEntityInterceptor<T> : IInterceptor where T : class
    {
        private static DaoEntityInterceptor<T> instance;

        private DaoEntityInterceptor()
        {
        }

        public void Intercept(IInvocation invocation)
        {
            var entityInstance = invocation.InvocationTarget;
            var mustcloseSession = false;
            if (typeof(IDaoInstance).IsAssignableFrom(entityInstance.GetType()))
            {
                var daoInstance = (IDaoInstance) entityInstance;

                // Si se ha invocado el método get de una propiedad
                if (invocation.Method.Name.StartsWith("get_", StringComparison.InvariantCulture)
                    && invocation.Method.Name != "get___DaoInfo")
                    // Si su carga es retardada
                    if (daoInstance.__DaoInfo.LoadType == DaoEntityLoadType.Lazy)
                    {
                        if (!daoInstance.__DaoInfo.Session.IsOpen)
                        {
                            if ((bool) daoInstance.__DaoInfo.Session.Context.Properties.GetValueIfExists(
                                    "lazyAutoCreateSessions", false))
                            {
                                daoInstance.__DaoInfo.Session = daoInstance.__DaoInfo.Session.Context.NewSession();
                                mustcloseSession = true;
                            }
                            else
                            {
                                throw new DaoException("DaoSession is closed in lazy loading entity " +
                                                       entityInstance.GetType().Name + "." + invocation.Method.Name);
                            }
                        }

                        var propertyName = invocation.Method.Name.Substring(4);
                        var propertyInfo = entityInstance.GetType().GetProperty(propertyName);

                        var mi = GetType().GetMethod("Load");
                        mi = mi.MakeGenericMethod(propertyInfo.PropertyType);
                        mi.Invoke(this,
                            new object[]
                            {
                                daoInstance.__DaoInfo.Session, (T) entityInstance, propertyInfo, null,
                                daoInstance.__DaoInfo
                            });

                        if (mustcloseSession)
                            daoInstance.__DaoInfo.Session.Dispose();
                        //this.Load<Object> (daoInstance.__DaoInfo.Session , (T)entityInstance, propertyInfo, null, daoInstance.__DaoInfo);
                    }
            }

            // Seguimos el curso de la invocación
            invocation.Proceed();
        }

        public object Load<S>(DaoSession daoSession, T entityInstance, PropertyInfo propertyInfo,
            Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo = null) where S : class
        {
            var dao = Dao<T>.Instance();

            if (daoSession == null && typeof(IDaoInstance).IsAssignableFrom(entityInstance.GetType()))
                daoSession = ((IDaoInstance) entityInstance).__DaoInfo.Session;

            if (daoSession == null)
                throw new DaoException(
                    "Can't take a valid open daoSession is null and entityInstance is not IDaoInstance");

            if (daoSession != null && !daoSession.IsOpen)
                throw new DaoException("Can't take a valid daoSession opened.");

            if (!typeof(T).IsAssignableFrom(propertyInfo.DeclaringType))
                throw new DaoException("Instance '" + typeof(T).Name + "' dont have a property '" + propertyInfo.Name +
                                       "'");

            var daoPropertyNavigatorAttribute =
                DaoEntityInfo.Instance<T>().DaoPropertyNavigatorAttributes
                    .FirstOrDefault(v => v.PropertyInfo.Name == propertyInfo.Name);

            // y la propiedad tiene un atributo DaoPropertyNavigatorAttribute
            if (daoPropertyNavigatorAttribute != null)
            {
                // Seteamos el valor del navegador a su propiedad
                if (daoPropertyNavigatorAttribute is DaoNavigationPropertyPKAttribute)
                    return dao.SetNavigatorProperty(daoSession, entityInstance,
                        daoPropertyNavigatorAttribute.PropertyInfo.Name, predicate, daoLoadInfo);
                if (daoPropertyNavigatorAttribute is DaoNavigationPropertyFKAttribute)
                    return dao.SetReverseNavigatorProperty(daoSession, entityInstance,
                        daoPropertyNavigatorAttribute.PropertyInfo.Name, predicate, daoLoadInfo);
                return null;
            }

            throw new DaoException("Entity '" + typeof(T).Name +
                                   "' have not a property Navigation Attribute with name '" + propertyInfo.Name + "'");
        }

        public static DaoEntityInterceptor<T> Instance()
        {
            if (instance == null)
                instance = new DaoEntityInterceptor<T>();

            return instance;
        }
    }
}