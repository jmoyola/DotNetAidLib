using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Database.SQL.Core
{
    public enum ToStringFormat
    {
        SQLSelect,
        SQLWhere,
        ResultColumnName
    }

    public class ColumnDefinition
    {
        private static readonly Regex
            regFQDN = new Regex("^((([a-zA-Z0-9_]+)\\.){0,3})(.+)?$", RegexOptions.IgnoreCase);

        private string m_ColumnAlias;

        private string m_NombreFQDNEnTabla;

        public ColumnDefinition(string value) :
            this(value, (DbConnection) null)
        {
        }

        public ColumnDefinition(string value, DbConnection cnx)
        {
            string sAlias = null;
            try
            {
                var iAlias = value.ToUpper().IndexOf("AS", StringComparison.InvariantCultureIgnoreCase);
                if (iAlias > -1)
                {
                    sAlias = value.Substring(iAlias + 3).Trim();
                    if (sAlias.Length == 0)
                        throw new Exception("Invalid Alias");
                    ColumnAlias = sAlias.Split(' ')[0];

                    value = value.Substring(0, iAlias - 1).Trim();
                }

                var m = regFQDN.Match(value.Trim());
                if (m.Success)
                    FQDNColumnName = m.Value;
                else
                    throw new Exception("Ivalid column name/expression definition");

                if (ColumnName.Equals("*") && cnx != null)
                    SubResultColumns = ColumnDefinitionList.FromWildcardColumnNameDefinition(value, cnx);

                // Dim m As Match = regFQDNAlias.Match(value.Trim())
                // If (m.Success) Then
                //     Me.FQDNColumnName = m.Groups(1).Value
                //     If (Not String.IsNullOrEmpty(m.Groups(8).Value)) Then
                //         Me.ColumnAlias = m.Groups(8).Value
                //     Else
                //         Me.ColumnAlias = Nothing
                //     End If
                // Else
                //     Throw New Exception("Sintax error. The expression '" & value & "' is not valid for a column definition. Sintax: [[[<Catalog>.]<Schema>.]<Table>.]<ColumnName> [[as] <ColumnAlias>]")
                // End If
            }
            catch (Exception ex)
            {
                throw new Exception("Sintax error. The expression \'"
                                    + value + "\' is not valid for a column definition: "
                                    + ex.Message + "\r\n" +
                                    "Sintax: [[[<Catalog>.]<Schema>.]<Table>.]<ColumnNameOrExpression> [[as] <ColumnAlias>]");
            }
        }

        public ColumnDefinition(string sFQDNColumnName, string columnAlias)
        {
            FQDNColumnName = sFQDNColumnName;
            ColumnAlias = columnAlias;
        }

        public ColumnDefinitionList SubResultColumns { get; }

        public string FQDNColumnName
        {
            get => m_NombreFQDNEnTabla;
            set
            {
                var m = regFQDN.Match(value.Trim());
                if (m.Success)
                {
                    m_NombreFQDNEnTabla = value.Trim();
                    ColumnName = m.Groups[4].Value;
                    //  Por cada captura del grupo 3 tenemos los componentes del fqdn del nombre
                    if (m.Groups[3].Captures.Count > 0)
                        TableName = m.Groups[3].Captures[m.Groups[3].Captures.Count - 1].Value;

                    if (m.Groups[3].Captures.Count > 1)
                        SchemaName = m.Groups[3].Captures[m.Groups[3].Captures.Count - 2].Value;

                    if (m.Groups[3].Captures.Count > 2)
                        CatalogName = m.Groups[3].Captures[m.Groups[3].Captures.Count - 3].Value;
                }
                else
                {
                    throw new Exception("Expression '" + value +
                                        "' is not valid for column name definition. sintax:[[[<Catalog>.]<Schema>.]<Table>.]<ColumnName>");
                }
            }
        }

        public string CatalogName { get; private set; }

        public string SchemaName { get; private set; }

        public string ColumnName { get; private set; }

        public string TableName { get; private set; }

        public string FQDomainPath
        {
            get
            {
                var m = regFQDN.Match(m_NombreFQDNEnTabla);
                var ret = m.Groups[1].Value;
                if (!string.IsNullOrEmpty(ret)) ret = ret.Substring(0, ret.Length - 1);
                //  Quitamos el ultimo punto
                return ret;
            }
        }

        public string ColumnAlias
        {
            get => m_ColumnAlias;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var reg = new Regex("^([a-zA-Z0-9_]+)?$", RegexOptions.IgnoreCase);
                    var m = reg.Match(value.Trim());
                    if (!m.Success)
                        throw new Exception("Expression '" + value +
                                            "' is not valid for alias definition. sintax: [<alias>]");

                    m_ColumnAlias = value.Trim();
                }
                else
                {
                    m_ColumnAlias = value;
                }
            }
        }

        public override string ToString()
        {
            return ToString(ToStringFormat.SQLSelect);
        }

        public string ToString(ToStringFormat format)
        {
            string ret = null;
            if (format.Equals(ToStringFormat.ResultColumnName))
            {
                if (!string.IsNullOrEmpty(ColumnAlias))
                    ret = ColumnAlias;
                else
                    ret = ColumnName;
            }
            else if (format.Equals(ToStringFormat.SQLSelect))
            {
                ret = FQDNColumnName;
                ret = ret + (!string.IsNullOrEmpty(ColumnAlias) ? " as " + ColumnAlias : "");
            }
            else if (format.Equals(ToStringFormat.SQLWhere))
            {
                ret = FQDNColumnName;
            }
            else
            {
                ret = ToString();
            }

            return ret;
        }

        public static implicit operator string(ColumnDefinition v)
        {
            return v.ToString(ToStringFormat.SQLSelect);
        }

        public static implicit operator ColumnDefinition(string v)
        {
            return new ColumnDefinition(v);
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
                if (typeof(ColumnDefinition).IsAssignableFrom(obj.GetType()))
                    return ToString().Equals((ColumnDefinition) obj);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}