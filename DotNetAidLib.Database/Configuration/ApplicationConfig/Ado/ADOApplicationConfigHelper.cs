using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DotNetAidLib.Core.Cryptography;
using DotNetAidLib.Core.Develop;
using System.Diagnostics;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Database;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Helpers
{
    public class ADOApplicationConfigHelper
    {
        private String dbProviderFactoryKey = "dbProviderFactory";
        private String dbConnectionStringPropertiesKey = "dbConnectionStringProperties";

        private String _DefaultDBProviderFactory = "MySql.Data.MySqlClient";
        private IDictionary<String, Object> _DefaultDBConnectionStringProperties
            = new Dictionary<String, Object>() {
                    {"server", "127.0.0.1"},
                    {"user id", "user"},
                    {"password", (StringCrypt)"password"},
                    {"port", 3306},
                    {"database", "database"},
                };

       
        public ADOApplicationConfigHelper(){

        }

        public ADOApplicationConfigHelper(String defaultDBProviderFactory, IDictionary<String, Object> defaultDBConnectionStringProperties)
        {
            this.DefaultDBProviderFactory = defaultDBProviderFactory;
            this.DefaultDBConnectionStringProperties = defaultDBConnectionStringProperties;
        }
        public String DBProviderFactoryKey
        {
            get { return dbProviderFactoryKey; }
            set {
                Assert.NotNullOrEmpty( value, nameof(value));
                dbProviderFactoryKey = value;
            }
        }
        public String DBConnectionStringPropertiesKey
        {
            get { return dbConnectionStringPropertiesKey; }
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                dbConnectionStringPropertiesKey = value;
            }
        }

        public String DefaultDBProviderFactory
        {
            get { return _DefaultDBProviderFactory; }
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _DefaultDBProviderFactory = value;
            }
        }

        public IDictionary<String, Object> DefaultDBConnectionStringProperties
        {
            get { return _DefaultDBConnectionStringProperties; }
            set {
                Assert.NotNullOrEmpty( value, nameof(value));
                this._DefaultDBConnectionStringProperties=value;
            }
        }

        public DBProviderConnector GetConnector(IApplicationConfigGroup dbConnectorConfigGroup, String dbConnectorKey= DBProviderConnector.DEFAULT_INSTANCE_ID)
        {
            // Proveedor de base de datos que utilizaremos
            IApplicationConfigGroup dbConnectionStringPropertiesGroup;

            try
            {
                String sDBProviderFactory = dbConnectorConfigGroup.GetConfiguration<String>(this.DBProviderFactoryKey).Value;
                Debug.WriteLine("DB Data Provider: " + sDBProviderFactory);
                DbProviderFactory dbProviderFactory = DotNetAidLib.Database.DbProviderFactories.GetFactory(sDBProviderFactory);
                // ConnectionString que utilizaremos
                DbConnectionStringBuilder dbConnectionStringBuilder = dbProviderFactory.CreateConnectionStringBuilder();

                dbConnectionStringPropertiesGroup = dbConnectorConfigGroup.GetGroup(this.DBConnectionStringPropertiesKey);
                Debug.WriteLine("DBConnectionStringProperties:");
                foreach (KeyValuePair<string, Type> kv in dbConnectionStringPropertiesGroup.ConfigurationKeys)
                {
                    IConfig<object> config = dbConnectionStringPropertiesGroup.GetConfiguration<Object>(kv.Key);
                    String value = (config.Value == null ? "<NULL>" : config.Value.ToString());
                    if (typeof(StringCrypt).IsAssignableFrom(config.Type))
                    {
                        dbConnectionStringBuilder.Add(config.Key, config.Value.ToString());
                        value = "*****";
                    }
                    else
                        dbConnectionStringBuilder.Add(config.Key, config.Value);

                    Debug.WriteLine(" - " + config.Key + ": " + value);
                }

                return DBProviderConnector.Instance(sDBProviderFactory, dbConnectionStringBuilder.ToString(), dbConnectorKey);
            }
            catch (Exception ex)
            {
                throw new ApplicationConfigHelperException("Error creating dbConnector", ex);
            }
        }

        public void Configure(IApplicationConfigGroup dbConnectorConfigGroup)
        {
            if(!dbConnectorConfigGroup.ConfigurationExist(this.DBProviderFactoryKey))
                dbConnectorConfigGroup.AddConfiguration(this.DBProviderFactoryKey, this.DefaultDBProviderFactory, true);

            if (!dbConnectorConfigGroup.GroupExist(this.DBConnectionStringPropertiesKey)) {
                IApplicationConfigGroup dbConnectionStringPropertiesGroup = dbConnectorConfigGroup.AddGroup(this.DBConnectionStringPropertiesKey);

                foreach (String key in DefaultDBConnectionStringProperties.Keys.ToArray())
                    dbConnectionStringPropertiesGroup.AddConfiguration(key, DefaultDBConnectionStringProperties[key], true);
            }

            dbConnectorConfigGroup.Root.Save();
        }

    }
}
