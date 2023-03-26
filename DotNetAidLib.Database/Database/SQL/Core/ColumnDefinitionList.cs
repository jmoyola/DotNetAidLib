using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Database.SQL.Core
{
    public class ColumnDefinitionList : List<ColumnDefinition>
    {
        public ColumnDefinitionList()
        {
        }

        public ColumnDefinitionList(string value)
        {
            value.Split(',').ToList().ForEach(v => Add(new ColumnDefinition(v)));
        }

        public ColumnDefinitionList(string sFQDNWildcardColumnsNameDefinition, DbConnection cnx)
        {
            try
            {
                var sDefs = sFQDNWildcardColumnsNameDefinition.Split(',').ToList();
                foreach (var sDef in sDefs)
                    if (sDef.IndexOf(".*", StringComparison.InvariantCultureIgnoreCase) > -1)
                        AddRange(FromWildcardColumnNameDefinition(sDef, cnx));
                    else
                        Add(new ColumnDefinition(sDef));
            }
            catch (Exception ex)
            {
                throw new Exception("Error importing from WildcardColumnsNameDefinition." + ("\r\n" + ex));
            }
        }

        public override string ToString()
        {
            return this.ToStringJoin(", ");
        }

        public string ToString(ToStringFormat format)
        {
            string ret = null;
            foreach (var def in this) ret = ret + ", " + def.ToString(format);

            if (ret.Length > 0) ret = ret.Substring(2);

            return ret;
        }

        public static implicit operator string(ColumnDefinitionList v)
        {
            return v.ToStringJoin(", ");
        }

        public static implicit operator ColumnDefinitionList(string v)
        {
            return new ColumnDefinitionList(v);
        }

        public static ColumnDefinitionList FromWildcardColumnNameDefinition(string sFQDNWildcardColumnNameDefinition,
            DbConnection cnx)
        {
            var closedConnection = false;
            var excludeColumnNames = new List<string>();
            var ret = new ColumnDefinitionList();
            try
            {
                if (cnx == null) throw new Exception("Database connection parameter \'cnx\' can\'t be null.");

                if (cnx.State != ConnectionState.Open)
                {
                    cnx.Open();
                    closedConnection = true;
                }

                var regFQDNWilcardColumnDefinition = new Regex(
                    "^(((([a-zA-Z0-9_]+)\\.){1,3})\\*)(\\s+(as\\s+)?([a-zA-Z0-9_]+))?(\\s+exclude\\s*\\((\\s*\\,?\\s*([a-zA-Z0-9_]+)" +
                    ")+\\))?$", RegexOptions.IgnoreCase);
                var m = regFQDNWilcardColumnDefinition.Match(sFQDNWildcardColumnNameDefinition);
                if (!m.Success)
                    throw new Exception("Column definition \'"
                                        + sFQDNWildcardColumnNameDefinition +
                                        "\' is not a Wilcard Full Quality domain column name definition of database column. Sintax: [[<Catalog>" +
                                        ".]<Schema>.]<Table>.* [ as [_]<Alias>[_] ] [exclude ( <columnName1> [..,<columnNameN>] )]");

                string columnAlias = null;
                if (!(m.Groups[7].Value == null)) columnAlias = m.Groups[7].Value;

                if (!(m.Groups[10].Value == null))
                    foreach (Capture mcap in m.Groups[10].Captures)
                        excludeColumnNames.Add(mcap.Value);

                // cols.Add(FQDNColumnName)
                // tables.Add(m.Groups(2).Value)
                var cmd = cnx.CreateCommand();
                cmd.CommandText = "SELECT "
                                  + m.Groups[1].Value + " FROM "
                                  + m.Groups[2].Value.Substring(0, m.Groups[2].Value.Length - 1) + " WHERE FALSE";
                var dr = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
                var dt = dr.GetSchemaTable();
                foreach (DataRow drc in dt.Rows)
                    if (!excludeColumnNames.Any(v =>
                            v.Equals(drc["ColumnName"].ToString(), StringComparison.InvariantCultureIgnoreCase)))
                    {
                        string fqdn = null;
                        if (!DBNull.Value.Equals(drc["BaseCatalogName"])
                            && !string.IsNullOrEmpty(drc["BaseCatalogName"].ToString()))
                            fqdn = fqdn
                                   + (drc["BaseCatalogName"] + ".");

                        if (!DBNull.Value.Equals(drc["BaseSchemaName"])
                            && !string.IsNullOrEmpty(drc["BaseSchemaName"].ToString()))
                            fqdn = fqdn
                                   + (drc["BaseSchemaName"] + ".");

                        if (!DBNull.Value.Equals(drc["BaseTableName"])
                            && !string.IsNullOrEmpty(drc["BaseTableName"].ToString()))
                            fqdn = fqdn
                                   + (drc["BaseTableName"] + ".");

                        fqdn = fqdn + drc["ColumnName"];
                        if (!(columnAlias == null))
                        {
                            if (columnAlias.EndsWith("_", StringComparison.InvariantCultureIgnoreCase))
                                fqdn = fqdn + " as "
                                            + (columnAlias + drc["ColumnName"]);
                            else if (columnAlias.StartsWith("_", StringComparison.InvariantCultureIgnoreCase))
                                fqdn = fqdn + " as "
                                            + (drc["ColumnName"] + columnAlias);
                            else
                                fqdn = fqdn + " as "
                                            + (drc["ColumnName"] + ("_" + columnAlias));
                        }

                        var dcs = new ColumnDefinition(fqdn);
                        ret.Add(dcs);
                    }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error importing from WildcardColumnNameDefinition." + ("\r\n" + ex));
            }
            finally
            {
                try
                {
                    if (closedConnection && cnx.State != ConnectionState.Closed) cnx.Close();
                }
                catch
                {
                }
            }
        }

        public ColumnDefinition Item(string toStringValue, ToStringFormat format)
        {
            return this.FirstOrDefault(v => v.ToString(format).Equals(toStringValue));
        }

        public bool Contains(string value, ToStringFormat format)
        {
            return this.Any(v => v.ToString(format).Equals(value));
        }

        public int IndexOf(string value, ToStringFormat format)
        {
            var ret = -1;
            var e = this.FirstOrDefault(v => v.ToString(format).Equals(value));
            if (!(e == null)) ret = IndexOf(e);

            return ret;
        }

        public int LastIndexOf(string value, ToStringFormat format)
        {
            var ret = -1;
            var e = this.LastOrDefault(v => v.ToString(format).Equals(value));
            if (!(e == null)) ret = IndexOf(e);

            return ret;
        }

        public List<string> ToList(ToStringFormat format)
        {
            var ret = new List<string>();
            foreach (var dcs in this) ret.Add(dcs.ToString(format));

            return ret;
        }

        public string[] ToArray(ToStringFormat format)
        {
            return ToList(format).ToArray();
        }
    }
}