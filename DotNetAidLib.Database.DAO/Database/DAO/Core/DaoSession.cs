using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoSession : IDisposable
    {
        private readonly Component component = new Component();

        private bool disposed;

        public DaoSession(DaoContext daoContext, IDbConnection dbConnection)
        {
            Assert.NotNull(daoContext, nameof(daoContext));
            Assert.NotNull(dbConnection, nameof(dbConnection));

            Context = daoContext;
            DBConnection = dbConnection;
        }

        public DaoContext Context { get; }

        public IDbConnection DBConnection { get; }

        public bool IsOpen => DBConnection.State == ConnectionState.Open;

        public void Dispose()
        {
            Dispose(true);
        }

        public DaoTransaction BeginTransaction()
        {
            return new DaoTransaction(this, DBConnection.BeginTransaction());
        }

        public void Open()
        {
            DBConnection.Open();
        }

        public void Close()
        {
            DBConnection.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DBConnection.Dispose();
                    component.Dispose();
                }

                disposed = true;
            }
        }

        public T Include<T>(T entity, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            Assert.NotNull(entity, nameof(entity));
            if (daoLoadInfo == null)
                daoLoadInfo = daoLoadInfo = new DaoLoadInfo(Context.DefaultEntityLoadType);

            // Si ya es instancia de Dao, se devuelve
            if (typeof(IDaoInstance).IsAssignableFrom(entity.GetType()))
            {
                var entityInfo = (IDaoInstance) entity;
                entityInfo.__DaoInfo.LoadType = daoLoadInfo.LoadType;
                entityInfo.__DaoInfo.ChildrenNavigators = daoLoadInfo.ChildrenNavigators;
                return entity;
            }

            var proxyOptions = new ProxyGenerationOptions();
            IDaoInstance daoInformer = new DaoInstance(this);
            daoInformer.__DaoInfo.LoadType = daoLoadInfo.LoadType;
            daoInformer.__DaoInfo.ChildrenNavigators = daoLoadInfo.ChildrenNavigators;
            proxyOptions.AddMixinInstance(daoInformer);

            return ProxyGeneratorFactory.Instance()
                .CreateClassProxyWithTarget(entity, proxyOptions, DaoEntityInterceptor<T>.Instance());
        }

        public T Exclude<T>(T entity) where T : class
        {
            Assert.NotNull(entity, nameof(entity));

            // Si es instancia de Dao, se devuelve
            if (typeof(IDaoInstance).IsAssignableFrom(entity.GetType()))
            {
                var entityInfo = (IDaoInstance) entity;
                entityInfo.__DaoInfo.LoadType = DaoEntityLoadType.None;
            }

            return entity;
        }

        public S Load<T, S>(T entity, Expression<Func<T, S>> property,
            Expression<Func<S, bool>> propertyPredicate = null, DaoLoadInfo daoLoadInfo = null)
            where T : class where S : class
        {
            return Dao<T>.Instance().Load(this, entity, property, propertyPredicate);
        }

        public IEnumerable<S> LoadCollection<T, S>(T entity, Expression<Func<T, IEnumerable<S>>> property,
            Expression<Func<S, bool>> propertyPredicate = null, DaoLoadInfo daoLoadInfo = null)
            where T : class where S : class
        {
            return Dao<T>.Instance().LoadCollection(this, entity, property, propertyPredicate);
        }

        public int Add<T>(T entity) where T : class
        {
            return Dao<T>.Instance().Add(this, entity);
        }

        public int Update<T>(T entity) where T : class
        {
            return Dao<T>.Instance().Update(this, entity);
        }

        public int AddOrUpdate<T>(T entity) where T : class
        {
            return Dao<T>.Instance().AddOrUpdate(this, entity);
        }

        public int UpdateWhere<T>(IDictionary<string, object> asignationProperties,
            IDictionary<string, object> filterProperties) where T : class
        {
            return Dao<T>.Instance().UpdateWhere(this, asignationProperties, filterProperties);
        }

        public int Remove<T>(T entity) where T : class
        {
            return Dao<T>.Instance().Remove(this, entity);
        }

        public int RemoveWhere<T>(IDictionary<string, object> filterProperties) where T : class
        {
            return Dao<T>.Instance().RemoveWhere(this, filterProperties);
        }

        public int RemoveWhere<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return Dao<T>.Instance().RemoveWhere(this, predicate);
        }

        public T FirstOrDefault<T>(object primaryKeys, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            if (!primaryKeys.GetType().IsAnonymous())
                throw new DaoException("Only anonymous instances are allowed for '" + nameof(primaryKeys) +
                                       "' parameter.");

            return FirstOrDefault<T>(primaryKeys.PropertiesToDictionary(), daoLoadInfo);
        }

        public T FirstOrDefault<T>(DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().FirstOrDefault(this, (Dictionary<string, object>) null, daoLoadInfo);
        }

        public T FirstOrDefault<T>(IDictionary<string, object> primaryKeys, DaoLoadInfo daoLoadInfo = null)
            where T : class
        {
            return Dao<T>.Instance().FirstOrDefault(this, primaryKeys, daoLoadInfo);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().FirstOrDefault(this, predicate, daoLoadInfo);
        }

        public T FirstOrDefault<T>(IDictionary<string, object> primaryKeys, Expression<Func<T, bool>> predicate,
            DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().FirstOrDefault(this, primaryKeys, predicate, daoLoadInfo);
        }

        public T FirstOrDefault<T>(string rawSQLCommand, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().FirstOrDefault(this, rawSQLCommand, daoLoadInfo);
        }

        public T LastOrDefault<T>(DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().LastOrDefault(this, (Dictionary<string, object>) null, daoLoadInfo);
        }

        public T LastOrDefault<T>(IDictionary<string, object> primaryKeys, DaoLoadInfo daoLoadInfo = null)
            where T : class
        {
            return Dao<T>.Instance().LastOrDefault(this, primaryKeys, daoLoadInfo);
        }

        public T LastOrDefault<T>(Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().LastOrDefault(this, predicate, daoLoadInfo);
        }

        public T LastOrDefault<T>(IDictionary<string, object> primaryKeys, Expression<Func<T, bool>> predicate,
            DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().LastOrDefault(this, primaryKeys, predicate, daoLoadInfo);
        }

        public T LastOrDefault<T>(string rawSQLCommand, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().LastOrDefault(this, rawSQLCommand, daoLoadInfo);
        }

        public IList<T> Where<T>(DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().Where(this, (Dictionary<string, object>) null, daoLoadInfo);
        }

        public IList<T> Where<T>(object filterProperties, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            if (!filterProperties.GetType().IsAnonymous())
                throw new DaoException("Only anonymous instances are allowed for '" + nameof(filterProperties) +
                                       "' parameter.");

            return Dao<T>.Instance().Where(this, filterProperties.PropertiesToDictionary(), daoLoadInfo);
        }

        public IList<T> Where<T>(Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().Where(this, predicate, daoLoadInfo);
        }

        public IList<T> Where<T>(string rawSQLCommand, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance().Where(this, rawSQLCommand, daoLoadInfo);
        }

        public S StdDev<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull(aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance().Aggregation(this, "STDDEV", aggregationPredicate, predicate);
        }

        public S Variance<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull(aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance().Aggregation(this, "VARIANCE", aggregationPredicate, predicate);
        }

        public S Sum<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull(aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance().Aggregation(this, "SUM", aggregationPredicate, predicate);
        }

        public S Avg<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull(aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance().Aggregation(this, "AVG", aggregationPredicate, predicate);
        }

        public S Min<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull(aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance().Aggregation(this, "MIN", aggregationPredicate, predicate);
        }

        public S Max<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull(aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance().Aggregation(this, "MAX", aggregationPredicate, predicate);
        }

        public ulong Count<T, S>(Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate)
            where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            return Dao<T>.Instance().AggregationCount(this, "COUNT", aggregationPredicate, predicate);
        }

        public ulong Count<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return Dao<T>.Instance().AggregationCount<int>(this, "COUNT", null, predicate);
        }

        public ulong Count<T>() where T : class
        {
            return Dao<T>.Instance().AggregationCount<int>(this, "COUNT", null);
        }

        public DaoStreamProperty<T> GetStreamProperty<T>(T entity, DaoPropertyAttribute daoPropertyAttribute)
            where T : class
        {
            return Dao<T>.Instance().GetStreamProperty(this, entity, daoPropertyAttribute);
        }

        public DaoStreamProperty<T> GetStreamProperty<T>(T entity, Expression<Func<T, byte[]>> propertyExpression)
            where T : class
        {
            return Dao<T>.Instance().GetStreamProperty(this, entity, propertyExpression);
        }
    }
}