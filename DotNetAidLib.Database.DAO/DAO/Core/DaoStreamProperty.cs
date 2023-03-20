using System;
using System.IO;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoStreamProperty<T> where T:class
    {
        protected DaoSession daoSession;
        protected T entity;
        protected DaoPropertyAttribute daoPropertyAttribute;
        protected DaoEntityAttribute daoEntityAttribute = null;

        public DaoStreamProperty (DaoSession daoSession, T entity, DaoEntityAttribute daoEntityAttribute, Expression<Func<T, byte[]>> propertyExpression)
            :this(daoSession, entity, daoEntityAttribute, Dao<T>.PropertyAtributeFromPropertyExpression (propertyExpression)){}

        public DaoStreamProperty (DaoSession daoSession, T entity, DaoEntityAttribute daoEntityAttribute, DaoPropertyAttribute daoPropertyAttribute)
        {
            Assert.NotNull( daoSession, nameof(daoSession));
            Assert.NotNull( entity, nameof(entity));
            Assert.NotNull( daoEntityAttribute, nameof(daoEntityAttribute));
            Assert.NotNull( daoPropertyAttribute, nameof(daoPropertyAttribute));
            if (!typeof (byte[]).IsAssignableFrom (daoPropertyAttribute.PropertyInfo.PropertyType))
                throw new DaoException ("Only 'byte[]' type is allowed for property type.");

            this.daoSession = daoSession;
            this.entity = entity;
            this.daoEntityAttribute = daoEntityAttribute;
            this.daoPropertyAttribute = daoPropertyAttribute;
        }

        public byte [] GetArray () {
            MemoryStream ms = new MemoryStream ();

            if (this.GetStream (ms)) {
                ms.Seek (0, SeekOrigin.Begin);
                return ms.ToArray ();
            } else
                return null;
        }

        public void SetArray (byte [] value)
        {
            MemoryStream ms = null;

            if(value!=null)
                ms= new MemoryStream (value);

            this.SetStream (ms);
        }

        public virtual bool GetStream(Stream outStream)
        {
            IDbConnection cnx = null;
            IDataReader dr = null;
            IList<KeyValuePair<String, Object>> primaryKeySQLValues = null;
            try
            {
                primaryKeySQLValues = GetPrimaryKeySQLValues(this.entity);
                cnx = this.daoSession.DBConnection;

                IDbCommand cmd = cnx.CreateCommand ("SELECT " + this.daoPropertyAttribute.ColumnName
                     + " FROM " + DaoEntityAttribute.GetAttribute (typeof (T)).TableName
                     + " WHERE " + primaryKeySQLValues.Select (v => v.Key + "=@" + v.Key).ToStringJoin(" AND "));
                primaryKeySQLValues.ToList ().ForEach (v => cmd.AddParameter("@" + v.Key, v.Value));

                this.OnDaoEvent (new DaoEventArgs (typeof (T), this.entity, DaoEventType.BeforeRetrieve));
                this.OnDaoSQLEvent (new DaoSQLEventArgs (typeof (T), cmd.GetCommandResult ()));
                dr = cmd.ExecuteReader ();
                this.OnDaoEvent (new DaoEventArgs (typeof (T), this.entity, DaoEventType.AfterRetrieve));

                if (!dr.Read ())
                    throw new DaoException ("Entity '" + typeof (T).Name + "' don't exists.");

                if (dr.IsDBNull (this.daoPropertyAttribute.ColumnName))
                    return false;
                else {
                    new DataReaderStream(dr, this.daoPropertyAttribute.ColumnName)
                        .CopyTo(outStream);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new DaoException("Error getting binary stream '" + typeof(T).Name + "." + this.daoPropertyAttribute.PropertyInfo.Name + "' from entity with id '" + primaryKeySQLValues.Select(v => v.Key + "=" + v.Value).ToStringJoin(", ") + "'", ex);
            }
            finally {
                if (dr != null)
                    dr.Dispose();
            }
        }

        public virtual void SetStream(Stream inputStream)
        {
            Object value = null;

            if (inputStream == null)
                value = DBNull.Value;
            else
                value = inputStream.ReadAll();

            IDbConnection cnx = null;
            IList<KeyValuePair<String, Object>> primaryKeySQLValues = null;
            try
            {
                primaryKeySQLValues = GetPrimaryKeySQLValues(this.entity);
                cnx = this.daoSession.DBConnection;

                IDbCommand cmd = cnx.CreateCommand ("UPDATE " + DaoEntityAttribute.GetAttribute (typeof (T)).TableName
                 + " SET " + daoPropertyAttribute.ColumnName + "=@" + daoPropertyAttribute.ColumnName
                 + " WHERE " + primaryKeySQLValues.Select (v => v.Key + "=@" + v.Key).ToStringJoin (" AND "))
                 .AddParameter ("@" + daoPropertyAttribute.ColumnName, value);

                primaryKeySQLValues.ToList ().ForEach (v => cmd.AddParameter ("@" + v.Key, v.Value));

                this.OnDaoEvent (new DaoEventArgs (typeof (T), this.entity, DaoEventType.BeforeUpdate));
                this.OnDaoSQLEvent (new DaoSQLEventArgs (typeof (T), cmd.GetCommandResult ()));

                int i = cmd.ExecuteNonQuery ();

                this.OnDaoEvent (new DaoEventArgs (typeof (T), this.entity, DaoEventType.AfterUpdate));
                if (i == 0)
                    throw new DaoException ("Entity '" + typeof (T).Name + "' don't exists.");
            }
            catch (Exception ex)
            {
                throw new DaoException("Error setting property '" + typeof(T).Name + "." + daoPropertyAttribute.PropertyInfo.Name + "' to entity with id '" + primaryKeySQLValues.Select(v => v.Key + "=" + v.Value).ToStringJoin(", ") + "'", ex);
            }
        }

        public virtual void SetFile(String path, SourceType fileSource)
        {

            IDbConnection cnx = null;
            SQLUploaderFile suf = null;
            IList<KeyValuePair<String, Object>> primaryKeySQLValues = null;
            try
            {
                primaryKeySQLValues = GetPrimaryKeySQLValues(this.entity);
                cnx = this.daoSession.DBConnection;

                suf = SQLUploaderFile.Instance(cnx, fileSource, path);
                IDbCommand cmd = cnx.CreateCommand("UPDATE " + DaoEntityAttribute.GetAttribute(typeof(T)).TableName
                 + " SET " + daoPropertyAttribute.ColumnName + "=" + suf.ToString()
                 + " WHERE " + primaryKeySQLValues.Select(v => v.Key + "=@" + v.Key).ToStringJoin(" AND "))
                 ;

                primaryKeySQLValues.ToList().ForEach(v => cmd.AddParameter("@" + v.Key, v.Value));

                this.OnDaoEvent(new DaoEventArgs(typeof(T), this.entity, DaoEventType.BeforeUpdate));
                this.OnDaoSQLEvent(new DaoSQLEventArgs(typeof(T), cmd.GetCommandResult()));

                int i = cmd.ExecuteNonQuery();

                this.OnDaoEvent(new DaoEventArgs(typeof(T), this.entity, DaoEventType.AfterUpdate));
                if (i == 0)
                    throw new DaoException("Entity '" + typeof(T).Name + "' don't exists.");
            }
            catch (Exception ex)
            {
                throw new DaoException("Error setting property '" + typeof(T).Name + "." + daoPropertyAttribute.PropertyInfo.Name + "' from file '" + path + "' to entity with id '" + primaryKeySQLValues.Select(v => v.Key + "=" + v.Value).ToStringJoin(", ") + "'", ex);
            }
            finally
            {
                try
                {
                    if (suf != null)
                        suf.Dispose();
                }
                catch { }
            }
        }

        public virtual void GetFile(String filePath) {
            FileStream fs=null;
            try {
                fs = new FileStream(filePath, FileMode.Create);
                this.GetStream(fs);
            }
            catch(Exception ex) {
                throw new DaoException("Error getting property to file '" + filePath + "'.", ex);
            }
            finally {
                if (fs != null)
                    fs.Close();
            }
        }

        private IList<KeyValuePair<String, Object>> GetPrimaryKeySQLValues(T entityValue)
        {
            List<KeyValuePair<String, Object>> ret = new List<KeyValuePair<String, Object>>();

            foreach (DaoPropertyAttribute dpa in DaoPropertyAttribute.GetPropertyAttributes(typeof(T))
                        .Where(v => v is DaoPropertyPKAttribute)
                        .OrderBy(v => ((DaoPropertyPKAttribute)v).Order))
            {
                Object value;
                DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                if (propertyParser != null)
                    value = propertyParser.SerializeBeforeUpdate(this.daoSession, DaoEntityAttribute.GetAttribute(typeof(T)), dpa, entityValue);
                else
                    value = dpa.PropertyInfo.GetValue(entityValue);

                ret.Add(new KeyValuePair<String, Object>(dpa.ColumnName, (value == null ? DBNull.Value : value)));
            }

            return ret;
        }

        protected void OnDaoEvent(DaoEventArgs args)
        {
            if (this.daoSession.Context.Properties.ContainsKey("daoEventHandler")
                && this.daoSession.Context.Properties["daoEventHandler"] != null)
            {
                try
                {
                    ((DaoEventHandler)this.daoSession.Context.Properties["daoEventHandler"]).Invoke(this, args);
                }
                catch { }
            }
        }

        protected void OnDaoSQLEvent(DaoSQLEventArgs args)
        {
            if (this.daoSession.Context.Properties.ContainsKey("daoSQLEventHandler")
                && this.daoSession.Context.Properties["daoSQLEventHandler"] != null)
            {
                try
                {
                    ((DaoSQLEventHandler)this.daoSession.Context.Properties["daoSQLEventHandler"]).Invoke(this, args);
                }
                catch { }
            }
        }
    }
}
