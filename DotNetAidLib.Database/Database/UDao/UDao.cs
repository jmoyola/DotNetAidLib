using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DotNetAidLib.Database.UDao;
using DotNetAidLib.Develop;

namespace DotNetAidLib.Database.UDao{
    public class UDao
    {
        private static IDictionary<String, UDao> instances = new Dictionary<String, UDao> ();

        private UDaoEntityMap entityMap = null;
        private List<UDaoSerializer> serializers = new List<UDaoSerializer> ();

        private UDao (UDaoEntityMap entityMap){
            Assert.NotNull( entityMap, nameof(entityMap));
            this.entityMap = entityMap;
        }

        public UDaoEntityMap EntityMap => this.entityMap;
        public IList<UDaoSerializer> Serializers => this.serializers.AsReadOnly();
        public UDao AddSerializer (UDaoSerializer daoSerializer) {
            Assert.NotNull( daoSerializer, nameof(daoSerializer));
            this.serializers.Add (daoSerializer);
            return this;
        }
        private UDaoSerializer GetSerializer (Type type)
        {
            Assert.NotNull( type, nameof(type));

            UDaoSerializer ret = null;
            ret = this.serializers.FirstOrDefault (v => v.Match(type));

            if (ret == null)
                ret = UDaoDefaultSerializer.Instance;

            return ret;
        }

        public IList<T> Select<T> (IDbConnection cnx, String filter = null)
        {
            IList<T> ret = new List<T> ();
            Type t = typeof (T);
            String cmdText = null;
            try {
                t = typeof (T);

                IList<PropertyInfo> pis = this.entityMap.Properties (t);

                IDbCommand cmd = cnx.CreateCommand ();

                cmdText = "SELECT " + pis.Select (v => this.entityMap.ColumnName (t, v)).ToStringJoin (", ")
                + " FROM " + t.Name
                + (String.IsNullOrEmpty (filter) ? "" : " WHERE " + filter)
                + ";";

                cmd.CommandText = cmdText;
                IDataReader dr = cmd.ExecuteReader ();
                while (dr.Read ()) {
                    T instance = Activator.CreateInstance<T> ();
                    foreach (PropertyInfo pi in pis) {
                        pi.SetValue (instance, this.GetSerializer(pi.PropertyType).Deserialize (dr.GetValue (this.entityMap.ColumnName (t, pi)), pi.PropertyType));
                    }
                    ret.Add (instance);
                }

                return ret;
            } catch (Exception ex) {
                throw new UDaoException ("Error selecting " + t.Name + "instance: " + cmdText, ex);
            }
        }

        public int Insert<T> (IDbConnection cnx, T value)
        {
            Type t = typeof (T);
            IDbCommand cmd = null;

            try {
                t = typeof (T);

                IList<PropertyInfo> pis = this.entityMap.Properties (t);

                cmd = cnx.CreateCommand ();
                pis.ToList ().ForEach (v => cmd.AddParameter (
                     this.entityMap.ColumnName (t, v)
                     , this.GetSerializer(v.PropertyType).Serialize (v.GetValue (value), v.PropertyType)));

                IEnumerable<string> prms = pis.Select (v => this.entityMap.ColumnName (t, v));
                String cmdText = "INSERT INTO " + t.Name
                + " (" + prms.Select (v => v).ToStringJoin (", ") + ")"
                + " VALUES (" + prms.Select (v => "@" + v).ToStringJoin (", ") + ");";

                cmd.CommandText = cmdText;
                return cmd.ExecuteNonQuery ();
            } catch (Exception ex) {
                String cmdResult = (cmd == null ? null : cmd.GetCommandResult (50));
                throw new UDaoException ("Error inserting " + t.Name + "instance" + (cmdResult == null ? "." : ": " + cmdResult), ex);
            }
        }

