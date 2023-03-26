using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Database.DbProviders;

namespace DotNetAidLib.Database.Export
{
    public class DataPartialDumpException : Exception
    {
        public DataPartialDumpException(string message) : base(message)
        {
        }

        public DataPartialDumpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class DataPartialDump
    {
        public DataSet InnerDataset { get; private set; } = new DataSet();

        public void Load(DBProviderConnector dbConnector, string databaseName, string tableName,
            IList<KeyValue<string, object>> idMatch, int maxdeep = 0)
        {
            IDbConnection sourceCnx = null;
            try
            {
                Assert.NotNull(dbConnector, nameof(dbConnector));
                Assert.NotNullOrEmpty(databaseName, nameof(databaseName));
                Assert.NotNullOrEmpty(tableName, nameof(tableName));

                sourceCnx = dbConnector.CreateConnection();

                sourceCnx.Open();

                InnerDataset = new DataSet();
                InnerDataset.DataSetName = databaseName;
                // Loading schema

                InnerDataset.FillSchemaAllTables(sourceCnx, true);

                var dataTable = InnerDataset.Tables.Cast<DataTable>().FirstOrDefault(v =>
                    v.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
                if (dataTable == null)
                    throw new Exception("Table '" + tableName + "' don't exists in connection.");

                InnerDataset.EnforceConstraints = false;
                // ImportParentsRelations(sourceCnx, ds, tableName, idMatch);
                ImportChildRelations(sourceCnx, InnerDataset, tableName, idMatch, 0, maxdeep);
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error loading records.", ex);
            }
            finally
            {
                if (sourceCnx.State != ConnectionState.Closed)
                    sourceCnx.Close();
            }
        }

        private string GetInsertCommand(DataRow dr, string databaseName = null)
        {
            var dbName = string.IsNullOrEmpty(databaseName) ? InnerDataset.DataSetName : databaseName;

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO " + dbName + "." + dr.Table.TableName);
            sb.AppendLine(" (" + dr.Table.Columns.Cast<DataColumn>()
                                   .OrderBy(v => v.Ordinal)
                                   .Select(v => v.ColumnName)
                                   .ToStringJoin(", ")
                               + ")");
            sb.AppendLine(" VALUES");
            sb.AppendLine(" (" + dr.ItemArray.Select(v => v.ToSQLValue()).ToStringJoin(", ") + ")");
            sb.Append(";");

            return sb.ToString();
        }

        private string GetUpdateCommand(DataRow dr, string databaseName = null)
        {
            var dbName = string.IsNullOrEmpty(databaseName) ? InnerDataset.DataSetName : databaseName;

            var sb = new StringBuilder();
            sb.AppendLine("UPDATE " + dbName + "." + dr.Table.TableName);
            sb.AppendLine(" SET " + Enumerable.Range(0, dr.Table.Columns.Count)
                .Select(i => dr.Table.Columns[i].ColumnName + "=" + dr[i].ToSQLValue())
                .ToStringJoin(", "));
            sb.AppendLine(" WHERE " + dr.Table.PrimaryKey.Select(v => v.ColumnName + "=" + dr[v.Ordinal].ToSQLValue()));
            sb.Append(";");

            return sb.ToString();
        }

        public ExceptionList Save(IDbConnection sourceCnx, bool orUpdate = false, bool ignoreErrors = false,
            string databaseName = null)
        {
            var dbName = string.IsNullOrEmpty(databaseName) ? InnerDataset.DataSetName : databaseName;
            var ret = new ExceptionList();

            foreach (var dt in InnerDataset.HierarchicalDataTableOrder())
            {
                var cmd = sourceCnx.CreateCommand();
                foreach (DataRow dr in dt.Rows)
                {
                    string cmdStr = null;
                    int n;
                    try
                    {
                        try
                        {
                            cmdStr = GetInsertCommand(dr, databaseName);
                            n = cmd.ExecuteNonQuery(cmdStr);
                        }
                        // Duplicate key (unique constraint) or fk constraint exception
                        catch (ConstraintException exd)
                        {
                            if (!orUpdate)
                                throw new DataPartialDumpException(
                                    "Error inserting duplicate key record (" + cmdStr + ")",
                                    exd);

                            try
                            {
                                cmdStr = GetUpdateCommand(dr, databaseName);
                                n = cmd.ExecuteNonQuery(cmdStr);
                            }
                            catch (Exception ex)
                            {
                                throw new DataPartialDumpException("Error updating record.", ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new DataPartialDumpException("Error inserting record.", ex);
                        }
                    }
                    catch (Exception e)
                    {
                        Exception ee = new DataPartialDumpException("Error saving records (" + cmdStr + ")", e);
                        if (ignoreErrors)
                            ret.Add(ee);
                        else
                            throw ee;
                    }
                }
            }

            return ret;
        }

        public void Save(StreamWriter sw, string databaseName = null)
        {
            Assert.NotNull(sw, nameof(sw));

            var dbName = string.IsNullOrEmpty(databaseName) ? InnerDataset.DataSetName : databaseName;

            foreach (var dt in InnerDataset.HierarchicalDataTableOrder())
            foreach (DataRow dr in dt.Rows)
            {
                string cmdStr = null;
                try
                {
                    cmdStr = GetInsertCommand(dr, databaseName);
                    sw.WriteLine(cmdStr);
                }
                catch (Exception e)
                {
                    throw new DataPartialDumpException("Error saving records (" + cmdStr + ")", e);
                }
            }

            sw.Flush();
        }

        public void Export(FileInfo outputFile)
        {
            try
            {
                InnerDataset.WriteXml(outputFile.FullName, XmlWriteMode.WriteSchema);
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error exporting records.", ex);
            }
        }

        public void Import(FileInfo inputFile)
        {
            try
            {
                Assert.Exists(inputFile, nameof(inputFile));

                InnerDataset = new DataSet();
                InnerDataset.ReadXml(inputFile.FullName, XmlReadMode.ReadSchema);
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error importing records.", ex);
            }
        }

        private void ImportChildRelations(IDbConnection sourceCnx, DataSet ds, string tableName,
            IList<KeyValue<string, object>> idMatch, int deep, int maxdeep)
        {
            try
            {
                Assert.NotNull(sourceCnx, nameof(sourceCnx));
                Assert.NotNull(ds, nameof(ds));
                Assert.NotNullOrEmpty(tableName, nameof(tableName));

                deep++;

                if (maxdeep > 0 && deep > maxdeep)
                    return;

                if (!ds.Tables.Contains(tableName))
                    throw new DataPartialDumpException("Table '" + tableName + "' dont exists in dataset.");

                var dtTable = ds.Tables[tableName];

                var query = "select * from " + ds.DataSetName + "." + tableName + " where " +
                            idMatch.Select(kv => kv.Key + "=" + kv.Value.ToSQLValue()).ToStringJoin(" AND ") + ";";

                var rowCount = dtTable.Rows.Count;
                var rowCountAdded = ds.Fill(sourceCnx.CreateCommand(query), tableName);
                var rows = dtTable.Extract(rowCount, rowCountAdded);

                // Por cada relación hija
                foreach (DataRelation childRelation in dtTable.ChildRelations)
                {
                    // Creating fkeys...
                    IList<KeyValue<string, object>> childMatch = childRelation.ChildColumns.OrderBy(v => v.Ordinal)
                        .Select(v => new KeyValue<string, object>(v.ColumnName, v.DefaultValue))
                        .ToList();

                    // Por cada fila padre
                    foreach (var row in rows)
                    {
                        // Poblating fkeys values...
                        childMatch.ToList()
                            .ForEach((kv, i) => kv.Value = row[childRelation.ParentColumns[i].ColumnName]);
                        // Importing child...
                        ImportChildRelations(sourceCnx, ds, childRelation.ChildTable.TableName, childMatch, deep,
                            maxdeep);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error exporting records for table " + tableName, ex);
            }
        }

        private void ImportParentsRelations(IDbConnection sourceCnx, DataSet ds, string tableName,
            IList<KeyValue<string, object>> idMatch)
        {
            try
            {
                Assert.NotNull(sourceCnx, nameof(sourceCnx));
                Assert.NotNull(ds, nameof(ds));
                Assert.NotNullOrEmpty(tableName, nameof(tableName));

                if (!ds.Tables.Contains(tableName))
                    throw new DataPartialDumpException("Table '" + tableName + "' dont exists in dataset.");

                var dtTable = ds.Tables[tableName];


                var query = "select * from " + ds.DataSetName + "." + tableName + " where " +
                            idMatch.Select(kv => kv.Key + "=" + kv.Value.ToSQLValue()).ToStringJoin(" AND ") + ";";

                var rowCount = dtTable.Rows.Count;
                var rowCountAdded = ds.Fill(sourceCnx.CreateCommand(query), tableName);
                var rows = dtTable.Extract(rowCount, rowCountAdded);

                // Por cada relación padre
                foreach (DataRelation parentRelation in dtTable.ParentRelations)
                {
                    // Creating fkeys...
                    IList<KeyValue<string, object>> parentMatch = parentRelation.ParentColumns.OrderBy(v => v.Ordinal)
                        .Select(v => new KeyValue<string, object>(v.ColumnName, v.DefaultValue))
                        .ToList();

                    // Por cada fila padre
                    foreach (var row in rows)
                    {
                        // Poblating fkeys values...
                        parentMatch.ToList().ForEach((kv, i) =>
                            kv.Value = row[parentRelation.ParentColumns[i].ColumnName]);
                        // Importing parent...
                        ImportParentsRelations(sourceCnx, ds, parentRelation.ParentTable.TableName, parentMatch);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error loading parent relations for table " + tableName, ex);
            }
        }
    }
}