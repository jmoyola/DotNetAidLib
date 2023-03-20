using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.SQL.Imp
{
	public class MySQLParser:ISQLParser
	{
		private static ISQLParser _Instance = null;

		private MySQLParser() { }

		public String ProviderFactoryName {
			get { return "MySqlClientFactory";}
		}

        public String LastInsertId() {
            return "Last_Insert_Id()";
        }

        public String CreateSchema(String schemaName, bool ifNotExists)
        {
			return "CREATE SCHEMA" + (ifNotExists?" IF NOT EXISTS":"") + " `" + schemaName + "`";  
		}

		public String CreateSchema(DataSet dSet, bool ifNotExists) {
			return this.CreateSchema(dSet.DataSetName, ifNotExists);
		}

        public String DropSchema(String schemaName, bool ifExists)
		{
			return "DROP SCHEMA" + (ifExists ? " IF EXISTS" : "") + " `" + schemaName + "`";
		}

		public String DropSchema(DataSet dSet, bool ifExists)
		{
			return this.DropSchema(dSet.DataSetName, ifExists);
		}

		public String CreateTable(DataTable dTable, bool ifNotExists)
        {
			String ret= "CREATE TABLE" + (ifNotExists?" IF NOT EXISTS":"") + " `" + dTable.DataSet.DataSetName + "`.`" + dTable.TableName + "` (";

			foreach (DataColumn dc in dTable.Columns)
				ret += DataColumnParse (dc) + ", ";

			ret += "PRIMARY KEY(" +String.Join(", ", dTable.PrimaryKey.Select(v=>v.ColumnName)) + ")";
			ret+=")";

			return ret;
		}

        public String DataColumnParse(DataColumn dc)
        {
			String ret = dc.ColumnName + " ";
			if (dc.DataType.Equals(typeof(String)))
			{
				if (dc.MaxLength == -1)
					ret += "TEXT";
				else
					ret += "VARCHAR(" + dc.MaxLength + ")";
			}
			else if (dc.DataType.Equals(typeof(DateTime)))
				ret += "DATETIME";
			else if (dc.DataType.Equals(typeof(bool)))
				ret += "BIT";
			else if (dc.DataType.Equals(typeof(UInt64)))
				ret += "UNSIGNED BIGINT";
			else if (dc.DataType.Equals(typeof(Int64)))
				ret += "BIGINT";
			else if (dc.DataType.Equals(typeof(UInt32)))
				ret += "UNSIGNED INT";
			else if (dc.DataType.Equals(typeof(Int32)))
				ret += "INT";
			else if (dc.DataType.Equals(typeof(UInt16)))
				ret += "UNSIGNED SMALLINT";
			else if (dc.DataType.Equals(typeof(Int16)))
				ret += "SMALLINT";
			else if (dc.DataType.Equals(typeof(byte)))
				ret += "TINYINT";
			else if (dc.DataType.Equals(typeof(float)))
				ret += "FLOAT";
			else if (dc.DataType.Equals(typeof(double)))
				ret += "DOUBLE";
			else if (dc.DataType.Equals(typeof(decimal)))
				ret += "DECIMAL";
			else if (dc.DataType.IsEnum)
				ret += "ENUM(" + Enum.GetValues(dc.DataType).Cast<String>().Select(v => "'" + v + "'").ToStringJoin(", ") + ")";
			else if (dc.DataType.Equals(typeof(byte[])))
				ret += "LONGBLOB";
			else
				throw new Exception("Data column type '" + dc.DataType.Name + "' is not allowed for SQLParser '" + this.GetType().Name + "'.");

			ret += " " + (dc.AllowDBNull ? "" : "NOT ") + "NULL";

			if(dc.DefaultValue != null && !String.IsNullOrEmpty(dc.DefaultValue.ToString()))
				ret += " DEFAULT " + DataColumnValueParse(dc, dc.DefaultValue);

            return ret;
		}

		public String DataColumnValueParse(DataColumn dc, Object value)
		{
			String ret = "";
			if (value == null)
				ret = "NULL";
			else if (dc.DataType.Equals(typeof(String)))
			{
				ret += "'" + value.ToString() + "'";
			}
			else if (dc.DataType.Equals(typeof(DateTime)))
				ret += "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
			else if (dc.DataType.IsEnum)
				ret += "'" + ((Enum)value).ToString() + "'";
			else {
				CultureInfo cultureAux = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				ret += value.ToString();
				Thread.CurrentThread.CurrentCulture = cultureAux;
			}
			return ret;
		}
        public String ActiveSchema(String schemaName)
        {
			return "USE " + schemaName +";SET SESSION sql_mode = '';";
        }

        public String SelectSchemas()
        {
            return "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA;";
        }

        public IEnumerable<DbIndex> GetTableIndexes(IDbConnection cnx, String schemaName, String tableName)
        {
            List<DbIndex> ret = new List<DbIndex>();

            IDbConnection cnxAux = null;

            try
            {
                cnxAux = cnx.CloneConnection();

                DataTable dtUniqueIndex = cnxAux.CreateCommand()
                    .ExecuteDataTable(@"SELECT INDEX_NAME, NON_UNIQUE, COLUMN_NAME, NULLABLE, SEQ_IN_INDEX
             FROM information_schema.statistics WHERE TABLE_schema = " + schemaName + " and TABLE_name = " + tableName + ";", false);

                IEnumerable<IGrouping<String, DataRow>> consPerName =
                    dtUniqueIndex
                        .AsEnumerable()
                        .GroupBy(v => v["INDEX_NAME"].ToString());

                foreach (IGrouping<String, DataRow> cons in consPerName)
                {
                    DbIndex dbIndex = new DbIndex();
                    ret.Add(dbIndex);

                    dbIndex.IndexName = cons.Key;

                    List<DbIndexColumn> columns = new List<DbIndexColumn>();
                    foreach (DataRow drCons in cons.OrderBy(v => Int32.Parse(v["SEQ_IN_INDEX"].ToString()))){
                        columns.Add(new DbIndexColumn() { ColumnName = drCons["COLUMN_NAME"].ToString(), Nullable= (Boolean)drCons["NULLABLE"], Unique = !(Boolean)drCons["NON_UNIQUE"] });
                    }
                    dbIndex.Columns = columns.ToArray();

                    dbIndex.IsPKIndex = columns.All(v => !v.Nullable && v.Unique);
                }
            }
            catch (Exception ex)
            {
                throw new SQLException("Error getting table indexes from database '" + schemaName + "', table name '" + tableName + "'.", ex);
            }
            finally
            {
                try
                {
                    cnxAux.Close();
                }
                catch { }
            }
            return ret;
        }



        public IEnumerable<DbConstrait> GetTableConstraits(IDbConnection cnx, String schemaName, String tableName)
        {
            List<DbConstrait> ret = new List<DbConstrait>();

            IDbConnection cnxAux = null;

            try
            {
                cnxAux = cnx.CloneConnection();


                DataTable dtForeignKeys = cnxAux.CreateCommand()
                    .ExecuteDataTable(@"
select c.CONSTRAINT_NAME,
 ck.COLUMN_NAME,
 ck.ORDINAL_POSITION,
 ck.REFERENCED_TABLE_SCHEMA,
 ck.REFERENCED_TABLE_NAME,
 ck.REFERENCED_COLUMN_NAME,
 rk.UPDATE_RULE,
 rk.DELETE_RULE
from information_schema.table_constraints as c
 inner join information_schema.key_column_usage ck
    on c.constraint_name=ck.constraint_name
    and c.constraint_schema=ck.constraint_schema
    and c.constraint_catalog=ck.constraint_catalog
 inner join information_schema.referential_constraints rk
    on c.constraint_name=rk.constraint_name
    and c.constraint_schema=rk.constraint_schema
    and c.constraint_catalog=rk.constraint_catalog
where c.CONSTRAINT_SCHEMA = " + schemaName + " AND c.TABLE_NAME = " + tableName +";", false);

                IEnumerable<IGrouping<String, DataRow>> consPerName =
                    dtForeignKeys
                        .AsEnumerable()
                        .GroupBy(v => v["CONSTRAINT_NAME"].ToString());

                foreach (IGrouping<String, DataRow> cons in consPerName)
                {
                    DbConstrait dbConstrait = new DbConstrait();
                    dbConstrait.ConstraitName = cons.Key;
                    ret.Add(dbConstrait);

                    List<DbConstraitReferenceColumn> referenceColumns = new List<DbConstraitReferenceColumn>();
                    foreach (DataRow drCons in cons.OrderBy(v => Int32.Parse(v["ORDINAL_POSITION"].ToString()))){

                        referenceColumns.Add(new DbConstraitReferenceColumn(){ ColumnName= drCons["COLUMN_NAME"].ToString(), ReferenceColumnName= drCons["REFERENCED_COLUMN_NAME"].ToString()});
                    }
                    dbConstrait.ReferenceColumns = referenceColumns.ToArray();

                    if (cons.Count() > 0){
                        dbConstrait.ReferenceTableName = cons.FirstOrDefault()["REFERENCED_TABLE_NAME"].ToString();
                        dbConstrait.DeleteRule= SqlRuleToDsRule(cons.FirstOrDefault()["DELETE_RULE"].ToString());
                        dbConstrait.UpdateRule = SqlRuleToDsRule(cons.FirstOrDefault()["UPDATE_RULE"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SQLException("Error getting table constraits from database '" + schemaName + "', table name '" + tableName + "'.", ex);
            }
            finally
            {
                try
                {
                    cnxAux.Close();
                }
                catch { }
            }
            return ret;
        }

        private static DbConstraitRule SqlRuleToDsRule(String sqlRule)
        {
            if (sqlRule.ToUpper().Contains("NULL"))
                return DbConstraitRule.SET_NULL;
            else if (sqlRule.ToUpper().Contains("CASCADE"))
                return DbConstraitRule.CASCADE;
            else if (sqlRule.ToUpper().Contains("NO"))
                return DbConstraitRule.NO_ACTION;
            else if (sqlRule.ToUpper().Contains("RESTRICT"))
                return DbConstraitRule.RESTRICT;
            else
                return DbConstraitRule.NO_ACTION;
        }

        public String CurrentId(String tableName, String columnName)
		{
			String[] fqnTableName = tableName.Split('.');
			if (fqnTableName.Length==1){
				return "SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = '" + tableName + "' AND table_schema = DATABASE();";
			}
			else{
				return "SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = '" + fqnTableName[fqnTableName.Length-1] + "' AND table_schema = '" + fqnTableName[fqnTableName.Length - 2] + "';";
			}
		}

		public String ChangeCurrentId(String tableName, String columnName, UInt64 id)
		{
			return "ALTER TABLE " + tableName + " AUTO_INCREMENT=" + id + ";";
		}

        public SQLUploaderFile GetContentFromFile(IDbConnection cnx, SourceType fileSource, String path) {
            return new MySQLSQLUploaderFile(cnx, fileSource, path);
        }

        public String Top(String select, int rowCount) {
            return select.Trim() + " LIMIT " + rowCount;
        }

        public String SequenceNextValue(String sequenceName) {
            return "nextval('" + sequenceName + "')";
        }

        public String SequenceCurrentValue(String sequenceName)
        {
            return "currval('" + sequenceName + "')";
        }

        public static ISQLParser Instance() {
			if (_Instance == null)
				_Instance = new MySQLParser();

			return _Instance;
		}
	}
}

