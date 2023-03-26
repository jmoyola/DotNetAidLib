using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.DbProviders;

namespace DotNetAidLib.Database.DAO.Core
{
    public enum DaoEntityLoadType
    {
        None = 0,
        Explicit = 1,
        Eager = 2,
        Lazy = 3
    }

    public class DaoContext : IDisposable
    {
        public const string DEFAULT_INSTANCE_NAME = "__DEFAULT__";
        private static readonly IDictionary<string, DaoContext> instances = new Dictionary<string, DaoContext>();

        private readonly Component component = new Component();

        private bool disposed;

        private DaoContext(DBProviderConnector dbConnector,
            DaoEntityLoadType defaultEntityLoadType = DaoEntityLoadType.Lazy)
        {
            Assert.NotNull(dbConnector, nameof(dbConnector));
            DBConnector = dbConnector;

            IDictionary<string, Type> assertTypes = new Dictionary<string, Type>
            {
                {"localBinaryFolder", typeof(DirectoryInfo)}, {"daoEventHandler", typeof(DaoEventHandler)},
                {"daoSQLEventHandler", typeof(DaoSQLEventHandler)},
                {"defaultEntityLoadType", typeof(DaoEntityLoadType)}, {"autoOpenSessions", typeof(bool)},
                {"lazyAutoCreateSessions", typeof(bool)}
            };

            IDictionary<string, object> defaults = new Dictionary<string, object>();

            Properties = new Properties(assertTypes, defaults, false);

            Properties.Add("defaultEntityLoadType", defaultEntityLoadType);
            Properties.Add("autoOpenSessions", false);
            Properties.Add("lazyAutoCreateSessions", false);
        }

        public DBProviderConnector DBConnector { get; }

        public DaoEntityLoadType DefaultEntityLoadType => (DaoEntityLoadType) Properties["defaultEntityLoadType"];

        public Properties Properties { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public DaoSession NewSession(bool openSession = false)
        {
            var daoSession = new DaoSession(this, DBConnector.CreateConnection());
            if (openSession || (bool) Properties.GetValueIfExists("autoOpenSessions", false))
                daoSession.Open();

            return daoSession;
        }

        public static DaoContext Instance(string instanceName = DEFAULT_INSTANCE_NAME)
        {
            if (!instances.ContainsKey(instanceName))
                throw new DaoException("Instance name '" + instanceName + " don't exists.");

            return instances[instanceName];
        }

        public static DaoContext Instance(DBProviderConnector dbConnector,
            DaoEntityLoadType defaultEntityLoadType = DaoEntityLoadType.Lazy,
            string instanceName = DEFAULT_INSTANCE_NAME)
        {
            if (instances.ContainsKey(instanceName))
                throw new DaoException("Instance name '" + instanceName + " already exists.");
            instances.Add(instanceName, new DaoContext(dbConnector, defaultEntityLoadType));

            return instances[instanceName];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) component.Dispose();

                disposed = true;
            }
        }

        ~DaoContext()
        {
            Dispose(false);
        }
    }
}