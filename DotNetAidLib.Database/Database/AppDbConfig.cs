using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib
{
    public class AppDbConfig
    {
        private static IDictionary<String, AppDbConfig> instances = new Dictionary<String, AppDbConfig> ();
        private IDbConnection dbConnection;

        private String configTableName;
        private String keyColumnName;
        private String valueColumnName;

        private AppDbConfig (IDbConnection dbConnection)
            :this(dbConnection, "_AppDbConfig", "_Key", "_Value") {}

        private AppDbConfig (IDbConnection dbConnection, String configTableName = "_AppDbConfig", String keyColumnName = "_Key", String valueColumnName = "_Value"){
            Assert.NotNull ( dbConnection, nameof(dbConnection));
            Assert.NotNullOrEmpty ( configTableName, nameof(configTableName));
            Assert.NotNullOrEmpty ( keyColumnName, nameof(keyColumnName));
            Assert.NotNullOrEmpty ( valueColumnName, nameof(valueColumnName));

            this.dbConnection = dbConnection;
            this.configTableName = configTableName;
            this.keyColumnName = keyColumnName;
            this.valueColumnName = valueColumnName;

            this.CreateConfig ();
        }

        public IDbConnection DbConnection { get => this.dbConnection; }
        public string ConfigTableName { get=> this.configTableName; }
        public string KeyColumnName { get => keyColumnName;}
        public string ValueColumnName { get => valueColumnName;}

        protected void CreateConfig () {
            try {
                this.dbConnection.CloneConnection()
                .CreateCommand ()
                    .ExecuteNonQuery ("CREATE TABLE " + this.ConfigTableName + " (" + this.keyColumnName + " VARCHAR(255), " + this.valueColumnName + " VARCHAR(1024), PRIMARY KEY (" + this.keyColumnName + "));", true);
            } catch{}
        }

        public void Set (String key, object value)
        {
            if (this.Update (key, value) == 0)
                this.Add (key, value);
        }

        public void Unset (String key)
        {
            this.dbConnection.CloneConnection ()
            .CreateCommand ()
                .AddParameter ("@key", key)
                .ExecuteNonQuery ("DELETE FROM " + this.configTableName + " WHERE " + this.keyColumnName + "=@key;", true);
        }

        public bool IsSet (String key)
        {
            return this.dbConnection.CloneConnection ()
            .CreateCommand ()
                .AddParameter ("@key", key)
                .ExecuteScalar<long>("SELECT COUNT(" + this.keyColumnName + ") FROM " + this.configTableName + " WHERE " + this.keyColumnName + "=@key;", true)
                .Value>0;
        }

        public void Add (String key, object value)
        {
            this.dbConnection.CloneConnection ()
            .CreateCommand ()
                .AddParameter ("@key", key)
                .AddParameter ("@value", Serialize(value))
                .ExecuteNonQuery ("INSERT INTO " + this.configTableName + " (" + this.keyColumnName + ", " + this.valueColumnName + ") VALUES (@key, @value);", true);
        }

        public int Update (String key, object value)
        {
            return this.dbConnection.CloneConnection ()
            .CreateCommand ()
                .AddParameter ("@key", key)
                .AddParameter ("@value", Serialize (value))
                .ExecuteNonQuery ("UPDATE " + this.configTableName + " SET " + this.valueColumnName + "=@value WHERE " + this.keyColumnName + "=@key;", true);
        }

        public T Get<T> (String key)
        {
            DBNullable<String> v = this.dbConnection.CloneConnection ()
                .CreateCommand ()
                    .AddParameter ("@key", key)
                    .ExecuteScalar<String> ("SELECT " + this.valueColumnName + " from " + this.configTableName + " where " + this.keyColumnName + "=@key limit 1;", true);

            if (v == null)
                return default(T);
            else
                return this.Deserialize<T> (v);
        }

        public T Get<T> (String key, T defaultValue)
        {
            DBNullable<String> v = this.dbConnection.CloneConnection ()
                .CreateCommand ()
                    .AddParameter ("@key", key)
                    .ExecuteScalar<String> ("SELECT " + this.valueColumnName + " from " + this.configTableName + " where " + this.keyColumnName + "=@key limit 1;", true);

            if (v == null)
                return defaultValue;
            else
                return this.Deserialize<T> (v);
        }

        public IList<T> GetMatch<T> (String keyMatch)
        {
            return this.dbConnection.CloneConnection ()
            .CreateCommand ()
                .AddParameter ("@key", keyMatch)
                .ExecuteScalarColumn<String> ("SELECT " + this.valueColumnName + " FROM " + this.configTableName + " WHERE " + this.keyColumnName + " LIKE '@keyMatch*' ORDER BY " + this.valueColumnName + " ASC;", this.valueColumnName, true)
                .Select(v=> this.Deserialize<T> (v))
                .ToList();
        }

        private Object Serialize<T> (T value)
        {
            Object ret;

            CultureInfo caux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (value == null)
                ret = DBNull.Value;
            else if (value is IList<byte>)
                ret = ((IList<byte>)value).ToHexadecimal();
            else if (value is DateTime)
                ret = ((DateTime)(object)value).ToStringISO8601(true, true);
            else if (value is TimeSpan)
                ret = ((TimeSpan)(object)value).ToStringISO8601(true, true);
            else if (value is bool)
                ret = ((bool)(object)value)?"1":"0";
            else
                ret = value.ToString ();

            Thread.CurrentThread.CurrentCulture = caux;
            return ret;
        }

        private T Deserialize<T> (DBNullable<String> value)
        {
            Assert.NotNull ( value, nameof(value));
            T ret;

            CultureInfo caux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (!value.HasValue)
                ret = default (T);
            else if (typeof(T).Equals(typeof(byte [])))
                ret = (T)(object)value.Value.HexToByteArray ();
            else if (typeof(T).Equals(typeof(DateTime)))
                ret = (T)(object)value.Value.ToDateTimeISO8601 ();
            else if (typeof(T).Equals(typeof(TimeSpan)))
                ret = (T)(object)value.Value.ToTimeSpanISO8601 ();
            else if (typeof(T).Equals(typeof(bool)))
                ret = (T)(object)(value.Value == "1" ? true : false);
            else
                ret = (T)Convert.ChangeType (value.Value, typeof (T));

            Thread.CurrentThread.CurrentCulture = caux;
            return ret;
        }

        /*
        private String Serialize<T> (T value)
        {
            Assert.NotNull ( value, nameof(value));
            String ret;

            CultureInfo caux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            ret = value.ToString ();

            Thread.CurrentThread.CurrentCulture = caux;
            return ret;
        }

        private T Deserialize<T> (String value)
        {
            Assert.NotNullOrEmpty ( value, nameof(value));
            T ret;

            CultureInfo caux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            ret=(T)Convert.ChangeType (value, typeof (T));

            Thread.CurrentThread.CurrentCulture = caux;
            return ret;
        }
        */

        public static AppDbConfig Instance (IDbConnection dbConnection)
        {
            return Instance (dbConnection, "_DEFAULT_");
        }

        public static AppDbConfig Instance (IDbConnection dbConnection, String instanceName, String configTableName = "_AppDbConfig", String keyColumnName = "_Key", String valueColumnName = "_Value")
        {
            Assert.NotNullOrEmpty ( instanceName, nameof(instanceName));

            if (!instances.ContainsKey (instanceName))
                instances.Add (instanceName, new AppDbConfig (dbConnection, configTableName, keyColumnName, valueColumnName));

            return instances[instanceName];
        }

        public static AppDbConfig Instance () {
            return Instance ("_DEFAULT_");
        }

        public static AppDbConfig Instance (String instanceName)
        {
            Assert.NotNullOrEmpty ( instanceName, nameof(instanceName));

            if (!instances.ContainsKey (instanceName))
                throw new Exception ("Don't exists instance '" + instanceName + "'");

            return instances [instanceName];
        }
    }
}
