using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DotNetAidLib.Core.Configuration.Dictionary.Core;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Serializer;

namespace DotNetAidLib.Database.Configuration.Dictionary.Impl
{
    public class AdoDictionaryConfig: DictionaryConfig
    {
        public const String DEFAULT_KEY_COLUMN_NAME = "_key";
        public const String DEFAULT_TYPE_COLUMN_NAME = "_type";
        public const String DEFAULT_VALUE_COLUMN_NAME = "_value";
        public const UInt32 DEFAULT_KEY_COLUMN_LENGTH = 255;
        public const UInt32 DEFAULT_TYPE_COLUMN_LENGTH = 255;
        public const UInt32 DEFAULT_VALUE_COLUMN_LENGTH = 1024;
        private DBProviderConnector dbProviderConnector;
        private String fqnTableName;
        private String keyColumnName;
        private String typeColumnName;
        private String valueColumnName;
        private IStringParser stringParser = null;

        public AdoDictionaryConfig (DBProviderConnector dbProviderConnector, IStringParser stringParser=null, String fqnTableName=DEFAULT_INSTANCE_ID, String keyColumnName=DEFAULT_KEY_COLUMN_NAME, String typeColumnName=DEFAULT_TYPE_COLUMN_NAME, String valueColumnName=DEFAULT_VALUE_COLUMN_NAME)
        {
            Assert.NotNull( dbProviderConnector, nameof(dbProviderConnector));
            Assert.NotNullOrEmpty( fqnTableName, nameof(fqnTableName));
            Assert.NotNullOrEmpty( keyColumnName, nameof(keyColumnName));
            Assert.NotNullOrEmpty( typeColumnName, nameof(typeColumnName));
            Assert.NotNullOrEmpty( valueColumnName, nameof(valueColumnName));

            this.dbProviderConnector = dbProviderConnector;
            this.fqnTableName = fqnTableName;
            this.keyColumnName = keyColumnName;
            this.typeColumnName = typeColumnName;
            this.valueColumnName = valueColumnName;
            this.stringParser = (stringParser == null?SimpleStringParser.Instance():stringParser);
            
            this.Init();
        }

        private void Init () {
            try {
                this.dbProviderConnector.CreateConnection(true)
                    .CreateCommand ()
                    .ExecuteNonQuery ("CREATE TABLE " + this.fqnTableName + " ("
                                      + this.keyColumnName + " VARCHAR(" + DEFAULT_KEY_COLUMN_LENGTH + "), "
                                      + this.typeColumnName + " VARCHAR(" + DEFAULT_TYPE_COLUMN_LENGTH + "), "
                                      + this.valueColumnName + " VARCHAR(" + DEFAULT_VALUE_COLUMN_LENGTH + ")"
                                      +");"
                                      +"ALTER TABLE " +  this.fqnTableName + " ADD CONSTRAINT pk PRIMARY KEY (" + this.keyColumnName + ");", true);
            } catch{}
        }
        public override Object this [string key] {
            get {
                try {
                    using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                    {
                        Type type = null;
                        var lRet = lCnx.CreateCommand()
                            .AddParameter("@keyColumnName", key)
                            .ExecuteListValues(
                                "SELECT " + this.valueColumnName + ", " + this.typeColumnName + " FROM " + fqnTableName
                                + " WHERE " + this.keyColumnName + "=@keyColumnName", false);
                        if (lRet.Count == 0)
                            throw new DictionaryConfigException("Config with key '" + key + "' don't exists.");

                        type = DBNull.Value.Equals(lRet[0][1]) ? null : Type.GetType(lRet[0][1].ToString());
                        return (DBNull.Value.Equals(lRet[0][0]) ? null:this.stringParser.Unparse(lRet[0][0].ToString(), type));
                    }
                } catch (Exception ex) {
                    throw new DictionaryConfigException ("Error getting value.", ex);
                }
            }
            set {
                try {
                    using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                    {
                        int lRet = lCnx.CreateCommand()
                            .AddParameter("@keyColumnName", key)
                            .AddParameter("@typeColumnName", value==null? null: value.GetType().AssemblyQualifiedName)
                            .AddParameter("@valueColumnName", this.stringParser.Parse(value))
                            .ExecuteNonQuery(
                                "UPDATE " + fqnTableName + " SET " 
                                + valueColumnName + "=@valueColumnName, "
                                + typeColumnName + "=@typeColumnName"
                                + " WHERE " + keyColumnName + "=@keyColumnName", false);
                        if (lRet == 0)
                            throw new DictionaryConfigException("Config with key '" + key + "' don't exists.");
                    }
                } catch (Exception ex) {
                    throw new DictionaryConfigException ("Error setting value.", ex);
                } 
            }
        }
        
        public override ICollection<string> Keys {
            get {
                try {
                    using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                    {
                        IList<Object> lRet = lCnx.CreateCommand()
                            .ExecuteScalarRow("SELECT " + keyColumnName + " FROM " + fqnTableName, false);

                        return lRet.Select(v => v.ToString()).ToList();
                    }
                } catch (Exception ex) {
                    throw new DictionaryConfigException ("Error getting keys.", ex);
                }
            }
        }

