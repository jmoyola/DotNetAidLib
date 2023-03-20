using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public enum DaoEntityLoadType { None = 0, Explicit = 1, Eager = 2, Lazy = 3, }
    public class DaoContext : IDisposable
    {
        private static IDictionary<String, DaoContext> instances = new Dictionary<string, DaoContext>();
        private DBProviderConnector dbConnector;
        private Properties properties;

        private Component component = new Component ();

        private DaoContext (DBProviderConnector dbConnector, DaoEntityLoadType defaultEntityLoadType = DaoEntityLoadType.Lazy)
        {
            Assert.NotNull( dbConnector, nameof(dbConnector));
            this.dbConnector = dbConnector;

            IDictionary<String, Type> assertTypes = new Dictionary<String, Type> ()
            {
                { "localBinaryFolder", typeof(DirectoryInfo)}
                , { "daoEventHandler", typeof(DaoEventHandler)}
                , { "daoSQLEventHandler", typeof(DaoSQLEventHandler)}
                , { "defaultEntityLoadType", typeof(DaoEntityLoadType)}
                , { "autoOpenSessions", typeof(bool)}
                , { "lazyAutoCreateSessions", typeof(bool)}
            };

            IDictionary<String, Object> defaults = new Dictionary<String, Object> () {
            };

            this.properties = new Properties (assertTypes, defaults, false);

            this.properties.Add("defaultEntityLoadType", defaultEntityLoadType);
            this.properties.Add("autoOpenSessions", false);
            this.properties.Add("lazyAutoCreateSessions", false);
        }

        public DBProviderConnector DBConnector { get =>this.dbConnector; }
        public DaoEntityLoadType DefaultEntityLoadType { get => (DaoEntityLoadType)this.properties["defaultEntityLoadType"]; }

        public DaoSession NewSession (bool openSession=false)
        {
            DaoSession daoSession = new DaoSession (this, this.dbConnector.CreateConnection ());
            if (openSession || (bool)this.properties.GetValueIfExists("autoOpenSessions", false))
                daoSession.Open();

            return daoSession;
        }

        public Properties Properties {
            get { return properties; }
        }

        public const String DEFAULT_INSTANCE_NAME="__DEFAULT__";
        
        public static DaoContext Instance(String instanceName=DEFAULT_INSTANCE_NAME)
        {
            if (!instances.ContainsKey(instanceName))
                throw new DaoException("Instance name '" + instanceName + " don't exists.");

            return instances[instanceName];
        }
        
        public static DaoContext Instance(DBProviderConnector dbConnector, DaoEntityLoadType defaultEntityLoadType = DaoEntityLoadType.Lazy, String instanceName=DEFAULT_INSTANCE_NAME)
        {
            if (instances.ContainsKey(instanceName))
                throw new DaoException("Instance name '" + instanceName + " already exists.");
            instances.Add(instanceName, new DaoContext(dbConnector, defaultEntityLoadType));
            
            return instances[instanceName];
        }

        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        private bool disposed = false;
        protected virtual void Dispose (bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    component.Dispose ();
                }

                disposed = true;
            }
        }

        ~DaoContext ()
        {
            Dispose (false);
        }
    }
}
