using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using DotNetAidLib.Core.Cmd;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Database.DbProviders;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database
{
    public static class DataHelpers
    {
        public static bool Test(this IDbConnection v)
        {
            var ret = false;

            var cnxTest = v.CloneConnection();
            try
            {
                cnxTest.Open();
                ret = true;
            }
            catch
            {
                ret = false;
            }
            finally
            {
                try
                {
                    cnxTest.Close();
                }
                catch
                {
                }
            }

            return ret;
        }

        public static string ToStringValues(this DataRow v)
        {
            var ret = new List<string>();

            var t = v.Table;
            ret.AddRange(t.PrimaryKey.Select(pk =>
                "[" + (v[pk.ColumnName] == DBNull.Value ? "<null>" : v[pk.ColumnName].ToString()) + "]"));
            ret.AddRange(t.Columns.Cast<DataColumn>()
                .Where(c1 => t.PrimaryKey.IndexOf(c1) == -1)
                .Select(c2 => v[c2.ColumnName] == DBNull.Value ? "<null>" : v[c2.ColumnName].ToString()));
            return ret.ToStringJoin(", ");
        }

        public static string GetCommandResult(this IDbCommand v)
        {
            return v.GetCommandResult(v.CommandText);
        }

        public static string GetCommandResult(this IDbCommand v, int fieldMaxLength)
        {
            return v.GetCommandResult(v.CommandText, fieldMaxLength);
        }

        public static string GetCommandResult(this IDbCommand v, string commandText)
        {
            return v.GetCommandResult(commandText, -1);
        }

        public static string GetCommandResult(this IDbCommand v, string commandText, int fieldMaxLength)
        {
            foreach (IDataParameter param in v.Parameters)
                commandText = Regex.Replace(commandText, "(" + Regex.Escape(param.ParameterName) + @"\b)",
                    m => param.Value.ToSQLValue(fieldMaxLength));

            return commandText;
        }

        public static DbProviderFactory GetDbProviderFactory(this IDbConnection v)
        {
            var ret = DbProviderFactories.GetFactory((DbConnection) v);
            return ret;
        }

        public static IDbCommand AddCommandText(this IDbCommand v, string commandText)
        {
            v.CommandText = commandText;
            return v;
        }

        public static IDbCommand CreateCommand(this IDbConnection v, string commandText)
        {
            var ret = v.CreateCommand();
            ret.CommandText = commandText;
            return ret;
        }

        public static IDbConnection CloneConnection(this IDbConnection v, bool includeDatabase = true)
        {
            var dbcsb = v.GetDbProviderFactory().CreateConnectionStringBuilder();
            dbcsb.ConnectionString = v.ConnectionString;
            if (!includeDatabase)
                foreach (var k in new[] {"database", "db"})
                    if (dbcsb.ContainsKey(k)) // Busca ignorando mayusculas
                        dbcsb.Remove(k);

            IDbConnection ret = v.GetDbProviderFactory().CreateConnection();
            ret.ConnectionString = dbcsb.ConnectionString;
            return ret;
        }

        public static IEnumerable<string> Schemas(this IDbConnection v)
        {
            return v.CloneConnection().CreateCommand()
                .ExecuteList("SELECT s.SCHEMA_NAME FROM information_schema.SCHEMATA s;",
                    t => t["SCHEMA_NAME"].ToString(),
                    true);
        }

        public static IEnumerable<string> Tables(this IDbConnection v, string schema)
        {
            return v.CloneConnection().CreateCommand()
                .AddParameter("@schema", schema)
                .ExecuteList("SELECT t.TABLE_NAME FROM information_schema.TABLES t where t.TABLE_SCHEMA = @schema;",
                    t => schema + t["TABLE_NAME"],
                    true);
        }

        public static IEnumerable<string> Columns(this IDbConnection v, string schema, string tableName)
        {
            return v.CloneConnection().CreateCommand()
                .AddParameter("@schema", schema)
                .AddParameter("@tableName", tableName)
                .ExecuteList(
                    "SELECT c.COLUMN_NAME FROM information_schema.COLUMNS c where c.TABLE_SCHEMA = @schema and c.TABLE_NAME = @tableName;",
                    t => schema + t["COLUMN_NAME"],
                    true);
        }

        public static IEnumerable<string> Columns(this IDbConnection v, string fqnTableName)
        {
            var cmd = v.CloneConnection().CreateCommand();

            var afqnTableName = fqnTableName.Split('.');
            if (afqnTableName.Length < 2)
                throw new SQLException("TableName must be full qualify name.");

            return v.CloneConnection().CreateCommand()
                .AddParameter("@schema", afqnTableName[afqnTableName.Length - 2])
                .AddParameter("@tableName", afqnTableName[afqnTableName.Length - 1])
                .ExecuteList(
                    "SELECT c.COLUMN_NAME FROM information_schema.COLUMNS c where c.TABLE_SCHEMA = @schema and c.TABLE_NAME = @tableName;",
                    t => t["COLUMN_NAME"].ToString(),
                    true);
        }

        public static int ExecuteNonQuery(this IDbCommand v, string sqlCommand)
        {
            return v.ExecuteNonQuery(sqlCommand, false);
        }

        public static int ExecuteNonQuery(this IDbCommand v, string sqlCommand, bool closeConnectionAfterExecution)
        {
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();
            var ret = v.ExecuteNonQuery();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static DBNullable<T> ExecuteScalar<T>(this IDbCommand v, string sqlCommand)
        {
            return v.ExecuteScalar<T>(sqlCommand, false);
        }

        public static DBNullable<T> ExecuteScalar<T>(this IDbCommand v, string sqlCommand,
            bool closeConnectionAfterExecution)
        {
            object ret = null;

            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            ret = v.ExecuteScalar();

            if (closeConnectionAfterExecution)
                v.Connection.Close();

            if (ret == null)
                return null;
            return new DBNullable<T>(ret);
        }

        public static IDataReader ExecuteReader(this IDbCommand v, string sqlCommand)
        {
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            return v.ExecuteReader();
        }

        public static bool IsDBNull(this IDataReader v, string columnName)
        {
            return v.IsDBNull(v.GetOrdinal(columnName));
        }

        public static object GetValue(this IDataReader v, string columnName)
        {
            return v.GetValue(v.GetOrdinal(columnName));
        }

        public static IList<DbRow> ExecuteListValues(this IDbCommand v, string sqlCommand)
        {
            return v.ExecuteListValues(sqlCommand, false);
        }

        public static IList<DbRow> ExecuteListValues(this IDbCommand v, string sqlCommand,
            bool closeConnectionAfterExecution)
        {
            var ret = new List<DbRow>();
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader();
            var fields = DbField.FromDataReader(dr);
            while (dr.Read())
            {
                var row = new DbRow(fields);
                for (var i = 0; i < dr.FieldCount; i++)
                    row.Add(dr[i]);
                ret.Add(row);
            }

            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static IList<DBNullable<T>> ExecuteScalarColumn<T>(this IDbCommand v, string sqlCommand, int columnIndex)
        {
            return v.ExecuteScalarColumn<T>(sqlCommand, columnIndex, false);
        }

        public static IList<DBNullable<T>> ExecuteScalarColumn<T>(this IDbCommand v, string sqlCommand, int columnIndex,
            bool closeConnectionAfterExecution)
        {
            var ret = new List<DBNullable<T>>();
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader();
            while (dr.Read()) ret.Add(new DBNullable<T>(dr.GetValue(columnIndex)));
            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static IList<DBNullable<T>> ExecuteScalarColumn<T>(this IDbCommand v, string sqlCommand,
            string columnName)
        {
            return v.ExecuteScalarColumn<T>(sqlCommand, columnName, false);
        }

        public static IList<DBNullable<T>> ExecuteScalarColumn<T>(this IDbCommand v, string sqlCommand,
            string columnName, bool closeConnectionAfterExecution)
        {
            var ret = new List<DBNullable<T>>();
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader();
            while (dr.Read()) ret.Add(new DBNullable<T>(dr.GetValue(dr.GetOrdinal(columnName))));
            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static DbRow ExecuteScalarRow(this IDbCommand v, string sqlCommand)
        {
            return v.ExecuteScalarRow(sqlCommand, false);
        }

        public static DbRow ExecuteScalarRow(this IDbCommand v, string sqlCommand, bool closeConnectionAfterExecution,
            bool nullIfNoResults = false)
        {
            var ret = new DbRow();
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader();
            if (dr.Read())
            {
                ret = new DbRow(DbField.FromDataReader(dr));
                for (var c = 0; c < dr.FieldCount; c++)
                    ret.Add(dr.GetValue(c));
            }
            else if (nullIfNoResults)
            {
                ret = null;
            }


            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static IList<T> ExecuteList<T>(this IDbCommand v, string sqlCommand, Func<IDataReader, T> transform)
        {
            return v.ExecuteList(sqlCommand, transform, false);
        }

        public static IList<T> ExecuteList<T>(this IDbCommand v, string sqlCommand, Func<IDataReader, T> transform,
            bool closeConnectionAfterExecution)
        {
            var ret = new List<T>();
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader();
            while (dr.Read()) ret.Add(transform.Invoke(dr));
            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static IDictionary<K, V> ExecuteDictionary<K, V>(this IDbCommand v, string sqlCommand,
            Func<IDataReader, KeyValuePair<K, V>> transform)
        {
            return v.ExecuteDictionary(sqlCommand, transform, false);
        }

        public static IDictionary<K, V> ExecuteDictionary<K, V>(this IDbCommand v, string sqlCommand,
            Func<IDataReader, KeyValuePair<K, V>> transform, bool closeConnectionAfterExecution)
        {
            var ret = new Dictionary<K, V>();
            v.CommandText = sqlCommand;
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader();
            while (dr.Read())
            {
                var kv = transform.Invoke(dr);
                ret.Add(kv.Key, kv.Value);
            }

            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static void GetFile(this IDataReader v, string columnName, FileInfo outputFile)
        {
            v.GetFile(v.GetOrdinal(columnName), outputFile);
        }

        public static void GetFile(this IDataReader v, int ordinal, FileInfo outputFile)
        {
            FileStream fs = null;

            try
            {
                fs = outputFile.Create();
                v.CopyTo(ordinal, fs);
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from database to file.", ex);
            }
            finally
            {
                fs.Close();
            }
        }

        public static byte[] GetAllBytes(this IDataReader v, string columnName)
        {
            return v.GetAllBytes(v.GetOrdinal(columnName));
        }

        public static byte[] GetAllBytes(this IDataReader v, int ordinal)
        {
            var ms = new MemoryStream();
            try
            {
                v.CopyTo(ordinal, ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from database to array.", ex);
            }
            finally
            {
                ms.Close();
            }
        }

        public static void CopyTo(this IDataReader v, int ordinal, Stream outputStream)
        {
            v.CopyTo(ordinal, outputStream, 1024, true);
        }

        public static void CopyTo(this IDataReader v, int ordinal, Stream outputStream, int bufferLength,
            bool autoFlush)
        {
            try
            {
                var buffer = new byte[bufferLength];
                long bytesLeidos = 0;
                long fieldOffset = 0;

                bytesLeidos = v.GetBytes(ordinal, fieldOffset, buffer, 0, buffer.Length);

                while (bytesLeidos == buffer.Length)
                {
                    outputStream.Write(buffer, 0, (int) bytesLeidos);
                    if (autoFlush)
                        outputStream.Flush();
                    fieldOffset += bytesLeidos;
                    bytesLeidos = v.GetBytes(ordinal, fieldOffset, buffer, 0, buffer.Length);
                }

                outputStream.Write(buffer, 0, (int) bytesLeidos);
                if (autoFlush)
                    outputStream.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from database.", ex);
            }
        }

        public static void CopyTo(this IDataReader v, int ordinal, StreamsHelpers.WriteBytesTransferHandler destination,
            int bufferLength)
        {
            try
            {
                var buffer = new byte[bufferLength];
                long bytesLeidos = 0;
                long fieldOffset = 0;

                bytesLeidos = v.GetBytes(ordinal, fieldOffset, buffer, 0, buffer.Length);

                while (bytesLeidos == buffer.Length)
                {
                    destination.Invoke(buffer, 0, (int) bytesLeidos);
                    fieldOffset += bytesLeidos;
                    bytesLeidos = v.GetBytes(ordinal, fieldOffset, buffer, 0, buffer.Length);
                }

                destination.Invoke(buffer, 0, (int) bytesLeidos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from database.", ex);
            }
        }

        public static Stream GetStream(this IDataReader v, int ordinal)
        {
            return new DataReaderStream(v, ordinal);
        }

        public static StreamReader GetStreamReader(this IDataReader v, int ordinal)
        {
            return v.GetStreamReader(ordinal, Encoding.UTF8);
        }

        public static StreamReader GetStreamReader(this IDataReader v, int ordinal, Encoding encoding)
        {
            return new StreamReader(new DataReaderStream(v, ordinal), encoding);
        }

        public static DataTable ExecuteDataTable(this IDbCommand v, string sqlCommand)
        {
            return v.ExecuteDataTable(sqlCommand, false);
        }

        public static DataTable ExecuteDataTable(this IDbCommand v, string sqlCommand,
            bool closeConnectionAfterExecution)
        {
            if (!v.Connection.State.Equals(ConnectionState.Open))
                v.Connection.Open();

            var dr = v.ExecuteReader(sqlCommand);
            var ret = new DataTable();
            ret.Load(dr);
            dr.Close();
            if (closeConnectionAfterExecution)
                v.Connection.Close();
            return ret;
        }

        public static IDbCommand AddParameter(this IDbCommand v, IDbDataAdapter parameter)
        {
            v.Parameters.Add(parameter);
            return v;
        }

        public static IDbCommand AddParameter(this IDbCommand v, string parameterName, object parameterValue)
        {
            var param = v.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = parameterValue;
            v.Parameters.Add(param);
            return v;
        }

        public static object[] PrimaryKeyValues(this DataRow dr)
        {
            var dcPks = dr.Table.PrimaryKey;

            var ret = new object[dcPks.Length];

            for (var iDc = 0; iDc <= dcPks.Length - 1; iDc++)
                ret[iDc] = dr[dcPks[iDc].Ordinal];

            return ret;
        }

        public static object[] Values(this DataRow dr, IList<string> columnNames)
        {
            var ret = new object[columnNames.Count];

            for (var i = 0; i <= columnNames.Count; i++)
                ret[i] = dr[columnNames[i]];

            return ret;
        }

        public static object[] Values(this DataRow dr, IList<int> columnIndexes)
        {
            var ret = new object[columnIndexes.Count];

            for (var i = 0; i <= columnIndexes.Count; i++)
                ret[i] = dr[columnIndexes[i]];

            return ret;
        }

        public static int RelationalLevel2(this DataTable dt)
        {
            return dt.RelationalLevel2(dt);
        }

        private static int RelationalLevel2(this DataTable dt, DataTable recursiveBaseDataTable)
        {
            if (dt.ParentRelations.Count == 0) // Si no tengo padres, mi nivel es 0
                return 0;
            // Si tengo padres, mi nivel es el mayor nivel de mis padres (que no sea el base que se consulta en recurividad) + 1 (si yo soy mi único padre, mi nivel es 0).
            return dt.ParentRelations.Cast<DataRelation>().Max(v =>
                v.ParentTable.Equals(recursiveBaseDataTable)
                    ? 0
                    : v.ParentTable.RelationalLevel2(recursiveBaseDataTable)) + 1;
        }

        public static int RelationalLevelOld(this DataTable dt)
        {
            if (dt.ParentRelations.Count == 0) // Si no tengo padres, mi nivel es 0
                return 0;
            // Si tengo padres, mi nivel es el mayor nivel de mis padres (que no sea yo) + 1 (si yo soy mi único padre, mi nivel es 0).
            return dt.ParentRelations.Cast<DataRelation>()
                .Max(v => v.ParentTable.Equals(dt) ? 0 : v.ParentTable.RelationalLevel()) + 1;
        }

        public static int RelationalLevel(this DataTable dt, IList<DataTable> stack = null)
        {
            // Gestión del stack para evitar llamadas recursivas
            if (stack == null) // Si es la primera ejecución, se crea el stack de llamada y se agrega el dt
                stack = new List<DataTable> {dt};
            else if
                (!stack.Contains(dt)) // Si no es la primera ejecución y no se encuentra dt entre el stack, se agrega
                stack.Add(dt);
            else // Si no es la primera ejecución y se encuentra dt entre el stack ES LLAMADA RECURSIVA, devolvemos 0
                return 0;

            if (dt.ParentRelations.Count == 0) // Si no tengo padres, mi nivel es 0
                return 0;
            // Si tengo padres, mi nivel es el mayor nivel de mis padres (que no sea yo) + 1 (si yo soy mi único padre, mi nivel es 0).
            return dt.ParentRelations.Cast<DataRelation>()
                .Max(v => v.ParentTable.Equals(dt) ? 0 : v.ParentTable.RelationalLevel(stack)) + 1;
        }

        public static IList<DataTable> HierarchicalDataTableOrder(this DataSet ds)
        {
            var orTables = ds.Tables.Cast<DataTable>().ToList();
            orTables.Sort((a, b) => a.RelationalLevel() - b.RelationalLevel());
            return orTables;
        }

        public static void WriteXml(this IDataReader dr, string tableName, XmlWriter xw)
        {
            dr.WriteXml(tableName, xw, null, null);
        }

        public static void WriteXml(this IDataReader dr, string tableName, XmlWriter xw, string prefix, string ns)
        {
            dr.WriteXml(tableName, xw, prefix, ns, false, DateTimeOffset.Now.Offset);
        }

        public static void WriteXml(this IDataReader dr, string tableName, XmlWriter xw, string prefix, string ns,
            bool blobsInHexadecimal, TimeSpan defaultTimeOffset)
        {
            while (dr.Read())
            {
                xw.WriteStartElement(prefix, tableName, ns);

                for (var fieldIndex = 0; fieldIndex < dr.FieldCount; fieldIndex++)
                    DataReaderFieldToXml(dr, fieldIndex, xw, prefix, ns, blobsInHexadecimal, defaultTimeOffset);

                xw.WriteEndElement();
            }
        }

        private static void DataReaderFieldToXml(IDataReader dr, int fieldIndex, XmlWriter xw, string prefix, string ns,
            bool blobsInHexadecimal, TimeSpan defaultTimeOffset)
        {
            if (!dr.IsDBNull(fieldIndex))
            {
                xw.WriteStartElement(prefix, dr.GetName(fieldIndex), ns);

                if (dr.GetFieldType(fieldIndex) == typeof(byte[]))
                {
                    if (blobsInHexadecimal)
                        dr.CopyTo(fieldIndex, (buffer, offset, length) => xw.WriteBinHex(buffer, offset, length), 1024);
                    else
                        dr.CopyTo(fieldIndex, (buffer, offset, length) => xw.WriteBase64(buffer, offset, length), 1024);
                }
                else if (dr.GetFieldType(fieldIndex) == typeof(DateTime))
                {
                    var d = (DateTime) dr.GetValue(fieldIndex);
                    xw.WriteValue(new DateTimeOffset(d.Ticks, defaultTimeOffset));
                }
                else
                {
                    xw.WriteValue(dr.GetValue(fieldIndex));
                }

                xw.WriteEndElement();
            }
        }

        public static DataTable RowDifferencesPK(this DataTable tBase, DataTable tToCompare)
        {
            var tDiferencias = tToCompare.Clone();
            foreach (DataRow dR in tToCompare.Rows)
            {
                var ret = dR.PrimaryKeyValues();
                if (!tBase.Rows.Contains(ret))
                    tDiferencias.ImportRow(dR);
            }

            return tDiferencias;
        }

        public static DataTable RowDifferences(this DataTable tBase, DataTable tToCompare, IList<string> tableNames)
        {
            var tDiferencias = tToCompare.Clone();
            foreach (DataRow drToCompare in tToCompare.Rows)
            {
                var ret = drToCompare.Values(tableNames);
                try
                {
                    if (!tBase.AsEnumerable().Any(drTBase =>
                            drToCompare.Values(tableNames).EqualsAll(drTBase.Values(tableNames))))
                        tDiferencias.ImportRow(drToCompare);
                }
                catch
                {
                    // SI hay error (no se encuentra alguna columna en algún datarow), la damos por direfencia
                    tDiferencias.ImportRow(drToCompare);
                }
            }

            return tDiferencias;
        }

        public static void FillDataAllTables(this DataSet ds, IDbConnection cnx)
        {
            var tables = ds
                .Tables.Cast<DataTable>()
                .Select(v => v.TableName);
            ds.FillDataIncludingTables(cnx, tables);
        }

        public static void FillDataExcludingTables(this DataSet ds, IDbConnection cnx,
            IEnumerable<DataTable> dataTablesToExclude)
        {
            IList<DataTable> tables = ds
                .Tables.Cast<DataTable>()
                .ToList();
            foreach (var tableToExclude in dataTablesToExclude)
                tables.Remove(tableToExclude);
            ds.FillDataIncludingTables(cnx, tables);
        }

        public static void FillDataExcludingTables(this DataSet ds, IDbConnection cnx,
            IEnumerable<string> dataTablesToExclude)
        {
            IList<DataTable> tables = ds
                .Tables.Cast<DataTable>()
                .ToList();
            foreach (var tableToExclude in dataTablesToExclude)
                tables.Remove(ds.Tables[tableToExclude]);
            ds.FillDataIncludingTables(cnx, tables);
        }

        public static void FillDataIncludingTables(this DataSet ds, IDbConnection cnx,
            IEnumerable<DataTable> dataTables)
        {
            var tables = dataTables
                .Where(v => v.DataSet.Equals(ds))
                .Select(v => v.TableName);
            ds.FillDataIncludingTables(cnx, tables);
        }

        public static void FillDataIncludingTables(this DataSet ds, IDbConnection cnx, IEnumerable<string> tables)
        {
            IDbConnection cnxAux = null;
            string tableName = null;
            string databaseName = null;

            bool auxDsEnforceConstraints;
            try
            {
                auxDsEnforceConstraints = ds.EnforceConstraints;

                databaseName = ds.DataSetName;

                cnxAux = cnx.CloneConnection();

                var dbProviderFactory = cnx.GetDbProviderFactory();
                var da = dbProviderFactory.CreateDataAdapter();

                ds.EnforceConstraints = false;

                foreach (var table in tables)
                {
                    tableName = table;
                    ds.Tables[table].Clear();

                    da.SelectCommand = (DbCommand) cnxAux.CreateCommand(
                        "SELECT * FROM " + databaseName + "." + table + ";");
                    da.Fill(ds, tableName);
                }

                ds.EnforceConstraints = auxDsEnforceConstraints;
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error filling data: " + ex.Message, ex);
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
        }

        public static void FillSchemaAllTables(this DataSet ds, IDbConnection cnx, bool includeconstraints)
        {
            IDbConnection cnxAux = null;
            string tableName = null;
            string databaseName = null;

            try
            {
                ds.Clear();

                databaseName = ds.DataSetName;

                cnxAux = cnx.CloneConnection();

                IList<string> databaseTables = null;

                databaseTables = cnxAux.CreateCommand()
                    .AddParameter("@table_schema", databaseName)
                    .ExecuteList(
                        "SELECT table_name FROM information_schema.TABLES where table_schema=@table_schema;",
                        v => v.GetString(0), true);

                ds.FillSchemaIncludingTables(cnx, databaseTables, includeconstraints);
            }
            catch (Exception ex)
            {
                throw new ArchiveException(
                    "Error filling schema from database '" + databaseName + "', table name '" + tableName + "'.", ex);
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
        }

        public static void FillSchemaExcludingTables(this DataSet ds, IDbConnection cnx,
            IEnumerable<string> excludingDatabaseTables, bool includeconstraints)
        {
            IDbConnection cnxAux = null;
            string tableName = null;
            string databaseName = null;

            try
            {
                ds.Clear();

                databaseName = ds.DataSetName;

                cnxAux = cnx.CloneConnection();

                IList<string> databaseTables = null;

                databaseTables = cnxAux.CreateCommand()
                    .AddParameter("@table_schema", databaseName)
                    .ExecuteList(
                        "SELECT table_name FROM information_schema.TABLES where TABLE_TYPE='BASE TABLE' AND table_schema=@table_schema;",
                        v => v.GetString(0), false);

                foreach (var excludingDatabaseTable in excludingDatabaseTables)
                    if (databaseTables.IndexOf(excludingDatabaseTable) > -1)
                        databaseTables.Remove(excludingDatabaseTable);

                ds.FillSchemaIncludingTables(cnx, databaseTables, includeconstraints);
            }
            catch (Exception ex)
            {
                throw new ArchiveException(
                    "Error filling schema from database '" + databaseName + "', table name '" + tableName + "'.", ex);
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
        }

        public static void FillSchemaIncludingTables(this DataSet ds, IDbConnection cnx,
            IEnumerable<string> includingDatabaseTables, bool includeconstraints)
        {
            ds.FillSchemaIncludingTables(cnx, includingDatabaseTables, includeconstraints, false);
        }

        public static void FillSchemaIncludingTables(this DataSet ds, IDbConnection cnx,
            IEnumerable<string> includingDatabaseTables, bool includeconstraints, bool errorIfNotExist)
        {
            IDbConnection cnxAux = null;
            string tableName = null;
            string databaseName = null;

            try
            {
                ds.Reset();

                databaseName = ds.DataSetName;

                cnxAux = cnx.CloneConnection();

                var dbProviderFactory = cnx.GetDbProviderFactory();
                var da = dbProviderFactory.CreateDataAdapter();

                foreach (var table in includingDatabaseTables)
                {
                    tableName = table;
                    try
                    {
                        cnxAux.Open();
                        da.SelectCommand = (DbCommand) cnxAux.CreateCommand(
                            "SELECT * FROM " + databaseName + "." + table + " WHERE false;");
                        da.Fill(ds, table);
                        da.FillSchema(ds, SchemaType.Source, tableName);
                    }
                    catch (Exception ex)
                    {
                        if (errorIfNotExist)
                            throw ex;
                    }
                    finally
                    {
                        if (cnxAux != null && cnxAux.State != ConnectionState.Closed)
                            cnxAux.Close();
                    }
                }

                if (includeconstraints)
                    ds.FillConstraints(cnx);
            }
            catch (Exception ex)
            {
                throw new ArchiveException(
                    "Error filling schema from database '" + databaseName + "', table name '" + tableName + "'.", ex);
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
        }

        public static void FillConstraints(this DataSet ds, IDbConnection cnx)
        {
            IDbConnection cnxAux = null;
            string tableName = null;
            var databaseName = ds.DataSetName;

            try
            {
                cnxAux = cnx.CloneConnection();

                // Le cambiamos el nombre a las constraint unique que son primary key
                foreach (DataTable dt in ds.Tables)
                {
                    //if(dt.TableName=="detalledeemailings")
                    //    Thread.Sleep(1);

                    var pkCons = dt.Constraints.Cast<Constraint>()
                        .FirstOrDefault(v =>
                            typeof(UniqueConstraint).IsAssignableFrom(v.GetType()) &&
                            ((UniqueConstraint) v).IsPrimaryKey);

                    if (pkCons != null && pkCons.ConstraintName != "PRIMARY")
                        pkCons.ConstraintName = "PRIMARY";
                    //if(pkCons!=null)
                    //    pkCons.ConstraintName = "uqpk_" + pkCons.Table.TableName;
                }


                // Creamos las constraints de Indices únicos
                for (var i = 0; i < ds.Tables.Count; i++)
                {
                    tableName = ds.Tables[i].TableName;

                    //if(tableName=="detalledeemailings")
                    //    Thread.Sleep(1);

                    cnxAux.Open();
                    var dtUniqueIndex = cnxAux.CreateCommand()
                        .AddParameter("@databaseName", databaseName)
                        //.AddParameter("@catalog", catalog)
                        .AddParameter("@tableName", tableName)
                        .ExecuteDataTable(@"
SELECT 
 TABLE_SCHEMA,
 TABLE_NAME,
 INDEX_NAME,
 NON_UNIQUE,
 COLUMN_NAME,
 NULLABLE,
 SEQ_IN_INDEX
FROM information_schema.statistics
WHERE TABLE_schema = @databaseName
 and TABLE_name=@tableName
 and NOT INDEX_NAME='PRIMARY';"
                            , true);
                    var consPerName =
                        dtUniqueIndex
                            .AsEnumerable()
                            .Where(v => v["NON_UNIQUE"].ToString().Equals("0"))
                            .GroupBy(v => v["INDEX_NAME"].ToString());

                    foreach (var cons in consPerName)
                    {
                        var indexName = cons.Key;
                        var dTable = ds.Tables[tableName];
                        var columns = new List<DataColumn>();

                        foreach (var drCons in cons.OrderBy(v => int.Parse(v["SEQ_IN_INDEX"].ToString())))
                            columns.Add(dTable.Columns[drCons["COLUMN_NAME"].ToString()]);

                        var oConstraint = new UniqueConstraint(indexName, columns.ToArray(), false);
                        try
                        {
                            // Se mete en un try por que da error si hay en una tabla dos constraints asociadas
                            // a la misma clave pero una siendo una pk y otra unique
                            ds.Tables[tableName].Constraints.Add(oConstraint);
                        }
                        catch
                        {
                        }
                    }
                }

                // Creamos las constraints de ForeignKey
                for (var i = 0; i < ds.Tables.Count; i++)
                {
                    tableName = ds.Tables[i].TableName;

                    cnxAux.Open();
                    var dtForeignKeys = cnxAux.CreateCommand()
                        .AddParameter("@databaseName", databaseName)
                        //.AddParameter("@catalog", catalog)
                        .AddParameter("@tableName", tableName)
                        .ExecuteDataTable(@"
select
 c.CONSTRAINT_SCHEMA,
 c.CONSTRAINT_NAME,
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
    and if(isnull(c.constraint_catalog), 1, c.constraint_catalog=ck.constraint_catalog)
 inner join information_schema.referential_constraints rk
    on c.constraint_name=rk.constraint_name
    and c.constraint_schema=rk.constraint_schema
    and if(isnull(c.constraint_catalog), 1, c.constraint_catalog=rk.constraint_catalog)
where
 c.CONSTRAINT_SCHEMA = @databaseName
 AND c.TABLE_NAME=@tableName;"
                            , true);
                    var consPerName =
                        dtForeignKeys
                            .AsEnumerable()
                            .GroupBy(v => v["CONSTRAINT_NAME"].ToString());

                    foreach (var cons in consPerName)
                    {
                        var foreigKeyName = cons.Key;
                        var dTable = ds.Tables[tableName];
                        var columns = new List<DataColumn>();
                        var referencedColumns = new List<DataColumn>();

                        foreach (var drCons in cons.OrderBy(v => int.Parse(v["ORDINAL_POSITION"].ToString())))
                        {
                            // Si la tabla no existe en el dataset, no rellenamos ninguna columna
                            if (!ds.Tables.Contains(drCons["REFERENCED_TABLE_NAME"].ToString()))
                                break;
                            var dReferencedTable = ds.Tables[drCons["REFERENCED_TABLE_NAME"].ToString()];
                            columns.Add(dTable.Columns[drCons["COLUMN_NAME"].ToString()]);
                            referencedColumns.Add(
                                dReferencedTable.Columns[drCons["REFERENCED_COLUMN_NAME"].ToString()]);
                        }

                        try
                        {
                            if (columns.Count > 0)
                            {
                                // Si hay columnas (es que la tabla referenciada se encuentra en el dataset)
                                var oConstraint = new ForeignKeyConstraint(foreigKeyName,
                                    referencedColumns.ToArray(), columns.ToArray());
                                oConstraint.AcceptRejectRule = AcceptRejectRule.Cascade;
                                oConstraint.DeleteRule = SqlRuleToDsRule(cons.First()["DELETE_RULE"].ToString());
                                oConstraint.UpdateRule = SqlRuleToDsRule(cons.First()["UPDATE_RULE"].ToString());
                                ds.Tables[tableName].Constraints.Add(oConstraint);

                                // Establecemos las relaciones padres - hija en el dataset
                                var parentRelation = new DataRelation(foreigKeyName,
                                    referencedColumns.ToArray(), columns.ToArray());
                                ds.Relations.Add(parentRelation);
                            }
                        }
                        // try por que puede existir ya una foreign key a la misma tabla y mismo id con diferente nombre
                        catch
                        {
                        }
                    }
                }


                ds.DataSetName = databaseName;
            }
            catch (Exception ex)
            {
                throw new ArchiveException(
                    "Error creating schema from database '" + databaseName + "', table name '" + tableName + "'.", ex);
            }
            finally
            {
                try
                {
                    if (cnxAux != null && cnxAux.State != ConnectionState.Closed)
                        cnxAux.Close();
                }
                catch
                {
                }
            }
        }


        private static Rule SqlRuleToDsRule(string sqlRule)
        {
            if (sqlRule.ToUpper().Contains("NULL"))
                return Rule.SetNull;
            if (sqlRule.ToUpper().Contains("CASCADE"))
                return Rule.Cascade;
            if (sqlRule.ToUpper().Contains("NO"))
                return Rule.None;
            return Rule.None;
        }

        public static void WriteXmlTableDataCommands(this DataSet ds, string filePath, XmlWriteMode mode,
            IDictionary<string, IDbCommand> tableDataCommands)
        {
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    ds.WriteXmlTableDataCommands(fs, mode, tableDataCommands);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }
        }

        public static void WriteXmlTableDataCommands(this DataSet ds, Stream st, XmlWriteMode mode,
            IDictionary<string, IDbCommand> tableDataCommands)
        {
            XmlTextWriter xmlTextWriter = null;
            try
            {
                xmlTextWriter = new XmlTextWriter(st, Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;

                xmlTextWriter.WriteStartDocument(true);
                xmlTextWriter.WriteStartElement(ds.DataSetName);

                if (mode == XmlWriteMode.WriteSchema)
                    ds.WriteXmlSchema(xmlTextWriter);

                foreach (var tableCommand in tableDataCommands)
                {
                    IDataReader dr = null;

                    var cnxState = tableCommand.Value.Connection.State;

                    if (tableCommand.Value.Connection.State != ConnectionState.Open)
                        tableCommand.Value.Connection.Open();

                    try
                    {
                        dr = tableCommand.Value.ExecuteReader();
                        dr.WriteXml(tableCommand.Key, xmlTextWriter);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (dr != null && !dr.IsClosed)
                            dr.Close();
                        if (cnxState == ConnectionState.Closed)
                            tableCommand.Value.Connection.Close();
                    }
                }

                xmlTextWriter.WriteEndElement();
                xmlTextWriter.WriteEndDocument();

                xmlTextWriter.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing to xml dinamic command.", ex);
            }
            finally
            {
                if (xmlTextWriter != null)
                    xmlTextWriter.Dispose();
            }
        }


        public static string FullName(this DataColumn v)
        {
            return v.Table.FullName() + "." + v.ColumnName;
        }

        public static string FullName(this DataTable v)
        {
            return v.DataSet.DataSetName + "." + v.TableName;
        }

        public static string ToStringDefinition(this DataSet ds)
        {
            var ret = "";
            ret += "DATASET '" + ds.DataSetName + "' DEFINITION:" + Environment.NewLine;
            ret += ds.Tables.Cast<DataTable>().OrderBy(v => v.TableName).Select(v => v.ToStringDefinition())
                .ToStringJoin(Environment.NewLine);

            return ret;
        }

        public static string ToStringDefinition(this DataTable dt)
        {
            var ret = "";
            ret += "DATATABLE '" + dt.TableName + "' DEFINITION:" + Environment.NewLine;

            // Primary keys
            ret += "- PRIMARY KEYS: " + dt.PrimaryKey.Select((c, i) => c.ColumnName + "(" + i + ")").ToStringJoin(", ");
            ret += Environment.NewLine;

            // Constraints
            ret += "- CONSTRAINTS:" + Environment.NewLine;
            ret += dt.Constraints.Cast<Constraint>().Select(c => " - " + c.ToStringDefinition())
                .ToStringJoin(Environment.NewLine);
            ret += Environment.NewLine;
            // Relations
            ret += "- PARENT RELATIONS:" + Environment.NewLine;
            ret += dt.ParentRelations.Cast<DataRelation>().Select(v => " - " + v.ToStringDefinition())
                .ToStringJoin(Environment.NewLine);
            ret += Environment.NewLine;
            ret += "- CHILD RELATIONS:" + Environment.NewLine;
            ret += dt.ChildRelations.Cast<DataRelation>().Select(v => " - " + v.ToStringDefinition())
                .ToStringJoin(Environment.NewLine);
            ret += Environment.NewLine;

            // Encabezados de columnas
            ret += "- COLUMNS:" + Environment.NewLine;
            ret += dt.Columns.OfType<DataColumn>().Select(v => " - " + v.ToStringDefinition())
                .ToStringJoin(Environment.NewLine);

            return ret;
        }

        public static string ToStringDefinition(this DataRelation dr)
        {
            var ret = "";

            ret += dr.RelationName;
            ret += " (";
            ret += dr.ParentTable.TableName + " [" + dr.ParentColumns.Select(c => c.ColumnName).ToStringJoin(", ") +
                   "]";
            ret += " = ";
            ret += dr.ChildTable.TableName + " [" + dr.ChildColumns.Select(c => c.ColumnName).ToStringJoin(", ") + "]";
            ret += ")";

            return ret;
        }

        public static string ToStringDefinition(this Constraint c)
        {
            var ret = "";
            if (c is UniqueConstraint)
            {
                var uqc = (UniqueConstraint) c;
                ret += "CONSTRAINT " + uqc.ConstraintName
                                     + " UNIQUE (" + uqc.Columns.Select(dc => dc.ColumnName).ToStringJoin(", ") + ")";
            }
            else if (c is ForeignKeyConstraint)
            {
                var fkc = (ForeignKeyConstraint) c;
                ret += "CONSTRAINT " + fkc.ConstraintName
                                     + " FOREIGN KEY (" + fkc.Columns.Select(fk => fk.ColumnName).ToStringJoin(", ") +
                                     ")"
                                     + " REFERENCES `" + fkc.RelatedTable.TableName + "`(" +
                                     fkc.RelatedColumns.Select(fk => fk.ColumnName).ToStringJoin(", ") + ")"
                                     + " ON UPDATE " + fkc.UpdateRule + " ON DELETE " + fkc.DeleteRule;
            }
            else
            {
                ret += "CONSTRAINT UNKNOW " + c.ConstraintName;
            }

            return ret;
        }

        public static string ToStringDefinition(this DataColumn dc)
        {
            return dc.ColumnName + " " +
                   dc.DataType.Name + (dc.MaxLength > 0 ? "(" + dc.MaxLength + ")" : "") +
                   (dc.DataTypeIsUnsigned() ? " UN" : "") +
                   (dc.AutoIncrement ? " AI_" + dc.AutoIncrementStep : "") +
                   (dc.Table.PrimaryKey.Contains(dc) ? " PK" + "(" + dc.Table.PrimaryKey.IndexOf(dc) + ")" : "") +
                   (dc.Unique ? " UQ" : "") +
                   (!dc.AllowDBNull ? " NN" : "") +
                   (DBNull.Value.Equals(dc.DefaultValue) ? "" : " DEF (" + dc.DefaultValue + ")") +
                   (string.IsNullOrEmpty(dc.Expression) ? "" : " EXP (" + dc.Expression + ")");
        }

        public static bool DataTypeIsUnsigned(this DataColumn dc)
        {
            if (dc.DataType.Equals(typeof(ushort))
                || dc.DataType.Equals(typeof(uint))
                || dc.DataType.Equals(typeof(ulong)))
                return true;
            return false;
        }

        public static string ToStringData(this DataSet ds)
        {
            return ds.ToStringData('|', false);
        }

        public static string ToStringData(this DataSet ds, bool showBinary)
        {
            return ds.ToStringData('|', showBinary);
        }

        public static string ToStringData(this DataSet ds, char separator, bool showBinary)
        {
            var ret = "DATASET '" + ds.DataSetName + "' DATA:";
            foreach (var dt in ds.Tables.Cast<DataTable>().OrderBy(v => v.TableName))
                ret += Environment.NewLine + dt.ToStringData(separator, showBinary);
            return ret;
        }

        public static string ToStringData(this DataTable dt)
        {
            return dt.ToStringData('|', false);
        }

        public static string ToStringData(this DataTable dt, char separator, bool showBinary)
        {
            var ret = "";

            ret = (dt.HasErrors ? "!" : "") + "DATATABLE '" + dt.TableName + "' DATA:";

            // Encabezados de columnas
            ret += Environment.NewLine;
            ret += dt.Columns.OfType<DataColumn>().Select(v => v.ColumnName).ToStringJoin("" + separator);
            ret += Environment.NewLine;
            // Filas
            ret += dt.Rows.OfType<DataRow>()
                .Select(v =>
                    Environment.NewLine + (v.HasErrors ? "!" : " ") + v.ToStringData(separator, showBinary) +
                    (string.IsNullOrEmpty(v.RowError) ? "" : v.RowError)).ToStringJoin(Environment.NewLine);

            return ret;
        }

        public static string ToStringData(this DataRow dr)
        {
            return dr.ToStringData('|', false);
        }

        public static string ToStringData(this DataRow dr, char separator, bool showBinary)
        {
            var ret = new List<string>();
            for (var columnIndex = 0; columnIndex < dr.Table.Columns.Count; columnIndex++)
                ret.Add(dr.ToStringData(columnIndex, showBinary));

            return ret.ToStringJoin("" + separator);
        }

        public static string ToStringData(this DataRow dr, string columnName)
        {
            return dr.ToStringData(columnName, false);
        }

        public static string ToStringData(this DataRow dr, string columnName, bool showBinary)
        {
            return dr.ToStringData(dr.Table.Columns.IndexOf(columnName), showBinary);
        }

        public static string ToStringData(this DataColumn dc, DataRow dataRow)
        {
            return dc.ToStringData(dataRow, false);
        }

        public static string ToStringData(this DataColumn dc, DataRow dataRow, bool showBinary)
        {
            Assert.NotNull(dataRow, nameof(dataRow));
            if (!dc.Table.Equals(dataRow.Table))
                throw new Exception("DataColumn table '" + dc.Table.TableName + "' is not from same DataRow table.");
            return dataRow.ToStringData(dataRow.Table.Columns.IndexOf(dc), showBinary);
        }

        public static string ToStringData(this DataRow dr, int columnIndex)
        {
            return dr.ToStringData(columnIndex, false);
        }

        public static string ToStringData(this DataRow dr, int columnIndex, bool showBinary)
        {
            var ret = "";

            if (DBNull.Value.Equals(dr[columnIndex]))
            {
                ret += "<null/>";
            }
            else
            {
                var cultureAux = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                if (dr[columnIndex] is DateTime)
                {
                    ret += ((DateTime) dr[columnIndex]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (dr[columnIndex] is byte[])
                {
                    var binary = (byte[]) dr[columnIndex];
                    if (showBinary)
                        ret += "<binary length=\"" + binary.Length + "\">" + binary.ToHexadecimal() + "</binary>";
                    else
                        ret += "<binary length=\"" + binary.Length + "\"/>";
                }
                else
                {
                    ret += dr[columnIndex].ToString();
                }

                Thread.CurrentThread.CurrentCulture = cultureAux;
            }

            var columnErr = dr.GetColumnError(columnIndex);
            if (!string.IsNullOrEmpty(columnErr))
                ret += "<error description=\"" + columnErr + "\">" + ret + "</error>";

            return ret;
        }

        public static bool TableExists(this IDbConnection cnx, string tableName)
        {
            return cnx.CloneConnection()
                .CreateCommand()
                .AddParameter("@table_schema", cnx.Database)
                .AddParameter("@table_name", tableName)
                .ExecuteScalar<int>(
                    "SELECT count(table_name) FROM information_schema.TABLES where table_schema=@table_schema AND table_name=@table_name;"
                    , false) > 0;
        }

        public static void DictionaryTableCreate(this IDbConnection cnx, string tableName)
        {
            cnx.CloneConnection()
                .CreateCommand()
                .ExecuteNonQuery("CREATE TABLE "
                                 + tableName
                                 + " (Key varchar(256) PRIMARY KEY, Value text);", true);
        }

        public static bool DictionaryTableKeyExists(this IDbConnection cnx, string tableName, string key)
        {
            return cnx.CloneConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteScalar<int>("SELECT count(Key) FROM "
                                    + tableName
                                    + " WHERE Key=@key;", true) > 0;
        }

        public static T? DictionaryTableGet<T>(this IDbConnection cnx, string tableName, string key) where T : struct
        {
            return cnx.CloneConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteScalar<T>("SELECT value FROM "
                                  + tableName
                                  + " WHERE Key=@key;", true);
        }

        public static void DictionaryTableSet(this IDbConnection cnx, string tableName, string key, object value)
        {
            var n = cnx.CloneConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .AddParameter("@value", value == null || value.Equals(DBNull.Value) ? null : value.ToString())
                .ExecuteNonQuery("UPDATE "
                                 + tableName
                                 + " SET Value=@value WHERE Key=@key;", true);
            if (n == 0)
                cnx.DictionaryTableAdd(tableName, key, value);
        }

        public static void DictionaryTableDelete(this IDbConnection cnx, string tableName, string key)
        {
            cnx.CloneConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .ExecuteNonQuery("DELETE FROM "
                                 + tableName
                                 + " WHERE Key=@key;", true);
        }

        public static void DictionaryTableAdd(this IDbConnection cnx, string tableName, string key, object value)
        {
            cnx.CloneConnection()
                .CreateCommand()
                .AddParameter("@key", key)
                .AddParameter("@value", value == null || value.Equals(DBNull.Value) ? null : value.ToString())
                .ExecuteNonQuery("INSERT INTO "
                                 + tableName
                                 + " (Key, Value) "
                                 + " VALUES "
                                 + " (@key, @value);", true);
        }

        public static int Index(this DataRow v)
        {
            return v.Table.Rows.IndexOf(v);
        }

        public static IEnumerable<DataRow> Extract(this DataTable v, int begin)
        {
            return v.Extract(begin, v.Rows.Count - begin);
        }

        public static IEnumerable<DataRow> Extract(this DataTable v, int begin, int count)
        {
            IList<DataRow> ret = new List<DataRow>();
            for (var i = begin; i < begin + count; i++)
                ret.Add(v.Rows[i]);
            return ret;
        }

        public static int Fill(this DataSet v, IDbConnection cnx, string selectQuery)
        {
            IDbConnection cnxAux = null;
            DbDataAdapter da = null;
            try
            {
                cnxAux = cnx.CloneConnection();
                cnxAux.Open();

                var pf = cnxAux.GetDbProviderFactory();

                da = pf.CreateDataAdapter();

                da.SelectCommand = (DbCommand) cnxAux.CreateCommand(selectQuery);
                da.FillSchema(v, SchemaType.Source);
                return da.Fill(v);
            }
            catch (Exception ex)
            {
                throw new DataException("Error in fill dataset.", ex);
            }
            finally
            {
                try
                {
                    da.Dispose();
                    cnxAux.Close();
                }
                catch
                {
                }
            }
        }

        public static int Fill(this DataSet v, IDbCommand cmd)
        {
            return v.Fill(cmd, null, false);
        }

        public static int Fill(this DataSet v, IDbCommand cmd, string toTable)
        {
            return v.Fill(cmd, toTable, false);
        }

        public static int Fill(this DataSet v, IDbCommand cmd, string toTable, bool includingSchema)
        {
            DbConnection oldAuxCnx = null;
            DbConnection auxCnx = null;
            DbDataAdapter da = null;

            try
            {
                oldAuxCnx = (DbConnection) cmd.Connection;

                auxCnx = (DbConnection) oldAuxCnx.CloneConnection();
                cmd.Connection = auxCnx;

                var pf = auxCnx.GetDbProviderFactory();

                auxCnx.Open();

                da = pf.CreateDataAdapter();

                da.SelectCommand = (DbCommand) cmd;

                if (includingSchema)
                    da.FillSchema(v, SchemaType.Source);

                if (string.IsNullOrEmpty(toTable))
                    return da.Fill(v);
                else
                    return da.Fill(v, toTable);
            }
            catch (Exception ex)
            {
                throw new DataException("Error in fill dataset.", ex);
            }
            finally
            {
                if (da != null)
                    da.Dispose();

                if (auxCnx != null && !auxCnx.State.Equals(ConnectionState.Closed))
                    auxCnx.Close();
                cmd.Connection = oldAuxCnx;
            }
        }

        public static IDictionary<string, int> Fill(this DataSet v, IDbConnection cnx, IEnumerable<string> tableNames,
            string sqlFilter)
        {
            IDbConnection cnxAux = null;
            IDictionary<string, int> ret = new Dictionary<string, int>();
            DbDataAdapter da = null;
            try
            {
                cnxAux = cnx.CloneConnection();
                cnxAux.Open();

                da = cnxAux.GetDbProviderFactory().CreateDataAdapter();
                foreach (var tableName in tableNames)
                {
                    da.SelectCommand = (DbCommand) cnxAux.CreateCommand("SELECT * FROM " + tableName +
                                                                        (string.IsNullOrEmpty(sqlFilter)
                                                                            ? ""
                                                                            : " WHERE " + sqlFilter));
                    da.FillSchema(v, SchemaType.Source, tableName);
                    ret.Add(tableName, da.Fill(v, tableName));
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new DataException("Error in fill dataset.", ex);
            }
            finally
            {
                try
                {
                    da.Dispose();
                    cnxAux.Close();
                }
                catch
                {
                }
            }
        }


        public static void ToCSV(this IDataReader value, TextWriter sw, bool includeHeaders = true)
        {
            value.ToCSV(sw, CultureInfo.CurrentCulture, includeHeaders, ',', Environment.NewLine);
        }

        public static void ToCSV(this IDataReader value, TextWriter sw, CultureInfo culture, bool includeHeaders = true,
            char fieldSeparator = ',', string recordSeparator = null)
        {
            Assert.NotNull(sw, nameof(sw));
            Assert.NotNull(culture, nameof(culture));

            recordSeparator = string.IsNullOrEmpty(recordSeparator) ? Environment.NewLine : recordSeparator;

            var auxCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = culture;

            IList<string> columns = value.GetSchemaTable()
                .Rows.Cast<DataRow>()
                .OrderBy(v => v["ColumnOrdinal"])
                .Select(v => v["ColumnName"].ToString())
                .ToList();

            if (includeHeaders)
            {
                sw.Write(columns.ToStringJoin(fieldSeparator + ""));
                sw.Write(recordSeparator);
            }

            var next = value.Read();
            while (next)
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    if (i > 0)
                        sw.Write(fieldSeparator);


                    var o = value.GetValue(i);
                    var t = o.GetType();

                    if (o.Equals(DBNull.Value))
                        sw.Write("");
                    else if (t.Equals(typeof(string)))
                        sw.Write(o.ToString().EscapeCSV(fieldSeparator));
                    else if (t.IsNumber())
                        sw.Write(o.ToString().EscapeCSV(fieldSeparator));
                    else if (t.Equals(typeof(DateTime)))
                        sw.Write(((DateTime) o).ToString("G").EscapeCSV(fieldSeparator));
                    else if (t.Equals(typeof(bool)))
                        sw.Write(((bool) o).ToString().EscapeCSV(fieldSeparator));
                    else if (t.Equals(typeof(byte[])))
                        sw.Write(((byte[]) o).ToHexadecimal().EscapeCSV(fieldSeparator));
                }

                next = value.Read();
                if (next)
                    sw.Write(recordSeparator);
            }

            Thread.CurrentThread.CurrentCulture = auxCulture;
            sw.Flush();
        }

        public static bool IsConstraintViolationError(this DbException v)
        {
            switch (v.GetType().Name)
            {
                case "SybaseException":
                    return v.ErrorCode == 0;
                case "MysqlException":
                    return v.ErrorCode == 1216;
                case "SqliteException":
                    return v.ErrorCode == 0;
                case "OracleException":
                    return v.ErrorCode == 0;
                case "SqlException":
                    return v.ErrorCode == 547; // Constraint check violation
                case "DB2Exception":
                    return v.ErrorCode == 0;
                case "PostgreSQLException":
                    return v.ErrorCode == 0;
                case "HSqlException":
                    return v.ErrorCode == 0;
                default:
                    return false;
            }
        }

        public static bool IsUniqueOrPKError(this DbException v)
        {
            switch (v.GetType().Name)
            {
                case "SybaseException":
                    return v.ErrorCode == 0;
                case "MysqlException":
                    return v.ErrorCode == 1062;
                case "SqliteException":
                    return v.ErrorCode == 0;
                case "OracleException":
                    return v.ErrorCode == 0;
                case "SqlException":
                    return v.ErrorCode == 2627 // Unique constraint error
                           || v.ErrorCode == 2601; // Duplicated key row error
                case "DB2Exception":
                    return v.ErrorCode == 0;
                case "PostgreSQLException":
                    return v.ErrorCode == 0;
                case "HSqlException":
                    return v.ErrorCode == 0;
                default:
                    return false;
            }
        }

        public static IDictionary<string, Type> GetProperties(this DbConnectionStringBuilder v)
        {
            IDictionary<string, Type> ret = new Dictionary<string, Type>();
            var csbt = v.GetType();
            // Tablecaching es por error del conector mysql
            foreach (var pi in csbt.GetProperties(BindingFlags.Instance
                                                  | BindingFlags.Public | BindingFlags.DeclaredOnly
                                                  | BindingFlags.GetProperty | BindingFlags.SetProperty))
                if (!new[] {"Count", "IsFixedSize", "Item", "Keys", "Values"}.Any(i => i.Equals(pi.Name)))
                    ret.Add(pi.Name, pi.PropertyType);
            return ret;
        }

        public static IDictionary<string, Type> GetKeyTypes(this DbConnectionStringBuilder v)
        {
            IDictionary<string, Type> ret = new Dictionary<string, Type>();
            var csbt = v.GetType();
            // Tablecaching es por error del conector mysql
            foreach (var pi in csbt.GetProperties(BindingFlags.Instance
                                                  | BindingFlags.Public | BindingFlags.DeclaredOnly
                                                  | BindingFlags.GetProperty | BindingFlags.SetProperty))
                if (!new[] {"Count", "IsFixedSize", "Item", "Keys", "Values"}.Any(i => i.Equals(pi.Name)))
                    ret.Add(SepareWithSpaces(pi.Name), pi.PropertyType);
            return ret;
        }

        private static string SepareWithSpaces(string v)
        {
            var ret = "";
            var va = v.ToCharArray();
            for (var i = 0; i < va.Length; i++)
            {
                if (char.IsUpper(va[i]) && i > 0 && !char.IsUpper(va[i - 1]))
                    ret += " ";
                ret += va[i];
            }

            return ret;
        }

        public static object GetProperty(this DbConnectionStringBuilder v, string name)
        {
            var csbt = v.GetType();
            return csbt.GetProperty(name).GetValue(v);
        }

        public static void SetProperty(this DbConnectionStringBuilder v, string name, object value)
        {
            var csbt = v.GetType();
            csbt.GetProperty(name).SetValue(v, value);
        }

        public static bool CmdInputConnectionStringBuilder(string label,
            DbConnectionStringBuilder connectionStringBuilder)
        {
            var ret = false;

            var cmd = new Regex(@"^([^\s]+)\s([^=]+)=(.*)$");
            string aux = null;
            Assert.NotNull(connectionStringBuilder, nameof(connectionStringBuilder));

            Action showHelp = () =>
            {
                Console.WriteLine("DbConnectionStringBuilder Command Help: ");
                Console.WriteLine(" - list                  List key-values.");
                Console.WriteLine(" - add <key>=<value>     Add key-value. ");
                Console.WriteLine(" - edit <key>=<value>    Edit key-value. ");
                Console.WriteLine(" - remove <key>          Remove key-value.");
                Console.WriteLine(" - quit                  Quit.");
                Console.WriteLine(" - help                  Show this help.");
            };

            Action<DbConnectionStringBuilder> showList = v =>
            {
                Console.WriteLine(
                    v.Values.Cast<KeyValuePair<string, object>>()
                        .Select(kv =>
                            kv.Key + " (" + connectionStringBuilder.GetKeyTypes()[kv.Key].Name + ") = " + kv.Value)
                        .ToStringJoin(Environment.NewLine)
                );
            };
            var b = true;
            do
            {
                Console.WriteLine(label);
                Console.WriteLine("Finish: [ESC]");

                if (!ConsoleHelper.ReadLineESC(out aux))
                    return false;

                var m = cmd.Match(aux);

                if (!m.Success)
                {
                    showList(connectionStringBuilder);
                    continue;
                }

                switch (m.Groups[1].Value.ToLower())
                {
                    case "list":
                        showList(connectionStringBuilder);
                        break;
                    case "quit":
                        b = false;
                        ret = true;
                        break;
                    case "help":
                        showHelp();
                        break;
                    case "add":
                        var v = m.Groups[3].Value.Trim();
                        if (v.Equals("true") || v.Equals("false"))
                            connectionStringBuilder.Add(m.Groups[2].Value.Trim(), bool.Parse(v));
                        else if (v.RegexIsMatch(@"[+-]?\d+"))
                            connectionStringBuilder.Add(m.Groups[2].Value.Trim(), int.Parse(v));
                        else if (v.RegexIsMatch(@"[+-]?\d+\.\d+"))
                            connectionStringBuilder.Add(m.Groups[2].Value.Trim(), decimal.Parse(v));
                        else
                            connectionStringBuilder.Add(m.Groups[2].Value.Trim(), v);
                        break;
                    case "edit":
                        connectionStringBuilder[m.Groups[2].Value.Trim()] = Convert.ChangeType(m.Groups[3].Value.Trim(),
                            connectionStringBuilder.GetProperties()[m.Groups[2].Value.Trim()]);
                        break;
                    case "remove":
                        connectionStringBuilder.Remove(m.Groups[2].Value.Trim());
                        break;
                }
            } while (b);

            return ret;
        }
    }
}