        public override ICollection<Object> Values {
            get {
                try {
                    using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                    {
                        IList<Object> lRet = lCnx.CreateCommand()
                            .ExecuteScalarRow("SELECT " + valueColumnName + " FROM " + fqnTableName, false);

                        return lRet.Select(v =>
                            (DBNull.Value.Equals(v) ? null : this.stringParser.Unparse(v.ToString()))).ToList();
                    }
                } catch (Exception ex) {
                    throw new DictionaryConfigException ("Error getting values.", ex);
                }
            }
        }

        public override IList<KeyValuePair<String, Object>> GetValueMatches(String pattern)
        {
            try {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    IList<KeyValuePair<String, Object>> ret = new List<KeyValuePair<String, Object>>();
                    
                    var lRet = lCnx.CreateCommand()
                        .ExecuteListValues(
                            "SELECT " + this.keyColumnName+  ", " + this.typeColumnName+ ", " + this.valueColumnName + " FROM " + fqnTableName
                            + " WHERE " + this.keyColumnName + " LIKE '" +pattern + "'"
                            + " ORDER BY " + this.keyColumnName + " ASC", false);
                    
                    foreach (var row in lRet)
                    {
                        String key = row[0].ToString();
                        Type type = DBNull.Value.Equals(row[1]) ? null : Type.GetType(row[1].ToString());
                        Object value= (DBNull.Value.Equals(row[2]) ? null:this.stringParser.Unparse(row[2].ToString(), type));
                        
                        ret.Add(new KeyValuePair<string, object>(key, value));
                    }
                    
                    return ret;
                }
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error getting value matchs.", ex);
            }

        }

        public override int Count {
            get {
                try {
                    using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                    {
                        DBNullable<int> lRet = lCnx.CreateCommand()
                            .ExecuteScalar<int>("SELECT COUNT(" + keyColumnName + ") FROM " + fqnTableName, false);

                        return (lRet == null ? 0 : lRet.Value);
                    }
                } catch (Exception ex) {
                    throw new DictionaryConfigException ("Error getting count.", ex);
                }
            }
        }

        public override bool IsReadOnly {
            get {
                return false;
            }
        }

        public override void Add(string key, Object value)
        {
            try
            {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    int lRet = lCnx.CreateCommand()
                        .AddParameter("@keyColumnName", key)
                        .AddParameter("@typeColumnName", value==null? null: value.GetType().AssemblyQualifiedName)
                        .AddParameter("@valueColumnName", this.stringParser.Parse(value))
                        .ExecuteNonQuery(
                            "INSERT INTO " + fqnTableName + " (" + keyColumnName + ", " + typeColumnName + ", " + valueColumnName +
                            ") VALUES (@keyColumnName, @typeColumnName, @valueColumnName)", false);
                }
            }
            catch (Exception ex)
            {
                throw new DictionaryConfigException("Error adding value.", ex);
            }
        }

        public override void Clear ()
        {
            try
            {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    DBNullable<int> lRet = lCnx.CreateCommand()
                    .ExecuteScalar<int>("DELETE FROM " + fqnTableName, false);
                }
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error clearing.", ex);
            }
        }

        public override bool ContainsKey (string key)
        {
            try {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    DBNullable<int> lRet = lCnx.CreateCommand()
                        .AddParameter("@keyColumnName", key)
                        .ExecuteScalar<int>(
                            "SELECT COUNT(" + keyColumnName + ") FROM " + fqnTableName + " WHERE " +
                            this.keyColumnName + "=@keyColumnName", false);

                    return (lRet != null && lRet.HasValue && lRet.Value == 1);
                }
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error contains key.", ex);
            }
        }

        protected override IList<KeyValuePair<string, Object>> GetItems ()
        {
            try {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    IList<DbRow> lRet = lCnx.CreateCommand()
                        .ExecuteListValues("SELECT " + keyColumnName + ", " + valueColumnName + " FROM " + fqnTableName,
                            false);

                    return lRet.Select(v => new KeyValuePair<String, Object>(v[0].ToString(),
                        DBNull.Value.Equals(v[1]) ? null : this.stringParser.Unparse(v[1].ToString()))).ToList();
                }
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error getting items.", ex);
            }
        }

        public override IEnumerator<KeyValuePair<string, Object>> GetEnumerator ()
        {
            return this.GetItems ().GetEnumerator ();
        }

        public override bool Remove (string key)
        {
            try {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    int lRet = lCnx.CreateCommand()
                        .AddParameter("@keyColumnName", key)
                        .ExecuteNonQuery("DELETE FROM " + fqnTableName + " WHERE " + keyColumnName + "=@keyColumnName",
                            false);
                    return lRet > 0;
                }
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error removing key.", ex);
            }
        }

        public override bool Remove (KeyValuePair<string, Object> item)
        {
            try {
                using (IDbConnection lCnx = this.dbProviderConnector.CreateConnection(true))
                {
                    int lRet = lCnx.CreateCommand()
                        .AddParameter("@keyColumnName", item.Key)
                        .AddParameter("@valueColumnName", this.stringParser.Parse(item.Value))
                        .ExecuteNonQuery(
                            "DELETE FROM " + fqnTableName + " WHERE " + keyColumnName + "=@keyColumnName AND " +
                            valueColumnName + "=@valueColumnName ", false);
                    return lRet > 0;
                }
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error removing item.", ex);
            }
        }

        public override bool TryGetValue (string key, out Object value)
        {
            value = null;
            try {
                value = this [key];
                return true;
            } catch {
                return false;
            }
        }
    }
}
