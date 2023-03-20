using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Helpers;
using DotNetAidLib.Core.Logger.Client;
using DataRelation = System.Data.DataRelation;

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
        private DataSet ds = new DataSet();

        public DataSet InnerDataset => this.ds; 
        public void Load(DBProviderConnector dbConnector, String databaseName, String tableName, IList<KeyValue<String, Object>> idMatch, int maxdeep=0)
        {
            IDbConnection sourceCnx = null;
            try
            {
                Assert.NotNull( dbConnector, nameof(dbConnector));
                Assert.NotNullOrEmpty( databaseName, nameof(databaseName));
                Assert.NotNullOrEmpty( tableName, nameof(tableName));

                sourceCnx = dbConnector.CreateConnection();

                sourceCnx.Open();
                
                ds = new DataSet();
                ds.DataSetName = databaseName;
                // Loading schema

                LoggerEx.Instance().WriteTrace("Filling schema from connection.");
                ds.FillSchemaAllTables(sourceCnx, true);

                DataTable dataTable = ds.Tables.Cast<DataTable>().FirstOrDefault(v =>
                    v.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
                if (dataTable == null)
                    throw new Exception("Table '" + tableName + "' don't exists in connection.");

                ds.EnforceConstraints = false;
                LoggerEx.Instance().WriteTrace("Loading records...");
                // ImportParentsRelations(sourceCnx, ds, tableName, idMatch);
                ImportChildRelations(sourceCnx, ds, tableName, idMatch, 0, maxdeep);
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error loading records.", ex);
            }
            finally
            {
                if(sourceCnx.State!= ConnectionState.Closed)
                    sourceCnx.Close();
            }
        }

        private String GetInsertCommand(DataRow dr, String databaseName = null)
        {
            String dbName = String.IsNullOrEmpty(databaseName) ? this.ds.DataSetName : databaseName;
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO " + dbName + "." + dr.Table.TableName);
            sb.AppendLine(" (" + dr.Table.Columns.Cast<DataColumn>()
                                  .OrderBy(v => v.Ordinal)
                                  .Select(v => v.ColumnName)
                                  .ToStringJoin(", ")
                              + ")");
            sb.AppendLine(" VALUES");
            sb.AppendLine(" (" + dr.ItemArray.Select(v=>v.ToSQLValue()).ToStringJoin(", ")+ ")");
            sb.Append(";");

            return sb.ToString();
        }

        private String GetUpdateCommand(DataRow dr, String databaseName = null)
        {
            String dbName = String.IsNullOrEmpty(databaseName) ? this.ds.DataSetName : databaseName;
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UPDATE " + dbName + "." + dr.Table.TableName);
            sb.AppendLine(" SET " + Enumerable.Range(0, dr.Table.Columns.Count)
                .Select(i=>dr.Table.Columns[i].ColumnName + "=" + dr[i].ToSQLValue())
                .ToStringJoin(", "));
            sb.AppendLine(" WHERE " + dr.Table.PrimaryKey.Select(v=>v.ColumnName + "=" + dr[v.Ordinal].ToSQLValue()));
            sb.Append(";");

            return sb.ToString();
        }

        public ExceptionList Save(IDbConnection sourceCnx, bool orUpdate=false, bool ignoreErrors=false, String databaseName=null)
        {
            String dbName = String.IsNullOrEmpty(databaseName) ? this.ds.DataSetName : databaseName;
            ExceptionList ret = new ExceptionList();
            
            foreach (DataTable dt in this.ds.HierarchicalDataTableOrder())
            {
                IDbCommand cmd=sourceCnx.CreateCommand();
                foreach (DataRow dr in dt.Rows)
                {
                    String cmdStr = null;
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
                                throw new DataPartialDumpException("Error inserting duplicate key record (" + cmdStr + ")",
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

        public void Save(StreamWriter sw, String databaseName=null)
        {
            Assert.NotNull( sw, nameof(sw));
            
            String dbName = String.IsNullOrEmpty(databaseName) ? this.ds.DataSetName : databaseName;
            
            foreach (DataTable dt in this.ds.HierarchicalDataTableOrder())
            {
                foreach (DataRow dr in dt.Rows)
                {
                    String cmdStr=null;
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
            }
            sw.Flush();
        }

        public void Export(FileInfo outputFile)
        {
            try
            {
                LoggerEx.Instance().WriteTrace("Exporting records...");
                ds.WriteXml(outputFile.FullName, XmlWriteMode.WriteSchema);

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
                Assert.Exists( inputFile, nameof(inputFile));
                
                LoggerEx.Instance().WriteTrace("Importing records...");
                this.ds = new DataSet();
                ds.ReadXml(inputFile.FullName, XmlReadMode.ReadSchema);
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error importing records.", ex);
            }
        }

        private void ImportChildRelations(IDbConnection sourceCnx, DataSet ds, String tableName, IList<KeyValue<String, Object>> idMatch, int deep, int maxdeep)
        {
            try
            {
                Assert.NotNull( sourceCnx, nameof(sourceCnx));
                Assert.NotNull( ds, nameof(ds));
                Assert.NotNullOrEmpty( tableName, nameof(tableName));
                
                deep++;
                
                if (maxdeep > 0 && deep > maxdeep)
                    return;

                if (!ds.Tables.Contains(tableName))
                    throw new DataPartialDumpException("Table '" + tableName + "' dont exists in dataset.");
                
                DataTable dtTable = ds.Tables[tableName];

                LoggerEx.Instance().WriteTrace("Import data for table " + tableName);
                String query = "select * from " + ds.DataSetName + "." + tableName + " where " +
                               idMatch.Select(kv => kv.Key + "=" + kv.Value.ToSQLValue()).ToStringJoin(" AND ") + ";";

                int rowCount = dtTable.Rows.Count;
                int rowCountAdded=ds.Fill(sourceCnx.CreateCommand(query) , tableName);
                IEnumerable<DataRow> rows = dtTable.Extract(rowCount, rowCountAdded);

                LoggerEx.Instance().WriteTrace("Retrieving data for table " + tableName + " (" + query + "): " + rowCountAdded + " records.");
                
                LoggerEx.Instance().WriteTrace("Setting child relations");
                // Por cada relación hija
                foreach (DataRelation childRelation in dtTable.ChildRelations)
                {
                    LoggerEx.Instance().WriteTrace("Child relation: " + childRelation.RelationName + ": " + childRelation.ParentTable.TableName + "." + childRelation.ParentColumns.Select(v=>v.ColumnName).ToStringJoin(", ") + "=>" + childRelation.ChildTable.TableName + "." + childRelation.ChildColumns.Select(v=>v.ColumnName).ToStringJoin(", "));
                    
                    // Creating fkeys...
                    IList<KeyValue<String, Object>> childMatch = childRelation.ChildColumns.OrderBy(v => v.Ordinal)
                        .Select(v => new KeyValue<String, Object>(v.ColumnName, v.DefaultValue))
                        .ToList();
                    
                    // Por cada fila padre
                    foreach (DataRow row in rows)
                    {
                        // Poblating fkeys values...
                        childMatch.ToList().ForEach((kv,i)=>kv.Value=row[childRelation.ParentColumns[i].ColumnName]);
                        // Importing child...
                        ImportChildRelations(sourceCnx, ds, childRelation.ChildTable.TableName, childMatch, deep, maxdeep);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataPartialDumpException("Error exporting records for table " + tableName, ex);
            }
        }

        private void ImportParentsRelations(IDbConnection sourceCnx, DataSet ds, String tableName, IList<KeyValue<String, Object>> idMatch)
        {
            try
            {
                Assert.NotNull( sourceCnx, nameof(sourceCnx));
                Assert.NotNull( ds, nameof(ds));
                Assert.NotNullOrEmpty( tableName, nameof(tableName));
                
                if (!ds.Tables.Contains(tableName))
                    throw new DataPartialDumpException("Table '" + tableName + "' dont exists in dataset.");
                
                DataTable dtTable = ds.Tables[tableName];
                
                
                LoggerEx.Instance().WriteTrace("Import data for table " + tableName);
                String query = "select * from " + ds.DataSetName + "." + tableName + " where " +
                               idMatch.Select(kv => kv.Key + "=" + kv.Value.ToSQLValue()).ToStringJoin(" AND ") + ";";

                int rowCount = dtTable.Rows.Count;
                int rowCountAdded=ds.Fill(sourceCnx.CreateCommand(query) , tableName);
                IEnumerable<DataRow> rows = dtTable.Extract(rowCount, rowCountAdded);
                LoggerEx.Instance().WriteTrace("Retrieving data for table " + tableName + " (" + query + "): " + rowCountAdded + " records.");

                LoggerEx.Instance().WriteTrace("Setting parent relations");
                // Por cada relación padre
                foreach (DataRelation parentRelation in dtTable.ParentRelations)
                {
                    LoggerEx.Instance().WriteTrace("Parent relation: " +dtTable.TableName + "." + parentRelation.RelationName + ": " + parentRelation.ParentTable.TableName + "." + parentRelation.ParentColumns.Select(v=>v.ColumnName).ToStringJoin(", ") + "=>" + parentRelation.ChildTable.TableName + "." + parentRelation.ParentColumns.Select(v=>v.ColumnName).ToStringJoin(", "));
                    
                    // Creating fkeys...
                    IList<KeyValue<String, Object>> parentMatch = parentRelation.ParentColumns.OrderBy(v => v.Ordinal)
                        .Select(v => new KeyValue<String, Object>(v.ColumnName, v.DefaultValue))
                        .ToList();
                    
                    // Por cada fila padre
                    foreach (DataRow row in rows)
                    {
                        // Poblating fkeys values...
                        parentMatch.ToList().ForEach((kv,i)=>kv.Value=row[parentRelation.ParentColumns[i].ColumnName]);
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