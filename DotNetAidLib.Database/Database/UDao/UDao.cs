using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.UDao;

namespace DotNetAidLib.Database.UDao
{
    public class UDao
    {
        public const string DEFAULT_INSTANCE = "__DEFAULT__";
        private static readonly IDictionary<string, UDao> instances = new Dictionary<string, UDao>();

        private readonly List<UDaoSerializer> serializers = new List<UDaoSerializer>();

        private UDao(UDaoEntityMap entityMap)
        {
            Assert.NotNull(entityMap, nameof(entityMap));
            EntityMap = entityMap;
        }

        public UDaoEntityMap EntityMap { get; }

        public IList<UDaoSerializer> Serializers => serializers.AsReadOnly();

        public UDao AddSerializer(UDaoSerializer daoSerializer)
        {
            Assert.NotNull(daoSerializer, nameof(daoSerializer));
            serializers.Add(daoSerializer);
            return this;
        }

        private UDaoSerializer GetSerializer(Type type)
        {
            Assert.NotNull(type, nameof(type));

            UDaoSerializer ret = null;
            ret = serializers.FirstOrDefault(v => v.Match(type));

            if (ret == null)
                ret = UDaoDefaultSerializer.Instance;

            return ret;
        }

        public IList<T> Select<T>(IDbConnection cnx, string filter = null)
        {
            IList<T> ret = new List<T>();
            var t = typeof(T);
            string cmdText = null;
            try
            {
                t = typeof(T);

                var pis = EntityMap.Properties(t);

                var cmd = cnx.CreateCommand();

                cmdText = "SELECT " + pis.Select(v => EntityMap.ColumnName(t, v)).ToStringJoin(", ")
                                    + " FROM " + t.Name
                                    + (string.IsNullOrEmpty(filter) ? "" : " WHERE " + filter)
                                    + ";";

                cmd.CommandText = cmdText;
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var instance = Activator.CreateInstance<T>();
                    foreach (var pi in pis)
                        pi.SetValue(instance,
                            GetSerializer(pi.PropertyType).Deserialize(dr.GetValue(EntityMap.ColumnName(t, pi)),
                                pi.PropertyType));
                    ret.Add(instance);
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new UDaoException("Error selecting " + t.Name + "instance: " + cmdText, ex);
            }
        }

        public int Insert<T>(IDbConnection cnx, T value)
        {
            var t = typeof(T);
            IDbCommand cmd = null;

            try
            {
                t = typeof(T);

                var pis = EntityMap.Properties(t);

                cmd = cnx.CreateCommand();
                pis.ToList().ForEach(v => cmd.AddParameter(
                    EntityMap.ColumnName(t, v)
                    , GetSerializer(v.PropertyType).Serialize(v.GetValue(value), v.PropertyType)));

                var prms = pis.Select(v => EntityMap.ColumnName(t, v));
                var cmdText = "INSERT INTO " + t.Name
                                             + " (" + prms.Select(v => v).ToStringJoin(", ") + ")"
                                             + " VALUES (" + prms.Select(v => "@" + v).ToStringJoin(", ") + ");";

                cmd.CommandText = cmdText;
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                var cmdResult = cmd == null ? null : cmd.GetCommandResult(50);
                throw new UDaoException(
                    "Error inserting " + t.Name + "instance" + (cmdResult == null ? "." : ": " + cmdResult), ex);
            }
        }

        public int Update<T>(IDbConnection cnx, T value)
        {
            var t = typeof(T);
            IDbCommand cmd = null;

            try
            {
                t = typeof(T);
                var pis = EntityMap.Properties(t);
                var pkis = EntityMap.PrimaryKeys(t);

                cmd = cnx.CreateCommand();
                pis.ToList().ForEach(v => cmd.AddParameter(
                    EntityMap.ColumnName(t, v),
                    GetSerializer(v.PropertyType).Serialize(v.GetValue(value), v.PropertyType)));

                var prms = pis.Select(v => EntityMap.ColumnName(t, v));
                var pkprms = pkis.Select(v => EntityMap.ColumnName(t, v));
                var cmdText = "UPDATE " + t.Name
                                        + " SET "
                                        + prms.Select(v => v + "=@" + v).ToStringJoin(", ")
                                        + " WHERE "
                                        + pkprms.Select(v => v + "=@" + v).ToStringJoin(" AND ")
                                        + ";";

                cmd.CommandText = cmdText;
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                var cmdResult = cmd == null ? null : cmd.GetCommandResult(50);
                throw new UDaoException(
                    "Error updating " + t.Name + "instance" + (cmdResult == null ? "." : ": " + cmdResult), ex);
            }
        }

        public int Delete<T>(IDbConnection cnx, T value)
        {
            var t = typeof(T);
            IDbCommand cmd = null;

            try
            {
                t = typeof(T);
                var pkis = EntityMap.PrimaryKeys(t);

                cmd = cnx.CreateCommand();
                pkis.ToList().ForEach(v => cmd.AddParameter(EntityMap.ColumnName(t, v),
                    GetSerializer(v.PropertyType).Serialize(v.GetValue(value), v.PropertyType)));

                var pkprms = pkis.Select(v => EntityMap.ColumnName(t, v));
                var cmdText = "DELETE FROM " + t.Name
                                             + " WHERE "
                                             + pkprms.Select(v => v + "=@" + v).ToStringJoin(", ")
                                             + ";";

                cmd.CommandText = cmdText;
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                var cmdResult = cmd == null ? null : cmd.GetCommandResult(50);
                throw new UDaoException(
                    "Error deleting " + t.Name + "instance" + (cmdResult == null ? "." : ": " + cmdResult), ex);
            }
        }

        public static UDao Instance(string key = DEFAULT_INSTANCE)
        {
            return Instance(UDaoDefaultEntityMap.Instance);
        }

        public static UDao Instance(UDaoEntityMap entityMap, string key = DEFAULT_INSTANCE)
        {
            if (!instances.ContainsKey(key))
                instances.Add(key, new UDao(entityMap));

            return instances[key];
        }
    }
}

namespace System.Data
{
    public static class EN_UDao
    {
        public static IList<T> UDaoSelect<T>(this IDbConnection cnx, string filter = null,
            string udaoInstance = UDao.DEFAULT_INSTANCE)
        {
            return UDao.Instance(udaoInstance).Select<T>(cnx, filter);
        }

        public static int UDaoInsert<T>(this IDbConnection cnx, T value, string udaoInstance = UDao.DEFAULT_INSTANCE)
        {
            return UDao.Instance(udaoInstance).Insert(cnx, value);
        }

        public static int UDaoUpdate<T>(this IDbConnection cnx, T value, string udaoInstance = UDao.DEFAULT_INSTANCE)
        {
            return UDao.Instance(udaoInstance).Update(cnx, value);
        }

        public static int UDaoDelete<T>(this IDbConnection cnx, T value, string udaoInstance = UDao.DEFAULT_INSTANCE)
        {
            return UDao.Instance(udaoInstance).Delete(cnx, value);
        }
    }
}