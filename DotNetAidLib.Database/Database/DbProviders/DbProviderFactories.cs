using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Plugins;

namespace DotNetAidLib.Database.DbProviders
{
    public class DbProviderException : Exception
    {
        public DbProviderException()
        {
        }

        public DbProviderException(string message) : base(message)
        {
        }

        public DbProviderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DbProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DbProvider
    {
        public DbProvider(string name, string invariant, Type type, string description)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNullOrEmpty(invariant, nameof(invariant));
            Assert.NotNull(type, nameof(type));

            Name = name;
            Invariant = invariant;
            Type = type;
            Description = description;
        }

        public string Name { get; }

        public string Invariant { get; }

        public Type Type { get; }

        public string Description { get; }

        public override string ToString()
        {
            return Invariant + ": " + Name + "(" + Type.AssemblyQualifiedName + ") " + Description;
        }
    }

    public class DbProviderFactories
    {
        private static readonly IDictionary<string, DbProvider> dbProviders = new Dictionary<string, DbProvider>();
        private static readonly object olock = new object();

        static DbProviderFactories()
        {
            AddAppFactory();
        }

        private DbProviderFactories()
        {
        }

        public static DbProviderFactory GetFactory(DbConnection connection)
        {
            Assert.NotNull(connection, nameof(connection));

            lock (olock)
            {
                var pi = connection.GetType().GetProperty("ProviderFactory",
                    BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);
                if (pi != null)
                    return (DbProviderFactory) pi.GetValue(connection);
                return null;
            }
        }

        public static DbProviderFactory GetFactory(DataRow providerRow)
        {
            Assert.NotNull(providerRow, nameof(providerRow));

            lock (olock)
            {
                try
                {
                    Assert.NotNull(providerRow, nameof(providerRow));

                    var AssemblyQualifiedName = providerRow["AssemblyQualifiedName"].ToString();
                    var DbProviderFactoryType = Type.GetType(AssemblyQualifiedName);

                    return GetInstance(DbProviderFactoryType);
                }
                catch (Exception ex)
                {
                    throw new DbProviderException("Error creating dataprovider from providerRow", ex);
                }
            }
        }

        public static bool ContainsFactory(string providerInvariantName)
        {
            Assert.NotNullOrEmpty(providerInvariantName, nameof(providerInvariantName));

            lock (olock)
            {
                return dbProviders.ContainsKey(providerInvariantName);
            }
        }

        public static DbProviderFactory GetFactory(string providerInvariantName, bool errorIfNotExists = true)
        {
            Assert.NotNullOrEmpty(providerInvariantName, nameof(providerInvariantName));

            lock (olock)
            {
                try
                {
                    if (!dbProviders.ContainsKey(providerInvariantName))
                    {
                        if (errorIfNotExists)
                            throw new Exception("Can't find any dataprovider from providerInvariantName '" +
                                                providerInvariantName + "'");
                        return null;
                    }

                    var dr = dbProviders[providerInvariantName];

                    return GetInstance(dr.Type);
                }
                catch (Exception ex)
                {
                    throw new DbProviderException("Error getting dataprovider from invariant name", ex);
                }
            }
        }

        private static DbProviderFactory GetInstance(Type type)
        {
            DbProviderFactory ret = null;
            var instanceMethod = type.GetMethod("Instance", BindingFlags.Static | BindingFlags.Public, null,
                new Type[0], null);
            var instanceProperty = type.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            var instanceField = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (instanceMethod != null)
                ret = (DbProviderFactory) instanceMethod.Invoke(null, new object[0]);

            if (ret == null && instanceProperty != null)
                ret = (DbProviderFactory) instanceProperty.GetValue(null, new object[0]);

            if (ret == null && instanceField != null)
                ret = (DbProviderFactory) instanceField.GetValue(null);

            if (ret == null)
                return (DbProviderFactory) Activator.CreateInstance(type, true);

            if (ret == null)
                throw new DbProviderException(
                    "Can't get Instance property, Instance member or Default constructor to create instance of type '" +
                    type.Name + "'.");

            return ret;
        }

