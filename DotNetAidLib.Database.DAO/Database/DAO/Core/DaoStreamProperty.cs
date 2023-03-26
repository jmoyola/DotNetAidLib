using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoStreamProperty<T> where T : class
    {
        protected DaoEntityAttribute daoEntityAttribute;
        protected DaoPropertyAttribute daoPropertyAttribute;
        protected DaoSession daoSession;
        protected T entity;

        public DaoStreamProperty(DaoSession daoSession, T entity, DaoEntityAttribute daoEntityAttribute,
            Expression<Func<T, byte[]>> propertyExpression)
            : this(daoSession, entity, daoEntityAttribute,
                Dao<T>.PropertyAtributeFromPropertyExpression(propertyExpression))
        {
        }

        public DaoStreamProperty(DaoSession daoSession, T entity, DaoEntityAttribute daoEntityAttribute,
            DaoPropertyAttribute daoPropertyAttribute)
        {
            Assert.NotNull(daoSession, nameof(daoSession));
            Assert.NotNull(entity, nameof(entity));
            Assert.NotNull(daoEntityAttribute, nameof(daoEntityAttribute));
            Assert.NotNull(daoPropertyAttribute, nameof(daoPropertyAttribute));
            if (!typeof(byte[]).IsAssignableFrom(daoPropertyAttribute.PropertyInfo.PropertyType))
                throw new DaoException("Only 'byte[]' type is allowed for property type.");

            this.daoSession = daoSession;
            this.entity = entity;
            this.daoEntityAttribute = daoEntityAttribute;
            this.daoPropertyAttribute = daoPropertyAttribute;
        }

        public byte[] GetArray()
        {
            var ms = new MemoryStream();

            if (GetStream(ms))
            {
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }

            return null;
        }

        public void SetArray(byte[] value)
        {
            MemoryStream ms = null;

            if (value != null)
                ms = new MemoryStream(value);

            SetStream(ms);
        }

        public virtual bool GetStream(Stream outStream)
        {
            IDbConnection cnx = null;
            IDataReader dr = null;
            IList<KeyValuePair<string, object>> primaryKeySQLValues = null;
            try
            {
                primaryKeySQLValues = GetPrimaryKeySQLValues(entity);
                cnx = daoSession.DBConnection;

                var cmd = cnx.CreateCommand("SELECT " + daoPropertyAttribute.ColumnName
                                                      + " FROM " + DaoEntityAttribute.GetAttribute(typeof(T)).TableName
                                                      + " WHERE " +
                                                      primaryKeySQLValues.Select(v => v.Key + "=@" + v.Key)
                                                          .ToStringJoin(" AND "));
                primaryKeySQLValues.ToList().ForEach(v => cmd.AddParameter("@" + v.Key, v.Value));

                OnDaoEvent(new DaoEventArgs(typeof(T), entity, DaoEventType.BeforeRetrieve));
                OnDaoSQLEvent(new DaoSQLEventArgs(typeof(T), cmd.GetCommandResult()));
                dr = cmd.ExecuteReader();
                OnDaoEvent(new DaoEventArgs(typeof(T), entity, DaoEventType.AfterRetrieve));

                if (!dr.Read())
                    throw new DaoException("Entity '" + typeof(T).Name + "' don't exists.");

                if (dr.IsDBNull(daoPropertyAttribute.ColumnName))
                {
                    return false;
                }
                else
                {
                    new DataReaderStream(dr, daoPropertyAttribute.ColumnName)
                        .CopyTo(outStream);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new DaoException(
                    "Error getting binary stream '" + typeof(T).Name + "." + daoPropertyAttribute.PropertyInfo.Name +
                    "' from entity with id '" +
                    primaryKeySQLValues.Select(v => v.Key + "=" + v.Value).ToStringJoin(", ") + "'", ex);
            }
            finally
            {
                if (dr != null)
                    dr.Dispose();
            }
        }

        public virtual void SetStream(Stream inputStream)
        {
            object value = null;

            if (inputStream == null)
                value = DBNull.Value;
            else
                value = inputStream.ReadAll();

            IDbConnection cnx = null;
            IList<KeyValuePair<string, object>> primaryKeySQLValues = null;
            try
            {
                primaryKeySQLValues = GetPrimaryKeySQLValues(entity);
                cnx = daoSession.DBConnection;

                var cmd = cnx.CreateCommand("UPDATE " + DaoEntityAttribute.GetAttribute(typeof(T)).TableName
                                                      + " SET " + daoPropertyAttribute.ColumnName + "=@" +
                                                      daoPropertyAttribute.ColumnName
                                                      + " WHERE " +
                                                      primaryKeySQLValues.Select(v => v.Key + "=@" + v.Key)
                                                          .ToStringJoin(" AND "))
                    .AddParameter("@" + daoPropertyAttribute.ColumnName, value);

                primaryKeySQLValues.ToList().ForEach(v => cmd.AddParameter("@" + v.Key, v.Value));

                OnDaoEvent(new DaoEventArgs(typeof(T), entity, DaoEventType.BeforeUpdate));
                OnDaoSQLEvent(new DaoSQLEventArgs(typeof(T), cmd.GetCommandResult()));

                var i = cmd.ExecuteNonQuery();

                OnDaoEvent(new DaoEventArgs(typeof(T), entity, DaoEventType.AfterUpdate));
                if (i == 0)
                    throw new DaoException("Entity '" + typeof(T).Name + "' don't exists.");
            }
            catch (Exception ex)
            {
                throw new DaoException(
                    "Error setting property '" + typeof(T).Name + "." + daoPropertyAttribute.PropertyInfo.Name +
                    "' to entity with id '" +
                    primaryKeySQLValues.Select(v => v.Key + "=" + v.Value).ToStringJoin(", ") + "'", ex);
            }
        }

        public virtual void SetFile(string path, SourceType fileSource)
        {
            IDbConnection cnx = null;
            SQLUploaderFile suf = null;
            IList<KeyValuePair<string, object>> primaryKeySQLValues = null;
            try
            {
                primaryKeySQLValues = GetPrimaryKeySQLValues(entity);
                cnx = daoSession.DBConnection;

                suf = SQLUploaderFile.Instance(cnx, fileSource, path);
                var cmd = cnx.CreateCommand("UPDATE " + DaoEntityAttribute.GetAttribute(typeof(T)).TableName
                                                      + " SET " + daoPropertyAttribute.ColumnName + "=" + suf
                                                      + " WHERE " +
                                                      primaryKeySQLValues.Select(v => v.Key + "=@" + v.Key)
                                                          .ToStringJoin(" AND "))
                    ;

                primaryKeySQLValues.ToList().ForEach(v => cmd.AddParameter("@" + v.Key, v.Value));

                OnDaoEvent(new DaoEventArgs(typeof(T), entity, DaoEventType.BeforeUpdate));
                OnDaoSQLEvent(new DaoSQLEventArgs(typeof(T), cmd.GetCommandResult()));

                var i = cmd.ExecuteNonQuery();

                OnDaoEvent(new DaoEventArgs(typeof(T), entity, DaoEventType.AfterUpdate));
                if (i == 0)
                    throw new DaoException("Entity '" + typeof(T).Name + "' don't exists.");
            }
            catch (Exception ex)
            {
                throw new DaoException(
                    "Error setting property '" + typeof(T).Name + "." + daoPropertyAttribute.PropertyInfo.Name +
                    "' from file '" + path + "' to entity with id '" +
                    primaryKeySQLValues.Select(v => v.Key + "=" + v.Value).ToStringJoin(", ") + "'", ex);
            }
            finally
            {
                try
                {
                    if (suf != null)
                        suf.Dispose();
                }
                catch
                {
                }
            }
        }

        public virtual void GetFile(string filePath)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create);
                GetStream(fs);
            }
            catch (Exception ex)
            {
                throw new DaoException("Error getting property to file '" + filePath + "'.", ex);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        private IList<KeyValuePair<string, object>> GetPrimaryKeySQLValues(T entityValue)
        {
            var ret = new List<KeyValuePair<string, object>>();

            foreach (var dpa in DaoPropertyAttribute.GetPropertyAttributes(typeof(T))
                         .Where(v => v is DaoPropertyPKAttribute)
                         .OrderBy(v => ((DaoPropertyPKAttribute) v).Order))
            {
                object value;
                var propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                if (propertyParser != null)
                    value = propertyParser.SerializeBeforeUpdate(daoSession, DaoEntityAttribute.GetAttribute(typeof(T)),
                        dpa, entityValue);
                else
                    value = dpa.PropertyInfo.GetValue(entityValue);

                ret.Add(new KeyValuePair<string, object>(dpa.ColumnName, value == null ? DBNull.Value : value));
            }

            return ret;
        }

        protected void OnDaoEvent(DaoEventArgs args)
        {
            if (daoSession.Context.Properties.ContainsKey("daoEventHandler")
                && daoSession.Context.Properties["daoEventHandler"] != null)
                try
                {
                    ((DaoEventHandler) daoSession.Context.Properties["daoEventHandler"]).Invoke(this, args);
                }
                catch
                {
                }
        }

        protected void OnDaoSQLEvent(DaoSQLEventArgs args)
        {
            if (daoSession.Context.Properties.ContainsKey("daoSQLEventHandler")
                && daoSession.Context.Properties["daoSQLEventHandler"] != null)
                try
                {
                    ((DaoSQLEventHandler) daoSession.Context.Properties["daoSQLEventHandler"]).Invoke(this, args);
                }
                catch
                {
                }
        }
    }
}