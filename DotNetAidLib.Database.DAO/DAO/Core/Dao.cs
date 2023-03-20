using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.DAO.Navigation;
using DotNetAidLib.Database.DAO.PKGenerators;
using DotNetAidLib.Database.SQL;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.DAO.Core
{
	public class ProxyGeneratorFactory
	{
		private static ProxyGenerator _Instance=null;

		private ProxyGeneratorFactory()
		{
		}

		public static ProxyGenerator Instance()
		{
			if (_Instance == null)
				_Instance = new ProxyGenerator();
			return _Instance;
		}

	}
	
    public class Dao<T>  where T : class
	{
        private static Dao<T> instance = null;

        private Dao()
		{
        }

        public static Dao<T> Instance ()
        {
            if (instance==null)
                instance=new Dao<T> ();

            return instance;
        }

        public int Add(DaoSession daoSession, T entity) {
            IDbConnection cnx = null;
			IDbCommand cmd = null;

            try
            {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( entity, nameof(entity));

                int ret = -1;

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand();

                List<String> columnNames = new List<String>();

                // PKGenerators (PreGeneration)
                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v is DaoPropertyPKAttribute))
                {
                    DaoPropertyPKGeneratorAttribute pkGenerator = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyPKGeneratorAttribute>(true);
                    if (pkGenerator != null)
                        pkGenerator.PreGeneration(DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity, cnx);
                }

                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v.Mode == DaoPropertyMode.Auto))
                {
                    columnNames.Add(dpa.ColumnName);

                    Object value;
                    DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                    if (propertyParser != null)
                        value = propertyParser.SerializeBeforeInsert(daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity);
                    else
                        value = dpa.PropertyInfo.GetValue(entity);
                    cmd.AddParameter("@" + dpa.ColumnName, (value == null ? DBNull.Value : value));
                }

                cmd.CommandText = "INSERT INTO "
                    + DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
                    + " ( " + columnNames.ToStringJoin(", ") + " )"
                    + " VALUES "
                    + " ( " + columnNames.Select(v => "@" + v).ToStringJoin(", ") + " )";



                this.OnDaoEvent(daoSession, new DaoEventArgs (typeof (T), entity, DaoEventType.BeforeAdd));
                this.OnDaoSQLEvent(daoSession, new DaoSQLEventArgs (typeof(T), cmd.GetCommandResult()));
                ret = cmd.ExecuteNonQuery();

                // PKGenerators (PostGeneration)
                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v is DaoPropertyPKAttribute))
                {
                    DaoPropertyPKGeneratorAttribute pkGenerator = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyPKGeneratorAttribute>(true);
                    if (pkGenerator != null)
                        pkGenerator.PostGeneration(DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity, cnx);
                }

                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v.Mode == DaoPropertyMode.Auto))
                {
                    DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                    if (propertyParser != null)
                        propertyParser.SerializeAfterInsert(daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity);
                }

                this.OnDaoEvent(daoSession, new DaoEventArgs (typeof (T), entity, DaoEventType.AfterAdd));

                return ret;
            }
			catch (Exception ex)
			{
				throw new DaoException("Error adding entity '" + typeof(T).Name + (cmd == null ? "'." : "': " + cmd.GetCommandResult()), ex);
			}
			finally {

                if (cmd != null)
                    cmd.Dispose();
            }
		}

        public int Update(DaoSession daoSession, T entity)
		{
            IDbConnection cnx = null;
			IDbCommand cmd = null;
			try
			{
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( entity, nameof(entity));

                cnx = daoSession.DBConnection;

				cmd = cnx.CreateCommand();

				List<String> columnValues = new List<String>();
				List<String> columnPKValues = new List<String>();
				foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v=>v.Mode==DaoPropertyMode.Auto))
				{
					columnValues.Add(dpa.ColumnName + "=" + "@" + dpa.ColumnName);

					Object value;
					DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute> (true);
					if (propertyParser != null)
						value = propertyParser.SerializeBeforeUpdate (daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity);
					else
						value = dpa.PropertyInfo.GetValue (entity);

					cmd.AddParameter("@" + dpa.ColumnName, (value == null ? DBNull.Value : value));
				}

				cmd.CommandText = "UPDATE "
					+ DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
					+ " SET "
					+ columnValues.ToStringJoin(", ")
					+ " WHERE " + DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v is DaoPropertyPKAttribute)
													  .Select(v => v.ColumnName + "=" + "@" + v.ColumnName).ToStringJoin(" AND ");

                this.OnDaoSQLEvent(daoSession, new DaoSQLEventArgs (typeof(T), cmd.GetCommandResult()));

                this.OnDaoEvent(daoSession, new DaoEventArgs (typeof (T), entity, DaoEventType.BeforeUpdate));

                int ret = cmd.ExecuteNonQuery();

                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v.Mode == DaoPropertyMode.Auto))
                {
                    DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                    if (propertyParser != null)
                        propertyParser.SerializeAfterUpdate(daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity);
                }

                this.OnDaoEvent(daoSession, new DaoEventArgs (typeof (T), entity, DaoEventType.AfterUpdate));

                return ret;
            }
			catch (Exception ex){
				throw new DaoException("Error updating entity '" + typeof(T).Name + (cmd==null?"'.":"': " + cmd.GetCommandResult()), ex);
			}
			finally
			{
                if(cmd!=null)
                    cmd.Dispose();
            }
		}

        public int AddOrUpdate(DaoSession daoSession, T entity){
            try {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( entity, nameof(entity));

                int ret = Update (daoSession, entity);
                if (ret == 0)
                    ret = Add (daoSession, entity);
                return ret;
            } catch (Exception ex) {
                throw new DaoException ("Error Adding or updating entity '" + typeof (T).Name, ex);
            }
        }

        public int UpdateWhere(DaoSession daoSession, IDictionary<String, Object> asignationProperties, IDictionary<String, Object> filterProperties)
        {
            IDbConnection cnx = null;
            IDbCommand cmd = null;

            try
            {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNullOrEmpty( asignationProperties, nameof(asignationProperties));
                Assert.NotNull( filterProperties, nameof(filterProperties));

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand();

                IList<String> columnAssignationValues = PropertiesToSQL(asignationProperties, ref cmd, true);
                IList<String> columnWhereValues = PropertiesToSQL(filterProperties, ref cmd);

                cmd.CommandText = "UPDATE "
                    + DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
                    + " SET " + columnAssignationValues.ToStringJoin(", ")
                    + (columnWhereValues.Count == 0 ? "" : " WHERE " + columnWhereValues.ToStringJoin(" AND "));

                return this.NonQueryCommand(daoSession, cmd);
            }
            catch (Exception ex)
            {
                if (cmd == null)
                    throw new DaoException("Error deleting for entity '" + typeof(T).Name + "'.", ex);
                else
                    throw new DaoException("Error deleting from filterProperties '" + cmd.CommandText + "' for entity '" + typeof(T).Name + "'.", ex);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
        }

        public int Remove (DaoSession daoSession, T entity)
		{
          
            IDbConnection cnx = null;
			IDbCommand cmd = null;

			try {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( entity, nameof(entity));

                cnx = daoSession.DBConnection;

				cmd = cnx.CreateCommand ();

				cmd.CommandText = "DELETE FROM "
					+ DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
					+ " WHERE " + DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v is DaoPropertyPKAttribute)
													  .Select(v => v.ColumnName + "=" + "@" + v.ColumnName).ToStringJoin(" AND ");
				DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v is DaoPropertyPKAttribute).ToList()
									  .ForEach(v => cmd.AddParameter("@" + v.ColumnName, v.PropertyInfo.GetValue(entity)));


                this.OnDaoEvent(daoSession, new DaoEventArgs(typeof (T), entity, DaoEventType.BeforeRemove));

                this.OnDaoSQLEvent (daoSession, new DaoSQLEventArgs (typeof (T), cmd.GetCommandResult ()));

                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v.Mode == DaoPropertyMode.Auto))
                {
                    DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                    if (propertyParser != null)
                        propertyParser.RemoveBeforeDelete(daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity);
                }

                int ret = cmd.ExecuteNonQuery ();

                foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v.Mode == DaoPropertyMode.Auto))
                {
                    DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
                    if (propertyParser != null)
                        propertyParser.RemoveAfterDelete(daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, entity);
                }

                this.OnDaoEvent(daoSession, new DaoEventArgs (typeof (T), entity, DaoEventType.AfterRemove));

                return ret;
			} catch (Exception ex) {
				throw new DaoException("Error removing entity '" + typeof(T).Name + (cmd == null ? "'." : "': " + cmd.GetCommandResult()), ex);
			} finally {
                if (cmd != null)
                    cmd.Dispose();

                
            }
		}

        private IList<String> PropertiesToSQL(IDictionary<String, Object> properties, ref IDbCommand cmd, bool asignationProperties = false) {
            Assert.NotNull( cmd, nameof(cmd));

            List<String> columnWhereValues = new List<String>();
            foreach (KeyValuePair<String, Object> dpa in properties) {
                if (dpa.Value is SQLLiteral && dpa.Value!=null)
                    columnWhereValues.Add(dpa.Key + "=" + dpa.Value);
                else
                {
                    String parameterName = "@" + dpa.Key + cmd.Parameters.Cast<IDataParameter>().Count(v => v.ParameterName.RegexIsMatch(dpa.Key + @"\d*"));
                    columnWhereValues.Add(dpa.Key + (dpa.Value == null && !asignationProperties ? " IS " : "=") + parameterName);
                    cmd.AddParameter(parameterName, (dpa.Value == null ? DBNull.Value : dpa.Value));
                }
            }
            return columnWhereValues;
        }

        public int RemoveWhere(DaoSession daoSession, IDictionary<String, Object> filterProperties)
        {
            IDbConnection cnx = null;
            IDbCommand cmd = null;

            try
            {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( filterProperties, nameof(filterProperties));

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand();

                IList<String> columnWhereValues = PropertiesToSQL(filterProperties, ref cmd);
                cmd.CommandText = "DELETE FROM "
                    + DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
                    + (columnWhereValues.Count == 0 ? "" : " WHERE " + columnWhereValues.ToStringJoin(" AND "));

                return this.NonQueryCommand(daoSession, cmd);
            }
            catch (Exception ex)
            {
                if (cmd == null)
                    throw new DaoException("Error deleting for entity '" + typeof(T).Name + "'.", ex);
                else
                    throw new DaoException("Error deleting from filterProperties '" + cmd.CommandText + "' for entity '" + typeof(T).Name + "'.", ex);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();

                
            }
        }

        public int RemoveWhere(DaoSession daoSession, Expression<Func<T, bool>> predicate)
        {
            IDbConnection cnx = null;
            IDbCommand cmd = null;

            try
            {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( predicate, nameof(predicate));

                LinqToSQLPredicate linqToSQLPredicate = new LinqToSQLPredicate(DaoEntityInfo.Instance<T>().MemberInfoToDaoMember);
                string whereClause = linqToSQLPredicate.TranslateLambda(predicate);

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand();

                cmd.CommandText = "DELETE FROM "
                    + DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
                    + (String.IsNullOrEmpty(whereClause) ? "" : " WHERE " + whereClause);

                return this.NonQueryCommand(daoSession, cmd);
            }
            catch (Exception ex)
            {
                if (cmd == null)
                    throw new DaoException("Error deleting for entity '" + typeof(T).Name + "'.", ex);
                else
                    throw new DaoException("Error deleting from predicate '" + cmd.CommandText + "'for entity '" + typeof(T).Name + "'.", ex);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();

                
            }
        }

		public IList<T> Where(DaoSession daoSession, IDictionary<String, Object> filterProperties, DaoLoadInfo daoLoadInfo = null) {
            return this.Where (daoSession,filterProperties , null, daoLoadInfo);
		}
        public IList<T> Where (DaoSession daoSession, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null)
        {
            return this.Where (daoSession, null, predicate, daoLoadInfo);
        }

        public IList<T> Where(DaoSession daoSession, IDictionary<String, Object> filterProperties, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null) {
            return this.Where(daoSession, filterProperties, predicate, daoLoadInfo, 0);
        }

        public IList<T> Where (DaoSession daoSession, IDictionary<String, Object> filterProperties, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null, int rowCount=0)
        {
            IDbConnection cnx = null;
            IDbCommand cmd = null;

            try {
                Assert.NotNull( daoSession, nameof(daoSession));

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand ();

                StringBuilder where = new StringBuilder();

                if (filterProperties != null) {
                    foreach (KeyValuePair<String, Object> dpa in filterProperties) {
                        where.Append ((where.Length >0?" AND ":"") +  dpa.Key + "=" + "@" + dpa.Key);
                        cmd.AddParameter ("@" + dpa.Key, (dpa.Value == null ? DBNull.Value : dpa.Value));
                    }
                }

                if (predicate != null) {
                    LinqToSQLPredicate linqToSQLPredicate = new LinqToSQLPredicate (DaoEntityInfo.Instance<T> ().MemberInfoToDaoMember);
                    where.Append ((where.Length > 0 ? " AND " : "") + linqToSQLPredicate.TranslateLambda (predicate));
                }

                cmd.CommandText = "SELECT "
                    + DaoEntityInfo.Instance<T> ().DaoPropertyAttributes.Where (v => v.Mode == DaoPropertyMode.Auto).Select (v => v.ColumnName).ToStringJoin (", ")
                    + " FROM " + DaoEntityInfo.Instance<T> ().DaoEntityAttribute.TableName
                    + (where.Length > 0 ? " WHERE " + where.ToString() : "");

                if (rowCount != 0) {
                    ISQLParser pf = SQLParserFactory.Instance(daoSession.Context.DBConnector.DBProviderFactory);
                    cmd.CommandText = cmd.CommandText + " ORDER BY " + DaoPropertyPKAttribute.GetPropertyAttributes(typeof(T)).OrderBy(v => v.Order).Select(v => v.ColumnName).ToStringJoin(", ");
                    if (rowCount > 0)
                        cmd.CommandText = pf.Top(cmd.CommandText, rowCount);
                    else
                        cmd.CommandText = pf.Top(cmd.CommandText + " DESC", Math.Abs(rowCount));
                }

                return this.WhereCommand (daoSession, cmd, daoLoadInfo);
            } catch (Exception ex) {
                if (cmd == null)
                    throw new DaoException ("Error selecting for entity '" + typeof (T).Name + "'.", ex);
                else
                    throw new DaoException ("Error selecting from filterProperties '" + cmd.CommandText + "' for entity '" + typeof (T).Name + "'.", ex);
            } finally {
                if (cmd != null)
                    cmd.Dispose ();


            }
        }

        public T FirstOrDefault (DaoSession daoSession, IDictionary<String, Object> primaryKeys, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null)
        {
            try {
                //return this.Where (daoSession, primaryKeys, predicate, daoLoadInfo).FirstOrDefault ();
                return this.Where(daoSession, primaryKeys, predicate, daoLoadInfo, 1).FirstOrDefault();
            } catch (Exception ex) {
                throw new DaoException ("Error getting entity '" + typeof (T).Name + "'.", ex.InnerException);
            }
        }

        public T FirstOrDefault (DaoSession daoSession, IDictionary<String, Object> primaryKeys, DaoLoadInfo daoLoadInfo = null){
            //return this.FirstOrDefault(daoSession, primaryKeys, null, daoLoadInfo);
            return this.Where(daoSession, primaryKeys, null, daoLoadInfo, 1).FirstOrDefault();
        }

        public T FirstOrDefault (DaoSession daoSession, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null){
            //return this.FirstOrDefault(daoSession, null, predicate, daoLoadInfo);
            return this.Where(daoSession, null, predicate, daoLoadInfo, 1).FirstOrDefault();
        }

        public T FirstOrDefault (DaoSession daoSession, String rawSQLCommand, DaoLoadInfo daoLoadInfo = null)
        {
            return Dao<T>.Instance ().Where (daoSession, rawSQLCommand, daoLoadInfo, 1).FirstOrDefault ();
        }

        public T LastOrDefault(DaoSession daoSession, IDictionary<String, Object> primaryKeys, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null)
        {
            try
            {
                //return this.Where (daoSession, primaryKeys, predicate, daoLoadInfo).FirstOrDefault ();
                return this.Where(daoSession, primaryKeys, predicate, daoLoadInfo, -1).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new DaoException("Error getting entity '" + typeof(T).Name + "'.", ex.InnerException);
            }
        }

        public T LastOrDefault(DaoSession daoSession, IDictionary<String, Object> primaryKeys, DaoLoadInfo daoLoadInfo = null)
        {
            //return this.FirstOrDefault(daoSession, primaryKeys, null, daoLoadInfo);
            return this.Where(daoSession, primaryKeys, null, daoLoadInfo, -1).FirstOrDefault();
        }

        public T LastOrDefault(DaoSession daoSession, Expression<Func<T, bool>> predicate, DaoLoadInfo daoLoadInfo = null)
        {
            //return this.FirstOrDefault(daoSession, null, predicate, daoLoadInfo);
            return this.Where(daoSession, null, predicate, daoLoadInfo, -1).FirstOrDefault();
        }

        public T LastOrDefault(DaoSession daoSession, String rawSQLCommand, DaoLoadInfo daoLoadInfo = null)
        {
            return Dao<T>.Instance().Where(daoSession, rawSQLCommand, daoLoadInfo, -1).FirstOrDefault();
        }

        public UInt64 AggregationCount<S> (DaoSession daoSession, String aggregationFunctionName, Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate = null) where S : IComparable, IConvertible, IEquatable<S>
		{
			IDbConnection cnx = null;
			IDbCommand cmd = null;

			try {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNullOrEmpty( aggregationFunctionName, nameof(aggregationFunctionName));

                LinqToSQLPredicate linqToSQLPredicate = new LinqToSQLPredicate (DaoEntityInfo.Instance<T>().MemberInfoToDaoMember);

				string aggregationClause = "*";

				if (aggregationPredicate != null)
					aggregationClause = linqToSQLPredicate.TranslateLambda (aggregationPredicate);

				string whereClause = null;

				if (predicate != null)
					whereClause = linqToSQLPredicate.TranslateLambda (predicate);

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand ();

				cmd.CommandText = "SELECT " + aggregationFunctionName + "(" + aggregationClause + ") FROM "
					+ DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
					+ (String.IsNullOrEmpty (whereClause) ? "" : " WHERE " + whereClause);

				return this.SelectAggregation<UInt64>(daoSession, cmd);
			} catch (Exception ex) {
				if (cmd == null)
					throw new DaoException ("Error selecting for entity '" + typeof (T).Name + "'.", ex);
				else
					throw new DaoException ("Error selecting from predicate '" + cmd.CommandText + "'for entity '" + typeof (T).Name + "'.", ex);
			} finally {
                if (cmd != null)
                    cmd.Dispose();

                
            }
		}

        public S Aggregation<S> (DaoSession daoSession, String aggregationFunctionName, Expression<Func<T, S>> aggregationPredicate, Expression<Func<T, bool>> predicate=null) where S : IComparable, IConvertible, IEquatable<S>
		{
			IDbConnection cnx = null;
			IDbCommand cmd = null;

			try {
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( aggregationFunctionName, nameof(aggregationFunctionName));

                LinqToSQLPredicate linqToSQLPredicate = new LinqToSQLPredicate (DaoEntityInfo.Instance<T>().MemberInfoToDaoMember);

				string aggregationClause = null;

				if (aggregationPredicate != null)
					aggregationClause = linqToSQLPredicate.TranslateLambda (aggregationPredicate);

				string whereClause = null;

				if (predicate != null)
					whereClause = linqToSQLPredicate.TranslateLambda (predicate);

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand ();

				cmd.CommandText = "SELECT " + aggregationFunctionName + "(" + aggregationClause + ") FROM "
					+ DaoEntityInfo.Instance<T>().DaoEntityAttribute.TableName
					+ (String.IsNullOrEmpty (whereClause) ? "" : " WHERE " + whereClause);

				return this.SelectAggregation<S> (daoSession, cmd);
			} catch (Exception ex) {
				if (cmd == null)
					throw new DaoException ("Error selecting for entity '" + typeof (T).Name + "'.", ex);
				else
					throw new DaoException ("Error selecting from predicate '" + cmd.CommandText + "'for entity '" + typeof (T).Name + "'.", ex);
			} finally {
                if (cmd != null)
                    cmd.Dispose();
            }
		}

        public S Load<S>(DaoSession daoSession, T entity, Expression<Func<T,S>> property, Expression<Func<S, bool>> propertyPredicate = null, DaoLoadInfo daoLoadInfo = null) where S:class
        {
            Assert.NotNull( property, nameof(property));
            MemberExpression mexpr = property.Body as MemberExpression;
            Assert.NotNull (mexpr.Member, "property expression member is null");
            PropertyInfo propertyInfo = typeof(T).GetProperty(mexpr.Member.Name);
            if (propertyInfo == null)
                throw new DaoException("Instance '" + typeof(T).Name + "' dont have a property '"+ mexpr.Member.Name+"'");

            return (S)DaoEntityInterceptor<T>.Instance ().Load<S> (daoSession, entity, propertyInfo, propertyPredicate, daoLoadInfo);
        }

        public IEnumerable<S> LoadCollection<S> (DaoSession daoSession, T entity, Expression<Func<T, IEnumerable<S>>> property, Expression<Func<S, bool>> propertyPredicate = null, DaoLoadInfo daoLoadInfo = null) where S : class
        {
            Assert.NotNull( property, nameof(property));
            MemberExpression mexpr = property.Body as MemberExpression;
            Assert.NotNull (mexpr.Member, "property expression member is null");
            PropertyInfo propertyInfo = typeof(T).GetProperty(mexpr.Member.Name);
            if (propertyInfo == null)
                throw new DaoException("Instance '" + typeof(T).Name + "' dont have a property '" + mexpr.Member.Name + "'");

            return (IEnumerable<S>)DaoEntityInterceptor<T>.Instance ().Load<S> (daoSession, entity, propertyInfo, propertyPredicate, daoLoadInfo);
        }

        private IList<T> WhereCommand (DaoSession daoSession, IDbCommand cmd, DaoLoadInfo daoLoadInfo = null)
		{
			IList<T> ret = null;
            IDataReader dr = null;
            try {
				Assert.NotNull( cmd, nameof(cmd));

                this.OnDaoEvent(daoSession, new DaoEventArgs (typeof(T), null, DaoEventType.BeforeRetrieve));
                this.OnDaoSQLEvent(daoSession, new DaoSQLEventArgs (typeof(T), cmd.GetCommandResult()));
                dr = cmd.ExecuteReader(); //new DisconnectedDataReader(cmd.ExecuteReader ());
                ret = this.RetrieveInstances(daoSession, dr, daoLoadInfo);
                dr.Dispose();
                // Propiedades eager
                if (daoLoadInfo!=null)
                    ret.ToList().ForEach(v=>this.SetNavigatorProperties(daoSession, v, daoLoadInfo));

                return ret;
			} catch (Exception ex) {
				throw new DaoException ("Error executing command '" + cmd.CommandText + "' for entity '" + typeof (T).Name + "'.", ex);
			}
            finally {
                //if (dr != null){
                //    dr.Close();
                //    dr.Dispose();
                //}
            }
        }

        private int NonQueryCommand(DaoSession daoSession, IDbCommand cmd)
        {
            try
            {
                Assert.NotNull( cmd, nameof(cmd));

                this.OnDaoSQLEvent(daoSession, new DaoSQLEventArgs (typeof(T), cmd.GetCommandResult()));

                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new DaoException("Error executing command '" + cmd.CommandText + "' for entity '" + typeof(T).Name + "'.", ex);
            }
        }

        private S SelectAggregation<S>(DaoSession daoSession, IDbCommand cmd) where S : IComparable, IConvertible, IEquatable<S>
		{
			try {
				Assert.NotNull( cmd, nameof(cmd));

                this.OnDaoSQLEvent(daoSession, new DaoSQLEventArgs (typeof(T), cmd.GetCommandResult()));

                DBNullable<S> dbRet= cmd.ExecuteScalar<S>(cmd.CommandText);

				if (dbRet.HasValue)
					return dbRet.Value;
				else
					return default(S);
			} catch (Exception ex) {
				throw new DaoException ("Error executing command '" + cmd.CommandText + "' for entity '" + typeof (T).Name + "'.", ex);
			}
		}

		public IList<T> Where(DaoSession daoSession, String rawSQLCommand, DaoLoadInfo daoLoadInfo = null, int rowCount=0)
		{
			IDbConnection cnx = null;
			IDbCommand cmd = null;
            IDataReader dr = null;
            IList<T> ret = null;

          try
			{
                Assert.NotNull( daoSession, nameof(daoSession));
                Assert.NotNull( rawSQLCommand, nameof(rawSQLCommand));

                cnx = daoSession.DBConnection;

                cmd = cnx.CreateCommand();

				   cmd.CommandText = rawSQLCommand;

                if (rowCount != 0)
                {
                    ISQLParser pf = SQLParserFactory.Instance(daoSession.Context.DBConnector.DBProviderFactory);
                    cmd.CommandText = cmd.CommandText + " ORDER BY " + DaoPropertyPKAttribute.GetPropertyAttributes(typeof(T)).OrderBy(v => v.Order).Select(v => v.ColumnName).ToStringJoin(", ");
                    if (rowCount > 0)
                        cmd.CommandText = pf.Top(cmd.CommandText, rowCount);
                    else
                        cmd.CommandText = pf.Top(cmd.CommandText + " DESC", Math.Abs(rowCount));
                }

                this.OnDaoSQLEvent(daoSession, new DaoSQLEventArgs (typeof(T), cmd.GetCommandResult()));
                dr = cmd.ExecuteReader();// new DisconnectedDataReader(cmd.ExecuteReader());
                ret = this.RetrieveInstances(daoSession, dr, daoLoadInfo);
                dr.Dispose();
                // Propiedades eager
                if (daoLoadInfo != null)
                    ret.ToList().ForEach(v => this.SetNavigatorProperties(daoSession, v, daoLoadInfo));

                return this.RetrieveInstances(daoSession, dr, daoLoadInfo);
			}
			catch (Exception ex)
			{
				throw new DaoException("Error selecting from raw SQL command '" + rawSQLCommand + "' for entity '" + typeof(T).Name + "'.", ex);
			}
			finally
			{
                //if (dr != null){
                //    dr.Close();
                //    dr.Dispose();
                //}

                if (cmd != null)
                    cmd.Dispose();
            }
		}

       private IList<T> RetrieveInstances(DaoSession daoSession, IDataReader dr, DaoLoadInfo daoLoadInfo = null)
		{
			IList<T> ret = null;

			try
			{
                Assert.NotNull( dr, nameof(dr));
                if (daoLoadInfo == null)
                    daoLoadInfo = new DaoLoadInfo(daoSession.Context.DefaultEntityLoadType);

                ret = new List<T>();

                while (dr.Read())
				{
					T retItem;

                    // Castle proxy
                    if (daoLoadInfo.LoadType == DaoEntityLoadType.Lazy) {
                        ProxyGenerationOptions proxyOptions = new ProxyGenerationOptions ();
                        IDaoInstance daoEntityInfo = new DaoInstance (daoSession);
                        daoEntityInfo.__DaoInfo.LoadType = daoLoadInfo.LoadType;
                        daoEntityInfo.__DaoInfo.ChildrenNavigators = daoLoadInfo.ChildrenNavigators;
                        proxyOptions.AddMixinInstance (daoEntityInfo);

                        retItem = ProxyGeneratorFactory.Instance ().CreateClassProxy<T> (proxyOptions, DaoEntityInterceptor<T>.Instance ());
                    } else
                        retItem = Activator.CreateInstance<T> ();

                    foreach (DaoPropertyAttribute dpa in DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v => v.Mode == DaoPropertyMode.Auto))
					{
                        Object value;
						DaoPropertyParserAttribute propertyParser = dpa.PropertyInfo.GetCustomAttribute<DaoPropertyParserAttribute>(true);
						if (propertyParser != null)
							value = propertyParser.Deserialize(daoSession, DaoEntityInfo.Instance<T>().DaoEntityAttribute, dpa, dr);
						else {
                            //if (dr.IsDBNull(dpa.ColumnName)) // Si es nulo
                            if (DBNull.Value.Equals(dr[dpa.ColumnName])) // Si es nulo
								value = null;
							else {
								value = dr[dpa.ColumnName];

								Type nullableType = Nullable.GetUnderlyingType(dpa.PropertyInfo.PropertyType);
								if (nullableType != null) // Si es nullable
								{
									value = Convert.ChangeType(value, nullableType);
									value = Activator.CreateInstance(dpa.PropertyInfo.PropertyType, value);
								}
								else
									value = Convert.ChangeType(value, dpa.PropertyInfo.PropertyType);
							}
						}

						dpa.PropertyInfo.SetValue(retItem, value);
					}

                    // Se ha comentado para hacerlo despues de cargar las propiedades basicas de la instancia
					//this.SetNavigatorProperties(daoSession, retItem, daoLoadInfo);

					ret.Add(retItem);
                    this.OnDaoEvent (daoSession, new DaoEventArgs (typeof (T), retItem, DaoEventType.AfterRetrieve));
                }

				return ret;
			}
			catch (Exception ex)
			{
				throw new DaoException("Error retrieving instances' for entity '" + typeof(T).Name + "'.", ex);
			}
		}

		public void SetNavigatorProperties(DaoSession daoSession, T pkInstance, DaoLoadInfo daoLoadInfo = null) {
			if (daoLoadInfo!=null && daoLoadInfo.ChildrenNavigators != null){
				foreach (String propertyNavigatorName in daoLoadInfo.ChildrenNavigators)
					this.SetNavigatorProperty<Object>(daoSession, pkInstance, propertyNavigatorName);
			}
		}

		public Object SetNavigatorProperty<S>(DaoSession daoSession, T pkInstance, String propertyNavigatorName, Expression<Func<S, bool>> predicate=null, DaoLoadInfo daoLoadInfo = null) where S : class
        {
			try
			{
				DaoNavigationPropertyPKAttribute propertyNavigator = null;
				Object ret = this.GetNavigatorPropertyInstance(daoSession, pkInstance, propertyNavigatorName, out propertyNavigator, predicate, daoLoadInfo);

				// Asignación de propied
				propertyNavigator.PropertyInfo.SetValue(pkInstance, ret);
                return ret;
			}
			catch (Exception ex) {
				throw new DaoNavigationException("Error setting navigation property '" + propertyNavigatorName + "' in instance of type '" + typeof(T).Name + "'."  , ex);
			}
		}

        public Object SetReverseNavigatorProperty<S>(DaoSession daoSession, T fkInstance, String reversePropertyNavigatorName, Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo=null) where S : class
        {
			try
			{
				DaoNavigationPropertyFKAttribute reversePropertyNavigator = null;
				Object ret = this.GetReverseNavigatorPropertyInstance<S>(daoSession, fkInstance, reversePropertyNavigatorName, out reversePropertyNavigator, predicate, daoLoadInfo);

				// Asignación de propied
				reversePropertyNavigator.PropertyInfo.SetValue(fkInstance, ret);

                return ret;
			}
			catch (Exception ex) {
				throw new DaoNavigationException("Error setting reverse navigation property '" + reversePropertyNavigatorName + "' in instance of type '" + typeof(T).Name + "'."  , ex);
			}
		}

        private Object GetNavigatorPropertyInstance<S> (DaoSession daoSession, T pkInstance, String propertyNavigatorName, out DaoNavigationPropertyPKAttribute propertyNavigator, Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo = null) where S : class
        {
			Assert.NotNull( propertyNavigatorName, nameof(propertyNavigatorName));
			propertyNavigator = DaoEntityInfo.Instance<T>().DaoPropertyNavigatorAttributes.OfType<DaoNavigationPropertyPKAttribute>()
				.FirstOrDefault (v => v.PropertyInfo.Name.Equals (propertyNavigatorName));

			if (propertyNavigator == null)
				throw new DaoNavigationException ("Property navigator '" + propertyNavigatorName + "' in type '" + typeof (T).Name + "' is missing.");

			return this.GetNavigatorPropertyInstance<S> (daoSession, pkInstance, propertyNavigator, predicate, daoLoadInfo);
		}

		private Object GetReverseNavigatorPropertyInstance<S>(DaoSession daoSession, T fkInstance, String reversePropertyNavigatorName, out DaoNavigationPropertyFKAttribute reversePropertyNavigator, Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo=null) where S : class
        {
			Assert.NotNull( reversePropertyNavigatorName, nameof(reversePropertyNavigatorName));
			reversePropertyNavigator = DaoEntityInfo.Instance<T>().DaoPropertyNavigatorAttributes.OfType<DaoNavigationPropertyFKAttribute>()
				.FirstOrDefault(v => v.PropertyInfo.Name.Equals(reversePropertyNavigatorName));

			if (reversePropertyNavigator == null)
				throw new DaoNavigationException("Property navigator '" + reversePropertyNavigatorName + "' in type '" + typeof(T).Name + "' is missing.");

			return this.GetReverseNavigatorPropertyInstance<S>(daoSession, fkInstance, reversePropertyNavigator, predicate, daoLoadInfo);
		}

		private Object GetNavigatorPropertyInstance<S>(DaoSession daoSession, T pkInstance, DaoNavigationPropertyPKAttribute propertyNavigator, Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo = null) where S : class
        {
			Object ret = null;
			if (propertyNavigator is DaoNavigationProperty1To1PKAttribute) {
				ret=GetNavigator1To1PKPropertyInstance (daoSession, pkInstance, propertyNavigator, daoLoadInfo);
			} else if (propertyNavigator is DaoNavigationProperty1To1FKAttribute) {
				ret = GetNavigator1To1FKPropertyInstance (daoSession, pkInstance, propertyNavigator, daoLoadInfo);
			} else if (propertyNavigator is DaoNavigationProperty1ToNAttribute) {
				ret = GetNavigator1ToNPropertyInstance (daoSession, pkInstance, propertyNavigator, predicate, daoLoadInfo);
			}

			return ret;
		}

		private IDictionary<String, Object> GetJoinWhere(T pkInstance, Type fkType, DaoNavigationPropertyPKAttribute pkPropertyNavigator, DaoNavigationPropertyFKAttribute fkPropertyNavigator) {

			IDictionary<String, Object> ret = new Dictionary<String, Object>();

			IEnumerable<DaoPropertyPKAttribute> pks = DaoPropertyPKAttribute.GetPropertyAttributes(typeof(T));

			for (int i= 0; i < fkPropertyNavigator.ForeignKeyProperties.Length; i++)
			{
				String fkProperty = fkPropertyNavigator.ForeignKeyProperties[i];
				DaoPropertyAttribute fkPropertyAttribute =
					DaoPropertyAttribute.GetPropertyAttributes(fkType)
					                      .FirstOrDefault(v => fkProperty == v.PropertyInfo.Name);

				if (fkPropertyAttribute == null)
					throw new DaoException("Can't find in type '" + fkType + "' any property with name '" + fkProperty + "'.");
				
				ret.Add(fkPropertyAttribute.ColumnName, pks.FirstOrDefault(v => v.Order == i).PropertyInfo.GetValue(pkInstance));
			}

			return ret;
		}

		private IDictionary<String, Object> GetReverseJoinWhere(T fkInstance, Type pkType, DaoNavigationPropertyPKAttribute pkPropertyNavigator, DaoNavigationPropertyFKAttribute fkPropertyNavigator)
		{

			IDictionary<String, Object> ret = new Dictionary<String, Object>();


				IList<DaoPropertyPKAttribute> pks = DaoPropertyPKAttribute.GetPropertyAttributes(pkType).OrderBy(v => v.Order).ToList();

				for (int i = 0; i < fkPropertyNavigator.ForeignKeyProperties.Length; i++)
					ret.Add(pks[i].ColumnName, typeof(T).GetProperty(fkPropertyNavigator.ForeignKeyProperties[i]).GetValue(fkInstance));


			return ret;
		}

		public Object GetReverseNavigatorPropertyInstance<S>(DaoSession daoSession, T fkInstance, DaoNavigationPropertyFKAttribute bPropertyNavigator, Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo = null) where S : class
        {
			Object ret = null;

			Type aType = bPropertyNavigator.PropertyInfo.PropertyType;

            Object aDaoInstance = DaoHelper.DaoInstance(aType);


            DaoNavigationPropertyPKAttribute aPropertyNavigator =
				DaoNavigationPropertyAttribute.GetNavigatorAttributes(aType)
				.OfType<DaoNavigationPropertyPKAttribute>()
				                              .FirstOrDefault(v => v.PropertyInfo.Name == bPropertyNavigator.ReferencePropertyName);

			if (aPropertyNavigator == null)
				throw new DaoException("Can't find any property in '" + aType.Name + "' type with 'DaoPropertyNavigatorPK' attribute Referenced by Property '" + bPropertyNavigator.PropertyInfo.Name + " in '" + typeof(T).Name + " type'.");

			IDictionary<String, Object> whereFilter = GetReverseJoinWhere(fkInstance, aType, aPropertyNavigator, bPropertyNavigator);

			// Obtenemos el objeto Navigator
			if (aPropertyNavigator is DaoNavigationProperty1ToNAttribute)
			{ // Si es 1ToN
				ret = aDaoInstance.GetType().GetMethod("FirstOrDefault", new Type[] { typeof (DaoSession), typeof (IDictionary<String, Object>),typeof(Expression <Func<S, bool>>), typeof(DaoLoadInfo) })
						.Invoke(aDaoInstance, new Object[] { daoSession, whereFilter, predicate, daoLoadInfo });
				// Asignación de propiedades inversas
				//foreach (Object r in (IEnumerable<Object>)ret)
				//	aPropertyNavigator.PropertyInfo.SetValue(r, fkInstance);
			}
			else { // Si es 1To1PK o 1TONFK
				ret = aDaoInstance.GetType().GetMethod("FirstOrDefault", new Type[] { typeof (DaoSession), typeof (IDictionary<String, Object>), typeof (Expression<Func<S, bool>>), typeof(DaoLoadInfo) })
					.Invoke(aDaoInstance, new Object[] { daoSession, whereFilter, predicate, daoLoadInfo });
				// Asignación de propiedades inversas
				//aPropertyNavigator.PropertyInfo.SetValue(ret, fkInstance);
			}

			return ret;
		}
			
		public Object GetNavigator1To1PKPropertyInstance(DaoSession daoSession, T pkInstance, DaoNavigationPropertyPKAttribute aPropertyNavigator, DaoLoadInfo daoLoadInfo = null)
		{
			Object ret = null;

			Type bType = aPropertyNavigator.PropertyInfo.PropertyType;

            Object bDaoInstance = DaoHelper.DaoInstance(bType);

			DaoNavigationPropertyFKAttribute bPropertyNavigator =
				DaoNavigationPropertyAttribute.GetNavigatorAttributes(bType)
				.OfType<DaoNavigationPropertyFKAttribute>()
				.FirstOrDefault(v=>v.ReferencePropertyName == aPropertyNavigator.PropertyInfo.Name
						&& v.PropertyInfo.PropertyType.Equals(typeof(T)));

			if (bPropertyNavigator == null)
				throw new DaoException("Can't find any property in '" + bType.Name  + "' type with 'DaoPropertyNavigatorFK' attribute and ReferencePropertyName to property '" + aPropertyNavigator.PropertyInfo.Name + " in '" + typeof(T).Name + " type'.");

			IDictionary<String, Object> whereFilter = GetJoinWhere (pkInstance,	bType, aPropertyNavigator, bPropertyNavigator);

            // Obtenemos el objeto Navigator
            ret = bDaoInstance.GetType().GetMethod("FirstOrDefault", new Type[] { typeof (DaoSession), typeof (IDictionary<String, Object>), typeof(DaoLoadInfo)})
                .Invoke(bDaoInstance, new Object[] { daoSession, whereFilter, daoLoadInfo});

			// Asignación de propiedades inversas
			//bPropertyNavigator.PropertyInfo.SetValue(ret, pkInstance);

			return ret;
		}

		public Object GetNavigator1To1FKPropertyInstance(DaoSession daoSession, T pkInstance, DaoNavigationPropertyPKAttribute aPropertyNavigator, DaoLoadInfo daoLoadInfo = null)
		{
			Object ret = null;

			Type bType = aPropertyNavigator.PropertyInfo.PropertyType;

			DaoNavigationPropertyFKAttribute bPropertyNavigator =
				DaoNavigationPropertyAttribute.GetNavigatorAttributes(bType)
				.OfType<DaoNavigationPropertyFKAttribute>()
				.FirstOrDefault(v => v.ReferencePropertyName == aPropertyNavigator.PropertyInfo.Name
						&& v.PropertyInfo.PropertyType.Equals(typeof(T)));

			if (bPropertyNavigator == null)
				throw new DaoException("Can't find any property in '" + bType.Name + "' type with 'DaoPropertyNavigatorFK' attribute and ReferencePropertyName to property '" + aPropertyNavigator.PropertyInfo.Name + " in '" + typeof(T).Name + " type'.");
                
            Object bDaoInstance = DaoHelper.DaoInstance(bType);

            IDictionary<String, Object> whereFilter = GetJoinWhere(pkInstance, bType, aPropertyNavigator, bPropertyNavigator);

			// Obtenemos el objeto Navigator
			ret = bDaoInstance.GetType().GetMethod("FirstOrDefault", new Type[] { typeof (DaoSession), typeof (IDictionary<String, Object>), typeof(DaoLoadInfo) })
						.Invoke(bDaoInstance, new Object[] { daoSession, whereFilter, daoLoadInfo });


			// Asignación de propiedades inversas
			//bPropertyNavigator.PropertyInfo.SetValue(ret, pkInstance);

			return ret;
		}

		public Object GetNavigator1ToNPropertyInstance<S>(DaoSession daoSession, T pkInstance, DaoNavigationPropertyPKAttribute aPropertyNavigator, Expression<Func<S, bool>> predicate = null, DaoLoadInfo daoLoadInfo = null) where S:class
		{
			Object ret = null;
		
			Type bType = aPropertyNavigator.PropertyInfo.PropertyType.GetGenericArguments()[0];

			DaoNavigationPropertyFKAttribute bPropertyNavigator =
				DaoNavigationPropertyAttribute.GetNavigatorAttributes(bType)
											  .OfType<DaoNavigationPropertyFKAttribute>()
				.FirstOrDefault(v => v.ReferencePropertyName == aPropertyNavigator.PropertyInfo.Name
					&& v.PropertyInfo.PropertyType.Equals(typeof(T)));

			if (bPropertyNavigator == null)
				throw new DaoException("Can't find any property in '" + bType.Name + "' type with 'DaoPropertyNavigatorFK' attribute and ReferencePropertyName to property '" + aPropertyNavigator.PropertyInfo.Name + " in '" + typeof(T).Name + " type'.");

            Object bDaoInstance = DaoHelper.DaoInstance(bType);

            IDictionary<String, Object> whereFilter = GetJoinWhere(pkInstance, bType, aPropertyNavigator, bPropertyNavigator);

            // Obtenemos el objeto Navigator
            Type exprType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(bType, typeof(bool)));
            MethodInfo mi = bDaoInstance.GetType().GetMethod("Where", new Type[] { typeof(DaoSession), typeof(IDictionary<String, Object>), exprType, typeof(DaoLoadInfo) });

            ret =mi.Invoke(bDaoInstance, new Object[] { daoSession, whereFilter, predicate,  daoLoadInfo});


            // Asignación de propiedades inversas
            //foreach(Object r in (IEnumerable<Object>)ret)
            //	bPropertyNavigator.PropertyInfo.SetValue(r, pkInstance);

            return ret;
		}

        protected void OnDaoEvent(DaoSession daoSession, DaoEventArgs args) {
            if (daoSession.Context.Properties.ContainsKey("daoEventHandler")
                && daoSession.Context.Properties["daoEventHandler"]!=null) {
                try
                {
                    ((DaoEventHandler)daoSession.Context.Properties["daoEventHandler"]).Invoke(this, args);
                }
                catch { }
            }
        }

        protected void OnDaoSQLEvent(DaoSession daoSession, DaoSQLEventArgs args)
        {
            if (daoSession.Context.Properties.ContainsKey("daoSQLEventHandler")
                && daoSession.Context.Properties["daoSQLEventHandler"] != null)
            {
                try
                {
                    ((DaoSQLEventHandler)daoSession.Context.Properties["daoSQLEventHandler"]).Invoke(this, args);
                }
                catch { }
            }
        }


        public Object[] GetIdFromEntity(T entity)
        {
            IList<Object> binaryId = new List<Object>();

            IEnumerable<DaoPropertyPKAttribute> pkAttributes =DaoEntityInfo.Instance<T>().DaoPropertyAttributes.Where(v=>v is DaoPropertyPKAttribute).Cast<DaoPropertyPKAttribute>();
            foreach (DaoPropertyPKAttribute pkAttribute in pkAttributes.OrderBy(v => v.Order))
                binaryId.Add(pkAttribute.PropertyInfo.GetValue(entity));

            return binaryId.ToArray();
        }

        public DaoStreamProperty<T> GetStreamProperty(DaoSession daoSession, T entity, DaoPropertyAttribute daoPropertyAttribute) {
            return new DaoStreamProperty<T>(daoSession, entity, DaoEntityInfo.Instance<T>().DaoEntityAttribute, daoPropertyAttribute);
        }

        public DaoStreamProperty<T> GetStreamProperty (DaoSession daoSession, T entity, Expression<Func<T, byte[]>> propertyExpression)
        {
            return new DaoStreamProperty<T> (daoSession, entity, DaoEntityInfo.Instance<T>().DaoEntityAttribute, propertyExpression);
        }

        public static DaoPropertyAttribute PropertyAtributeFromPropertyExpression<S>(Expression<Func<T, S>> propertyExpression)
        {
            DaoPropertyAttribute ret = null;
            var lambda = (LambdaExpression)propertyExpression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression) {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            } else {
                memberExpression = (MemberExpression)lambda.Body;
            }

            ret = DaoPropertyAttribute.GetPropertyAttributes (typeof (T))
                .FirstOrDefault (v => v.PropertyInfo.Name == memberExpression.Member.Name);

            if (ret == null)
                throw new DaoException ("Property '" + memberExpression.Member.Name + "' have not DaoPropertyAttribute.");

            return ret;
        }

    }
}
