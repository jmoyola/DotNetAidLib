using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Database.DbProviders
{
    public class DBProviderConnectorException : Exception
    {
        public DBProviderConnectorException()
        {
        }

        public DBProviderConnectorException(string message) : base(message)
        {
        }

        public DBProviderConnectorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DBProviderConnectorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DBProviderConnector
    {
        public const string DEFAULT_INSTANCE_ID = "__DEFAULT__";

        private static readonly IDictionary<string, DBProviderConnector> instances =
            new Dictionary<string, DBProviderConnector>();

        private readonly string dbConnectionString;

        public DBProviderConnector(string dbProvider, string dbConnectionString, string description = null)
        {
            Assert.NotNullOrEmpty(dbProvider, nameof(dbProvider));
            Assert.NotNullOrEmpty(dbConnectionString, nameof(dbConnectionString));

            DBProvider = dbProvider;
            this.dbConnectionString = dbConnectionString;
            Description = description;

            DBProviderFactory = DbProviderFactories.GetFactory(dbProvider);
        }

        public DBProviderConnector(DbProviderFactory dbProviderFactory,
            DbConnectionStringBuilder dbConnectionStringBuilder, string description = null)
        {
            Assert.NotNull(dbProviderFactory, nameof(dbProviderFactory));
            Assert.NotNull(dbConnectionStringBuilder, nameof(dbConnectionStringBuilder));

            if (!dbProviderFactory.CreateConnectionStringBuilder().GetType()
                    .IsAssignableFrom(dbConnectionStringBuilder.GetType()))
                throw new DBProviderConnectorException("dbConnectionStringBuilder '" +
                                                       dbConnectionStringBuilder.GetType().FullName +
                                                       "' is not ConnectionStringBuilder for dbProviderFactory '" +
                                                       dbProviderFactory.GetType().FullName + "'");

            DBProviderFactory = dbProviderFactory;
            dbConnectionString = dbConnectionStringBuilder.ToString();
            Description = description;

            DBProvider = dbProviderFactory.GetType().FullName;
        }

        public DbProviderFactory DBProviderFactory { get; }

        public string DBProvider { get; }

        public string Description { get; }

        public void Test()
        {
            Helper.TryTimes(() =>
            {
                using (var cnx = CreateConnection())
                {
                    cnx.Open();
                    cnx.Close();
                }
            }, 3, 1000);
        }

        public IDbConnection CreateConnection(bool openConnection = false)
        {
            IDbConnection cnx = DBProviderFactory.CreateConnection();
            cnx.ConnectionString = dbConnectionString;
            if (openConnection)
                cnx.Open();
            return cnx;
        }

        public static DBProviderConnector Instance()
        {
            return Instance(DEFAULT_INSTANCE_ID);
        }

        public static bool ContainsId(string id)
        {
            return instances.ContainsKey(id);
        }

        public static DBProviderConnector Instance(string id)
        {
            if (!instances.ContainsKey(id))
                throw new DBProviderConnectorException("Don't exists instace with id '" + id +
                                                       "' (before must to create it to use).");

            return instances[id];
        }

        public static DBProviderConnector Instance(DbProviderFactory dbProviderFactory,
            DbConnectionStringBuilder dbConnectionStringBuilder, string id = DEFAULT_INSTANCE_ID,
            string description = null)
        {
            if (instances.ContainsKey(id))
                throw new DBProviderConnectorException("Already exists instance with id '" + id + "'.");

            instances.Add(id, new DBProviderConnector(dbProviderFactory, dbConnectionStringBuilder, description));

            return instances[id];
        }

        public static DBProviderConnector Instance(string dbProvider,
            IDictionary<string, object> dbConnectionProperties, string id = DEFAULT_INSTANCE_ID,
            string description = null)
        {
            Assert.NotNullOrEmpty(dbProvider, nameof(dbProvider));
            Assert.NotNull(dbConnectionProperties, nameof(dbConnectionProperties));

            var dbProviderFactory = DbProviderFactories.GetFactory(dbProvider);
            var dbConnectionStringBuilder = dbProviderFactory.CreateConnectionStringBuilder();

            dbConnectionProperties.ToList().ForEach(kv => dbConnectionStringBuilder.Add(kv.Key, kv.Value));

            return Instance(dbProvider, dbConnectionStringBuilder.ToString(), id, description);
        }

        public static DBProviderConnector Instance(string dbProvider, string dbConnectionString,
            string id = DEFAULT_INSTANCE_ID, string description = null)
        {
            if (instances.ContainsKey(id))
                throw new DBProviderConnectorException("Already exists instance with id '" + id + "'.");

            instances.Add(id, new DBProviderConnector(dbProvider, dbConnectionString, description));

            return instances[id];
        }

        public override string ToString()
        {
            return DBProvider + (string.IsNullOrEmpty(Description) ? "" : "(" + Description + ")");
        }
    }
}