using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.DynamicProxy;
using System.Linq.Expressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoSession : IDisposable
    {

        private DaoContext daoContext;

        private IDbConnection dbConnection;

        private Component component = new Component ();

        public DaoSession (DaoContext daoContext, IDbConnection dbConnection)
        {
            Assert.NotNull( daoContext, nameof(daoContext));
            Assert.NotNull( dbConnection, nameof(dbConnection));

            this.daoContext = daoContext;
            this.dbConnection = dbConnection;
        }

        public DaoContext Context { get => this.daoContext; }
        public IDbConnection DBConnection { get => this.dbConnection; }

        public DaoTransaction BeginTransaction ()
        {
            return new DaoTransaction (this, this.DBConnection.BeginTransaction ());
        }

        public void Open ()
        {
            this.dbConnection.Open ();
        }

        public void Close ()
        {
            this.dbConnection.Close ();
        }

        public bool IsOpen{ get =>this.dbConnection.State==ConnectionState.Open; }

        public void Dispose ()
        {
            Dispose (true);
        }

        private bool disposed = false;
        protected virtual void Dispose (bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.dbConnection.Dispose ();
                    component.Dispose ();
                }

                disposed = true;
            }
        }

        public T Include<T> (T entity, DaoLoadInfo daoLoadInfo = null) where T : class
        {

            Assert.NotNull( entity, nameof(entity));
            if (daoLoadInfo == null)
                daoLoadInfo = daoLoadInfo = new DaoLoadInfo(this.Context.DefaultEntityLoadType);

            // Si ya es instancia de Dao, se devuelve
            if (typeof (IDaoInstance).IsAssignableFrom (entity.GetType ())) {
                IDaoInstance entityInfo = (IDaoInstance)entity;
                entityInfo.__DaoInfo.LoadType = daoLoadInfo.LoadType;
                entityInfo.__DaoInfo.ChildrenNavigators = daoLoadInfo.ChildrenNavigators;
                return entity;
            } else {

                ProxyGenerationOptions proxyOptions = new ProxyGenerationOptions ();
                IDaoInstance daoInformer = new DaoInstance (this);
                daoInformer.__DaoInfo.LoadType = daoLoadInfo.LoadType;
                daoInformer.__DaoInfo.ChildrenNavigators = daoLoadInfo.ChildrenNavigators;
                proxyOptions.AddMixinInstance (daoInformer);

                return (T)ProxyGeneratorFactory.Instance ().CreateClassProxyWithTarget<T> (entity, proxyOptions, DaoEntityInterceptor<T>.Instance ());
            }
        }

        public T Exclude<T> (T entity) where T : class
        {
            Assert.NotNull( entity, nameof(entity));

            // Si es instancia de Dao, se devuelve
            if (typeof (IDaoInstance).IsAssignableFrom (entity.GetType ())) {
                IDaoInstance entityInfo = (IDaoInstance)entity;
                entityInfo.__DaoInfo.LoadType = DaoEntityLoadType.None;
            }

            return entity;
        }

        public S Load<T, S> (T entity, Expression<Func<T, S>> property, Expression<Func<S, bool>> propertyPredicate = null, DaoLoadInfo daoLoadInfo= null) where T : class where S : class => Dao<T>.Instance ().Load(this, entity, property, propertyPredicate);
        public IEnumerable<S> LoadCollection<T, S> (T entity, Expression<Func<T, IEnumerable<S>>> property, Expression<Func<S, bool>> propertyPredicate = null, DaoLoadInfo daoLoadInfo = null) where T : class where S : class => Dao<T>.Instance ().LoadCollection (this, entity, property, propertyPredicate);
        public int Add<T> (T entity) where T : class => Dao<T>.Instance ().Add (this, entity);
        public int Update<T> (T entity) where T : class => Dao<T>.Instance ().Update (this, entity);
        public int AddOrUpdate<T> (T entity) where T : class => Dao<T>.Instance ().AddOrUpdate (this, entity);
        public int UpdateWhere<T> (IDictionary<String, Object> asignationProperties, IDictionary<String, Object> filterProperties) where T : class => Dao<T>.Instance ().UpdateWhere (this, asignationProperties, filterProperties);
        public int Remove<T> (T entity) where T : class => Dao<T>.Instance ().Remove (this, entity);
        public int RemoveWhere<T> (IDictionary<String, Object> filterProperties) where T : class => Dao<T>.Instance ().RemoveWhere (this, filterProperties);
        public int RemoveWhere<T> (Expression<Func<T, bool>> predicate) where T : class => Dao<T>.Instance ().RemoveWhere (this, predicate);
        public T FirstOrDefault<T> (Object primaryKeys, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            if (!primaryKeys.GetType ().IsAnonymous())
                throw new DaoException ("Only anonymous instances are allowed for '" + nameof (primaryKeys) + "' parameter.");

            return this.FirstOrDefault<T> (primaryKeys.PropertiesToDictionary (), daoLoadInfo);
        }

        public T FirstOrDefault<T> (DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance ().FirstOrDefault(this, (Dictionary<String, Object>)null, daoLoadInfo);
        public T FirstOrDefault<T> (IDictionary<String, Object> primaryKeys, DaoLoadInfo daoLoadInfo = null) where T : class=> Dao<T>.Instance ().FirstOrDefault (this, primaryKeys, daoLoadInfo);
        public T FirstOrDefault<T> (Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance ().FirstOrDefault (this, predicate, daoLoadInfo);
        public T FirstOrDefault<T>(IDictionary<String, Object> primaryKeys, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance().FirstOrDefault(this, primaryKeys, predicate, daoLoadInfo);
        public T FirstOrDefault<T> (String rawSQLCommand, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance ().FirstOrDefault (this, rawSQLCommand, daoLoadInfo);

        public T LastOrDefault<T>(DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance().LastOrDefault(this, (Dictionary<String, Object>)null, daoLoadInfo);
        public T LastOrDefault<T>(IDictionary<String, Object> primaryKeys, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance().LastOrDefault(this, primaryKeys, daoLoadInfo);
        public T LastOrDefault<T>(Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance().LastOrDefault(this, predicate, daoLoadInfo);
        public T LastOrDefault<T>(IDictionary<String, Object> primaryKeys, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance().LastOrDefault(this, primaryKeys, predicate, daoLoadInfo);
        public T LastOrDefault<T>(String rawSQLCommand, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance().LastOrDefault(this, rawSQLCommand, daoLoadInfo);

        public IList<T> Where<T> (DaoLoadInfo daoLoadInfo = null) where T : class
        {
            return Dao<T>.Instance ().Where (this, (Dictionary<String, Object>)null, daoLoadInfo);
        }

        public IList<T> Where<T> (Object filterProperties, DaoLoadInfo daoLoadInfo = null) where T : class
        {
            if (!filterProperties.GetType ().IsAnonymous ())
                throw new DaoException ("Only anonymous instances are allowed for '" + nameof (filterProperties) + "' parameter.");

            return Dao<T>.Instance ().Where (this, filterProperties.PropertiesToDictionary (), daoLoadInfo);
        }
        public IList<T> Where<T> (Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance ().Where (this, predicate, daoLoadInfo);
        public IList<T> Where<T> (String rawSQLCommand, DaoLoadInfo daoLoadInfo = null) where T : class => Dao<T>.Instance ().Where (this, rawSQLCommand, daoLoadInfo);

        public S StdDev<T,S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where T:class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull( aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance ().Aggregation<S> (this, "STDDEV", aggregationPredicate, predicate);
        }

        public S Variance<T, S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull( aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance ().Aggregation<S> (this, "VARIANCE", aggregationPredicate, predicate);
        }

        public S Sum<T, S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull( aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance ().Aggregation<S> (this, "SUM", aggregationPredicate, predicate);
        }

        public S Avg<T, S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull( aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance ().Aggregation<S> (this, "AVG", aggregationPredicate, predicate);
        }

        public S Min<T, S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull( aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance ().Aggregation<S> (this, "MIN", aggregationPredicate, predicate);
        }

        public S Max<T, S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            Assert.NotNull( aggregationPredicate, nameof(aggregationPredicate));
            return Dao<T>.Instance ().Aggregation<S> (this, "MAX", aggregationPredicate, predicate);
        }

        public UInt64 Count<T, S> (Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate) where T : class where S : IComparable, IConvertible, IEquatable<S>
        {
            return Dao<T>.Instance ().AggregationCount<S> (this, "COUNT", aggregationPredicate, predicate);
        }

        public UInt64 Count<T> (Expression<Func<T, bool>> predicate) where T:class
        {
            return Dao<T>.Instance ().AggregationCount<int> (this, "COUNT", null, predicate);
        }

        public UInt64 Count<T> () where T : class
        {
            return Dao<T>.Instance ().AggregationCount<int> (this, "COUNT", null, (Expression<Func<T, bool>>)null);
        }

        public DaoStreamProperty<T> GetStreamProperty<T> (T entity, DaoPropertyAttribute daoPropertyAttribute) where T:class
        {
            return Dao<T>.Instance ().GetStreamProperty(this, entity, daoPropertyAttribute);
        }

        public DaoStreamProperty<T> GetStreamProperty<T> (T entity, Expression<Func<T, byte []>> propertyExpression) where T : class
        {
            return Dao<T>.Instance ().GetStreamProperty (this, entity, propertyExpression);
        }



    }
}
