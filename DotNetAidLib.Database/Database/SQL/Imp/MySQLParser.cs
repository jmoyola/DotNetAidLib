using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.SQL.Imp
{
    public class MySQLParser : ISQLParser
    {
        private static ISQLParser _Instance;

        private MySQLParser()
        {
        }

        public string ProviderFactoryName => "MySqlClientFactory";

        public string LastInsertId()
        {
            return "Last_Insert_Id()";
        }

        public string CreateSchema(string schemaName, bool ifNotExists)
        {
            return "CREATE SCHEMA" + (ifNotExists ? " IF NOT EXISTS" : "") + " `" + schemaName + "`";
        }

        public string CreateSchema(DataSet dSet, bool ifNotExists)
        {
            return CreateSchema(dSet.DataSetName, ifNotExists);
        }

        public string DropSchema(string schemaName, bool ifExists)
        {
            return "DROP SCHEMA" + (ifExists ? " IF EXISTS" : "") + " `" + schemaName + "`";
        }

        public string DropSchema(DataSet dSet, bool ifExists)
        {
            return DropSchema(dSet.DataSetName, ifExists);
        }

        public string CreateTable(DataTable dTable, bool ifNotExists)
        {
            var ret = "CREATE TABLE" + (ifNotExists ? " IF NOT EXISTS" : "") + " `" + dTable.DataSet.DataSetName +
                      "`.`" + dTable.TableName + "` (";

            foreach (DataColumn dc in dTable.Columns)
                ret += DataColumnParse(dc) + ", ";

            ret += "PRIMARY KEY(" + string.Join(", ", dTable.PrimaryKey.Select(v => v.ColumnName)) + ")";
            ret += ")";

            return ret;
        }

        public string DataColumnParse(DataColumn dc)
        {
            var ret = dc.ColumnName + " ";
            if (dc.DataType.Equals(typeof(string)))
            {
                if (dc.MaxLength == -1)
                    ret += "TEXT";
                else
                    ret += "VARCHAR(" + dc.MaxLength + ")";
            }
            else if (dc.DataType.Equals(typeof(DateTime)))
            {
                ret += "DATETIME";
            }
            else if (dc.DataType.Equals(typeof(bool)))
            {
                ret += "BIT";
            }
            else if (dc.DataType.Equals(typeof(ulong)))
            {
                ret += "UNSIGNED BIGINT";
            }
            else if (dc.DataType.Equals(typeof(long)))
            {
                ret += "BIGINT";
            }
            else if (dc.DataType.Equals(typeof(uint)))
            {
                ret += "UNSIGNED INT";
            }
            else if (dc.DataType.Equals(typeof(int)))
            {
                ret += "INT";
            }
            else if (dc.DataType.Equals(typeof(ushort)))
            {
                ret += "UNSIGNED SMALLINT";
            }
            else if (dc.DataType.Equals(typeof(short)))
            {
                ret += "SMALLINT";
            }
            else if (dc.DataType.Equals(typeof(byte)))
            {
                ret += "TINYINT";
            }
            else if (dc.DataType.Equals(typeof(float)))
            {
                ret += "FLOAT";
            }
            else if (dc.DataType.Equals(typeof(double)))
            {
                ret += "DOUBLE";
            }
            else if (dc.DataType.Equals(typeof(decimal)))
            {
                ret += "DECIMAL";
            }
            else if (dc.DataType.IsEnum)
            {
                ret += "ENUM(" + Enum.GetValues(dc.DataType).Cast<string>().Select(v => "'" + v + "'")
                    .ToStringJoin(", ") + ")";
            }
            else if (dc.DataType.Equals(typeof(byte[])))
            {
                ret += "LONGBLOB";
            }
            else
            {
                throw new Exception("Data column type '" + dc.DataType.Name + "' is not allowed for SQLParser '" +
                                    GetType().Name + "'.");
            }

            ret += " " + (dc.AllowDBNull ? "" : "NOT ") + "NULL";

            if (dc.DefaultValue != null && !string.IsNullOrEmpty(dc.DefaultValue.ToString()))
                ret += " DEFAULT " + DataColumnValueParse(dc, dc.DefaultValue);

            return ret;
        }

        public string DataColumnValueParse(DataColumn dc, object value)
        {
            var ret = "";
            if (value == null)
            {
                ret = "NULL";
            }
            else if (dc.DataType.Equals(typeof(string)))
            {
                ret += "'" + value + "'";
            }
            else if (dc.DataType.Equals(typeof(DateTime)))
            {
                ret += "'" + ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            }
            else if (dc.DataType.IsEnum)
            {
                ret += "'" + (Enum) value + "'";
            }
            else
            {
                var cultureAux = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                ret += value.ToString();
                Thread.CurrentThread.CurrentCulture = cultureAux;
            }

            return ret;
        }

        public string ActiveSchema(string schemaName)
        {
            return "USE " + schemaName + ";SET SESSION sql_mode = '';";
        }

        public IEnumerable<DbIndex> GetTableIndexes(IDbConnection cnx, string schemaName, string tableName)
        {
            var ret = new List<DbIndex>();

            IDbConnection cnxAux = null;

            try
            {
                cnxAux = cnx.CloneConnection();

                var dtUniqueIndex = cnxAux.CreateCommand()
                    .ExecuteDataTable(@"SELECT INDEX_NAME, NON_UNIQUE, COLUMN_NAME, NULLABLE, SEQ_IN_INDEX
             FROM information_schema.statistics WHERE TABLE_schema = " + schemaName + " and TABLE_name = " +
                                      tableName + ";", false);

                var consPerName =
                    dtUniqueIndex
                        .AsEnumerable()
                        .GroupBy(v => v["INDEX_NAME"].ToString());

                foreach (var cons in consPerName)
                {
                    var dbIndex = new DbIndex();
                    ret.Add(dbIndex);

                    dbIndex.IndexName = cons.Key;

                    var columns = new List<DbIndexColumn>();
                    foreach (var drCons in cons.OrderBy(v => int.Parse(v["SEQ_IN_INDEX"].ToString())))
                        columns.Add(new DbIndexColumn
                        {
                            ColumnName = drCons["COLUMN_NAME"].ToString(), Nullable = (bool) drCons["NULLABLE"],
                            Unique = !(bool) drCons["NON_UNIQUE"]
                        });
                    dbIndex.Columns = columns.ToArray();

                    dbIndex.IsPKIndex = columns.All(v => !v.Nullable && v.Unique);
                }
            }
            catch (Exception ex)
            {
                throw new SQLException(
                    "Error getting table indexes from database '" + schemaName + "', table name '" + tableName + "'.",
                    ex);
            }
            finally
            {
                try
                {
                    cnxAux.Close();
                }
                catch
                {
                }
            }

            return ret;
        }


        public IEnumerable<DbConstrait> GetTableConstraits(IDbConnection cnx, string schemaName, string tableName)
        {
            var ret = new List<DbConstrait>();

            IDbConnection cnxAux = null;

            try
            {
                cnxAux = cnx.CloneConnection();


                var dtForeignKeys = cnxAux.CreateCommand()
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
where c.CONSTRAINT_SCHEMA = " + schemaName + " AND c.TABLE_NAME = " + tableName + ";", false);

                var consPerName =
                    dtForeignKeys
                        .AsEnumerable()
                        .GroupBy(v => v["CONSTRAINT_NAME"].ToString());

                foreach (var cons in consPerName)
                {
                    var dbConstrait = new DbConstrait();
                    dbConstrait.ConstraitName = cons.Key;
                    ret.Add(dbConstrait);

                    var referenceColumns = new List<DbConstraitReferenceColumn>();
                    foreach (var drCons in cons.OrderBy(v => int.Parse(v["ORDINAL_POSITION"].ToString())))
                        referenceColumns.Add(new DbConstraitReferenceColumn
                        {
                            ColumnName = drCons["COLUMN_NAME"].ToString(),
                            ReferenceColumnName = drCons["REFERENCED_COLUMN_NAME"].ToString()
                        });
                    dbConstrait.ReferenceColumns = referenceColumns.ToArray();

                    if (cons.Count() > 0)
                    {
                        dbConstrait.ReferenceTableName = cons.FirstOrDefault()["REFERENCED_TABLE_NAME"].ToString();
                        dbConstrait.DeleteRule = SqlRuleToDsRule(cons.FirstOrDefault()["DELETE_RULE"].ToString());
                        dbConstrait.UpdateRule = SqlRuleToDsRule(cons.FirstOrDefault()["UPDATE_RULE"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SQLException(
                    "Error getting table constraits from database '" + schemaName + "', table name '" + tableName +
                    "'.", ex);
            }
            finally
            {
                try
                {
                    cnxAux.Close();
                }
                catch
                {
                }
            }

            return ret;
        }

        public string CurrentId(string tableName, string columnName)
        {
            var fqnTableName = tableName.Split('.');
            if (fqnTableName.Length == 1)
                return "SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = '" + tableName +
                       "' AND table_schema = DATABASE();";
            return "SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = '" +
                   fqnTableName[fqnTableName.Length - 1] + "' AND table_schema = '" +
                   fqnTableName[fqnTableName.Length - 2] + "';";
        }

        public string ChangeCurrentId(string tableName, string columnName, ulong id)
        {
            return "ALTER TABLE " + tableName + " AUTO_INCREMENT=" + id + ";";
        }

        public SQLUploaderFile GetContentFromFile(IDbConnection cnx, SourceType fileSource, string path)
        {
            return new MySQLSQLUploaderFile(cnx, fileSource, path);
        }

        public string Top(string select, int rowCount)
        {
            return select.Trim() + " LIMIT " + rowCount;
        }

        public string SequenceNextValue(string sequenceName)
        {
            return "nextval('" + sequenceName + "')";
        }

        public string SequenceCurrentValue(string sequenceName)
        {
            return "currval('" + sequenceName + "')";
        }

        public string SelectSchemas()
        {
            return "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA;";
        }

        private static DbConstraitRule SqlRuleToDsRule(string sqlRule)
        {
            if (sqlRule.ToUpper().Contains("NULL"))
                return DbConstraitRule.SET_NULL;
            if (sqlRule.ToUpper().Contains("CASCADE"))
                return DbConstraitRule.CASCADE;
            if (sqlRule.ToUpper().Contains("NO"))
                return DbConstraitRule.NO_ACTION;
            if (sqlRule.ToUpper().Contains("RESTRICT"))
                return DbConstraitRule.RESTRICT;
            return DbConstraitRule.NO_ACTION;
        }

        public static ISQLParser Instance()
        {
            if (_Instance == null)
                _Instance = new MySQLParser();

            return _Instance;
        }
    }
}