        public int Update<T> (IDbConnection cnx, T value)
        {
            Type t = typeof (T);
            IDbCommand cmd = null;

            try {
                t = typeof (T);
                IList<PropertyInfo> pis = this.entityMap.Properties (t);
                IList<PropertyInfo> pkis = this.entityMap.PrimaryKeys (t);

                cmd = cnx.CreateCommand ();
                pis.ToList ().ForEach (v => cmd.AddParameter (
                     this.entityMap.ColumnName (t, v), this.GetSerializer(v.PropertyType).Serialize (v.GetValue (value), v.PropertyType)));

                IEnumerable<string> prms = pis.Select (v => this.entityMap.ColumnName (t, v));
                IEnumerable<string> pkprms = pkis.Select (v => this.entityMap.ColumnName (t, v));
                String cmdText = "UPDATE " + t.Name
                    + " SET "
                    + prms.Select (v => v + "=@" + v).ToStringJoin (", ")
                    + " WHERE "
                    + pkprms.Select (v => v + "=@" + v).ToStringJoin (" AND ")
                    + ";";

                cmd.CommandText = cmdText;
                return cmd.ExecuteNonQuery ();
            } catch (Exception ex) {
                String cmdResult = (cmd == null ? null : cmd.GetCommandResult (50));
                throw new UDaoException ("Error updating " + t.Name + "instance" + (cmdResult == null ? "." : ": " + cmdResult), ex);
            }
        }

        public int Delete<T> (IDbConnection cnx, T value)
        {
            Type t = typeof (T);
            IDbCommand cmd = null;

            try {
                t = typeof (T);
                IList<PropertyInfo> pkis = this.entityMap.PrimaryKeys (t);

                cmd = cnx.CreateCommand ();
                pkis.ToList ().ForEach (v => cmd.AddParameter (this.entityMap.ColumnName (t, v), this.GetSerializer(v.PropertyType).Serialize (v.GetValue (value), v.PropertyType)));

                IEnumerable<string> pkprms = pkis.Select (v => this.entityMap.ColumnName (t, v));
                String cmdText = "DELETE FROM " + t.Name
                    + " WHERE "
                    + pkprms.Select (v => v + "=@" + v).ToStringJoin (", ")
                    + ";";

                cmd.CommandText = cmdText;
                return cmd.ExecuteNonQuery ();
            } catch (Exception ex) {
                String cmdResult = (cmd == null ? null : cmd.GetCommandResult (50));
                throw new UDaoException ("Error deleting " + t.Name + "instance" + (cmdResult == null ? "." : ": " + cmdResult), ex);
            }
        }

        public const String DEFAULT_INSTANCE = "__DEFAULT__";

        public static UDao Instance (String key = DEFAULT_INSTANCE)
        {
            return Instance(UDaoDefaultEntityMap.Instance);
        }

        public static UDao Instance (UDaoEntityMap entityMap, String key=DEFAULT_INSTANCE)
        {
            if (!instances.ContainsKey (key))
                instances.Add (key, new UDao (entityMap));

            return instances [key];
        }
    }
}

namespace System.Data
{
    public static class EN_UDao {

        public static IList<T> UDaoSelect<T> (this IDbConnection cnx, String filter = null, String udaoInstance=UDao.DEFAULT_INSTANCE) {
            return UDao.Instance (udaoInstance).Select<T> (cnx, filter);
        }

        public static int UDaoInsert<T> (this IDbConnection cnx, T value, String udaoInstance = UDao.DEFAULT_INSTANCE) {
            return UDao.Instance (udaoInstance).Insert<T> (cnx, value);
        }

        public static int UDaoUpdate<T> (this IDbConnection cnx, T value, String udaoInstance = UDao.DEFAULT_INSTANCE) {
            return UDao.Instance (udaoInstance).Update<T> (cnx, value);
        }

        public static int UDaoDelete<T> (this IDbConnection cnx, T value, String udaoInstance = UDao.DEFAULT_INSTANCE) {
            return UDao.Instance (udaoInstance).Delete<T> (cnx, value);
        }
    }
}
