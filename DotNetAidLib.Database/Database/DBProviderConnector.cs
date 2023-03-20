using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Database
{
    public class DBProviderConnectorException : Exception
    {
        public DBProviderConnectorException(){}
        public DBProviderConnectorException(string message) : base(message){}
        public DBProviderConnectorException(string message, Exception innerException) : base(message, innerException){}
        protected DBProviderConnectorException(SerializationInfo info, StreamingContext context) : base(info, context){}
    }

    public class DBProviderConnector
    {
        private static IDictionary<String, DBProviderConnector> instances = new Dictionary<String, DBProviderConnector>();

        private DbProviderFactory dbProviderFactory;

        private String dbProvider;
        private String dbConnectionString;
        private String description;

        public DBProviderConnector(String dbProvider, String dbConnectionString, String description=null)
        {
            Assert.NotNullOrEmpty( dbProvider, nameof(dbProvider));
            Assert.NotNullOrEmpty( dbConnectionString, nameof(dbConnectionString));

            this.dbProvider = dbProvider;
            this.dbConnectionString = dbConnectionString;
            this.description = description;

            this.dbProviderFactory = DbProviderFactories.GetFactory(dbProvider);
        }

        public DBProviderConnector(DbProviderFactory dbProviderFactory, DbConnectionStringBuilder dbConnectionStringBuilder, String description=null)
        {
            Assert.NotNull( dbProviderFactory, nameof(dbProviderFactory));
            Assert.NotNull( dbConnectionStringBuilder, nameof(dbConnectionStringBuilder));

            if(!dbProviderFactory.CreateConnectionStringBuilder().GetType().IsAssignableFrom(dbConnectionStringBuilder.GetType()))
                throw new DBProviderConnectorException("dbConnectionStringBuilder '" + dbConnectionStringBuilder.GetType().FullName + "' is not ConnectionStringBuilder for dbProviderFactory '" + dbProviderFactory.GetType().FullName + "'");

            this.dbProviderFactory = dbProviderFactory;
            this.dbConnectionString = dbConnectionStringBuilder.ToString();
            this.description = description;

            this.dbProvider = dbProviderFactory.GetType().FullName;
        }

        public DbProviderFactory DBProviderFactory { get => this.dbProviderFactory; }
        public string DBProvider { get => dbProvider; }
        public string Description { get => description; }

        public void Test() {
            Helper.TryTimes(() => {
                using (IDbConnection cnx = this.CreateConnection()) {
                    cnx.Open();
                    cnx.Close();
                }
            }, 3, 1000);
        }

        public IDbConnection CreateConnection(bool openConnection=false)
        {
            IDbConnection cnx = this.dbProviderFactory.CreateConnection();
            cnx.ConnectionString = this.dbConnectionString;
            if(openConnection)
                cnx.Open();
            return cnx;
        }

        public const string DEFAULT_INSTANCE_ID = "__DEFAULT__";

        public static DBProviderConnector Instance() {
            return Instance(DEFAULT_INSTANCE_ID);
        }

        public static bool ContainsId(String id)
        {
            return instances.ContainsKey(id);
        }

        public static DBProviderConnector Instance(String id)
        {
            if (!instances.ContainsKey(id))
                throw new DBProviderConnectorException("Don't exists instace with id '" + id + "' (before must to create it to use).");

            return instances[id];
        }

        public static DBProviderConnector Instance(DbProviderFactory dbProviderFactory, DbConnectionStringBuilder dbConnectionStringBuilder, String id = DEFAULT_INSTANCE_ID, String description = null)
        {
            if (instances.ContainsKey(id))
                throw new DBProviderConnectorException("Already exists instance with id '" + id + "'.");

            instances.Add(id, new DBProviderConnector(dbProviderFactory, dbConnectionStringBuilder, description));

            return instances[id];
        }

        public static DBProviderConnector Instance(String dbProvider, IDictionary <String, Object> dbConnectionProperties, String id = DEFAULT_INSTANCE_ID, String description = null) {
            Assert.NotNullOrEmpty( dbProvider, nameof(dbProvider));
            Assert.NotNull( dbConnectionProperties, nameof(dbConnectionProperties));

            DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory(dbProvider);
            DbConnectionStringBuilder dbConnectionStringBuilder = dbProviderFactory.CreateConnectionStringBuilder();

            dbConnectionProperties.ToList().ForEach(kv=> dbConnectionStringBuilder.Add(kv.Key, kv.Value ));

            return Instance(dbProvider, dbConnectionStringBuilder.ToString(), id, description);
        }

        public static DBProviderConnector Instance(String dbProvider, String dbConnectionString, String id= DEFAULT_INSTANCE_ID, String description=null) {
            if (instances.ContainsKey(id))
                throw new DBProviderConnectorException("Already exists instance with id '" + id + "'.");

            instances.Add(id, new DBProviderConnector(dbProvider, dbConnectionString, description));

            return instances[id];
        }
        
        public override string ToString(){
            return this.dbProvider + (string.IsNullOrEmpty (this.description)?"": "(" + this.description + ")");
        }
    }
}