        public static IEnumerable<DbProvider> GetFactoryClasses()
        {
            lock (olock)
            {
                return dbProviders.Values.ToList().AsReadOnly();
            }
        }

        public static void AddFactory(DirectoryInfo directory)
        {
            AddFactory(directory, "*.dll");
        }

        public static void AddFactory(DirectoryInfo directory, string filePattern)
        {
            Assert.NotNull(directory, nameof(directory));
            Assert.NotNullOrEmpty(filePattern, nameof(filePattern));

            lock (olock)
            {
                var plugings = Plugins.GetPluginsTypesInFolder<DbProviderFactory>(directory);
                foreach (var pluging in plugings)
                {
                    var dbName = pluging.Name.Replace("Factory", "");
                    AddFactory(dbName,
                        pluging.Namespace,
                        ".Net Framework Data Provider for " + dbName,
                        pluging.AssemblyQualifiedName);
                }
            }
        }

        public static void AddAppFactory()
        {
            var baseDir = Helper.BaseDirectory();
            var configFiles = baseDir
                .GetFiles()
                .AsEnumerable()
                .Where(v => v.Name.Equals("app.config", StringComparison.InvariantCultureIgnoreCase)
                            || v.Name.Equals("web.config", StringComparison.InvariantCultureIgnoreCase));

            configFiles.ToList().ForEach(v => AddFactory(v));
        }

        public static void AddFactory(FileInfo xmlConfigFile)
        {
            lock (olock)
            {
                try
                {
                    Assert.Exists(xmlConfigFile, nameof(xmlConfigFile));

                    var dom = new XmlDocument();
                    dom.Load(xmlConfigFile.FullName);
                    var dpfs = dom.GetElementsByTagName("DbProviderFactories");
                    foreach (XmlNode dpf in dpfs)
                    foreach (XmlNode n in dpf.ChildNodes)
                    {
                        string name, invariant, description, type;
                        if (n.Name.Equals("remove", StringComparison.InvariantCultureIgnoreCase)
                            && n.Attributes["invariant"] != null)
                        {
                            invariant = n.Attributes["invariant"].Value;
                            RemoveFactory(invariant);
                        }
                        else if (n.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase)
                                 && n.Attributes["name"] != null
                                 && n.Attributes["invariant"] != null
                                 && n.Attributes["type"] != null)
                        {
                            name = n.Attributes["name"].Value;
                            invariant = n.Attributes["invariant"].Value;
                            if (n.Attributes["description"] == null)
                                description = name + " provider";
                            else
                                description = n.Attributes["description"].Value;
                            type = n.Attributes["type"].Value;
                            AddFactory(name, invariant, description, type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DbProviderException("Error adding dataproviders from xml config file", ex);
                }
            }
        }

        public static void AddFactory(string name, string invariantName, string description,
            string assemblyQualifiedName)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNullOrEmpty(invariantName, nameof(invariantName));
            Assert.NotNullOrEmpty(assemblyQualifiedName, nameof(assemblyQualifiedName));

            lock (olock)
            {
                if (dbProviders.ContainsKey(invariantName))
                    return;

                // 3. Add a new entry in the configuration (replace the values with ones for your provider).
                var dbProvider = new DbProvider(description, invariantName, Type.GetType(assemblyQualifiedName),
                    description);

                dbProviders.Add(dbProvider.Invariant, dbProvider);
            }
        }

        public static void RemoveFactory(string invariantName)
        {
            Assert.NotNullOrEmpty(invariantName, nameof(invariantName));

            lock (olock)
            {
                if (!dbProviders.ContainsKey(invariantName))
                    return;

                dbProviders.Remove(invariantName);
            }
        }

        /*

        dataRow["Name"] = "SqlClient Data Provider";
            dataRow["InvariantName"] = typeof(SqlConnection).Namespace.ToString();
            dataRow["Description"] = ".Net Framework Data Provider for SqlServer";
            dataRow["AssemblyQualifiedName"] = typeof(SqlConnection).AssemblyQualifiedName;
        */
    }
}