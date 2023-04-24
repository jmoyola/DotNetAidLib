using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Time;
using DotNetAidLib.Database.DbProviders;

namespace DotNetAidLib.Database
{
    public class AppDbConfig
    {
        private static readonly IDictionary<string, AppDbConfig> instances = new Dictionary<string, AppDbConfig>();

        private AppDbConfig(DBProviderConnector dbConnector)
            : this(dbConnector, "_AppDbConfig")
        {
        }

        private AppDbConfig(DBProviderConnector dbConnector, string configTableName = "_AppDbConfig",
            string keyColumnName = "_Key", string valueColumnName = "_Value")
        {
            Assert.NotNull(dbConnector, nameof(dbConnector));
            Assert.NotNullOrEmpty(configTableName, nameof(configTableName));
            Assert.NotNullOrEmpty(keyColumnName, nameof(keyColumnName));
            Assert.NotNullOrEmpty(valueColumnName, nameof(valueColumnName));

            DbConnection = dbConnector;
            ConfigTableName = configTableName;
            KeyColumnName = keyColumnName;
            ValueColumnName = valueColumnName;

            CreateConfig();
        }

        public DBProviderConnector DbConnection { get; }

        public string ConfigTableName { get; }

        public string KeyColumnName { get; }

        public string ValueColumnName { get; }

        protected void CreateConfig()
        {
            try
            {
                DbConnection.CreateConnection()
                    .CreateCommand()
                    .ExecuteNonQuery(
                        "CREATE TABLE " + ConfigTableName + " (" + KeyColumnName + " VARCHAR(255), " + ValueColumnName +
                        " VARCHAR(1024), PRIMARY KEY (" + KeyColumnName + "));", true);
            }
            catch
            {
            }
        }

        public void Set(string key, object value)
        {
            if (Update(key, value) == 0)
                Add(key, value);
        }

        public void Unset(string key)
        {
            DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteNonQuery("DELETE FROM " + ConfigTableName + " WHERE " + KeyColumnName + "=@key;", true);
        }

        public bool IsSet(string key)
        {
            return DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteScalar<long>(
                    "SELECT COUNT(" + KeyColumnName + ") FROM " + ConfigTableName + " WHERE " + KeyColumnName +
                    "=@key;", true)
                .Value > 0;
        }

        public void Add(string key, object value)
        {
            DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .AddParameter("@value", Serialize(value))
                .ExecuteNonQuery(
                    "INSERT INTO " + ConfigTableName + " (" + KeyColumnName + ", " + ValueColumnName +
                    ") VALUES (@key, @value);", true);
        }

        public int Update(string key, object value)
        {
            return DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .AddParameter("@value", Serialize(value))
                .ExecuteNonQuery(
                    "UPDATE " + ConfigTableName + " SET " + ValueColumnName + "=@value WHERE " + KeyColumnName +
                    "=@key;", true);
        }

        public T Get<T>(string key)
        {
            var v = DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteScalar<string>(
                    "SELECT " + ValueColumnName + " from " + ConfigTableName + " where " + KeyColumnName +
                    "=@key limit 1;", true);

            if (v == null)
                return default;
            return Deserialize<T>(v);
        }

        public T Get<T>(string key, T defaultValue)
        {
            var v = DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteScalar<string>(
                    "SELECT " + ValueColumnName + " from " + ConfigTableName + " where " + KeyColumnName +
                    "=@key limit 1;", true);

            if (v == null)
                return defaultValue;
            return Deserialize<T>(v);
        }

        public IList<T> GetMatch<T>(string keyMatch)
        {
            return DbConnection.CreateConnection()
                .CreateCommand()
                .AddParameter("@key", keyMatch)
                .ExecuteScalarColumn<string>(
                    "SELECT " + ValueColumnName + " FROM " + ConfigTableName + " WHERE " + KeyColumnName +
                    " LIKE '@keyMatch*' ORDER BY " + ValueColumnName + " ASC;", ValueColumnName, true)
                .Select(v => Deserialize<T>(v))
                .ToList();
        }

        private object Serialize<T>(T value)
        {
            object ret;

            var caux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (value == null)
                ret = DBNull.Value;
            else if (value is IList<byte>)
                ret = ((IList<byte>) value).ToHexadecimal();
            else if (value is DateTime)
                ret = ((DateTime) (object) value).ToStringISO8601(true, true);
            else if (value is TimeSpan)
                ret = ((TimeSpan) (object) value).ToStringISO8601(true, true);
            else if (value is bool)
                ret = (bool) (object) value ? "1" : "0";
            else
                ret = value.ToString();

            Thread.CurrentThread.CurrentCulture = caux;
            return ret;
        }

        private T Deserialize<T>(DBNullable<string> value)
        {
            Assert.NotNull(value, nameof(value));
            T ret;

            var caux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (!value.HasValue)
                ret = default;
            else if (typeof(T).Equals(typeof(byte[])))
                ret = (T) (object) value.Value.HexToByteArray();
            else if (typeof(T).Equals(typeof(DateTime)))
                ret = (T) (object) value.Value.ToDateTimeISO8601();
            else if (typeof(T).Equals(typeof(TimeSpan)))
                ret = (T) (object) value.Value.ToTimeSpanISO8601();
            else if (typeof(T).Equals(typeof(bool)))
                ret = (T) (object) (value.Value == "1" ? true : false);
            else
                ret = (T) Convert.ChangeType(value.Value, typeof(T));

            Thread.CurrentThread.CurrentCulture = caux;
            return ret;
        }

        public static AppDbConfig Instance(DBProviderConnector dbConnector)
        {
            return Instance(dbConnector, "_DEFAULT_");
        }

        public static AppDbConfig Instance(DBProviderConnector dbConnector, string instanceName,
            string configTableName = "_AppDbConfig", string keyColumnName = "_Key", string valueColumnName = "_Value")
        {
            Assert.NotNullOrEmpty(instanceName, nameof(instanceName));

            if (!instances.ContainsKey(instanceName))
                instances.Add(instanceName,
                    new AppDbConfig(dbConnector, configTableName, keyColumnName, valueColumnName));

            return instances[instanceName];
        }

        public static AppDbConfig Instance()
        {
            return Instance("_DEFAULT_");
        }

        public static AppDbConfig Instance(string instanceName)
        {
            Assert.NotNullOrEmpty(instanceName, nameof(instanceName));

            if (!instances.ContainsKey(instanceName))
                throw new Exception("Don't exists instance '" + instanceName + "'");

            return instances[instanceName];
        }
    }
}