using System;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace DotNetAidLib.Database.SQL.Core
{
    public enum ToStringFormat
    {
        SQLSelect,
        SQLWhere,
        ResultColumnName,
    }

    public class ColumnDefinition
    {

        private static System.Text.RegularExpressions.Regex regFQDN = new System.Text.RegularExpressions.Regex ("^((([a-zA-Z0-9_]+)\\.){0,3})(.+)?$", RegexOptions.IgnoreCase);

        private string m_NombreFQDNEnTabla = null;

        private string m_ColumnAlias = null;

        private string m_CatalogName;

        private string m_SchemaName;

        private string m_TableName;

        private string m_ColumnName;

        private ColumnDefinitionList m_SubResultColumns = null;

        public ColumnDefinition(string value) :
                this(value, ((DbConnection)(null)))
        {
        }

        public ColumnDefinition(string value, DbConnection cnx)
        {
            string sAlias = null;
            try
            {
                int iAlias = value.ToUpper().IndexOf("AS", StringComparison.InvariantCultureIgnoreCase);
                if ((iAlias > -1))
                {
                    sAlias = value.Substring((iAlias + 3)).Trim();
                    if ((sAlias.Length == 0))
                    {
                        throw new Exception("Invalid Alias");
                    }
                    else
                    {
                        this.ColumnAlias = sAlias.Split(' ')[0];
                    }

                    value = value.Substring(0, (iAlias - 1)).Trim();
                }

                Match m = regFQDN.Match(value.Trim());
                if (m.Success)
                {
                    this.FQDNColumnName = m.Value;
                }
                else
                {
                    throw new Exception("Ivalid column name/expression definition");
                }

                if (this.ColumnName.Equals("*") && cnx != null)
                {
                    m_SubResultColumns = ColumnDefinitionList.FromWildcardColumnNameDefinition(value, cnx);
                }

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
                throw new Exception(("Sintax error. The expression \'"
                                + (value + ("\' is not valid for a column definition: "
                                + (ex.Message + ("\r\n" + "Sintax: [[[<Catalog>.]<Schema>.]<Table>.]<ColumnNameOrExpression> [[as] <ColumnAlias>]"))))));
            }

        }

        public ColumnDefinition(string sFQDNColumnName, string columnAlias)
        {
            this.FQDNColumnName = sFQDNColumnName;
            this.ColumnAlias = columnAlias;
        }

        public ColumnDefinitionList SubResultColumns
        {
            get
            {
                return m_SubResultColumns;
            }
        }

        public string FQDNColumnName
        {
            get
            {
                return m_NombreFQDNEnTabla;
            }
            set
            {
                Match m = regFQDN.Match(value.Trim());
                if (m.Success)
                {
                    m_NombreFQDNEnTabla = value.Trim();
                    m_ColumnName = m.Groups[4].Value;
                    //  Por cada captura del grupo 3 tenemos los componentes del fqdn del nombre
                    if (m.Groups[3].Captures.Count > 0)
                    {
                        m_TableName = m.Groups[3].Captures[m.Groups[3].Captures.Count - 1].Value;
                    }

                    if (m.Groups[3].Captures.Count > 1)
                    {
                        m_SchemaName = m.Groups[3].Captures[m.Groups[3].Captures.Count - 2].Value;
                    }

                    if (m.Groups[3].Captures.Count > 2)
                    {
                        m_CatalogName = m.Groups[3].Captures[m.Groups[3].Captures.Count - 3].Value;
                    }

                }
                else
                {
                    throw new Exception("Expression '" + value + "' is not valid for column name definition. sintax:[[[<Catalog>.]<Schema>.]<Table>.]<ColumnName>");
                }

            }
        }

        public string CatalogName
        {
            get
            {
                return m_CatalogName;
            }
        }

        public string SchemaName
        {
            get
            {
                return m_SchemaName;
            }
        }

        public string ColumnName
        {
            get
            {
                return m_ColumnName;
            }
        }

        public string TableName
        {
            get
            {
                return m_TableName;
            }
        }

        public string FQDomainPath
        {
            get
            {
                Match m = regFQDN.Match(m_NombreFQDNEnTabla);
                string ret = m.Groups[1].Value;
                if (!string.IsNullOrEmpty(ret))
                {
                    ret = ret.Substring(0, (ret.Length - 1));
                    //  Quitamos el ultimo punto
                }

                return ret;
            }
        }

        public string ColumnAlias
        {
            get
            {
                return m_ColumnAlias;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex ("^([a-zA-Z0-9_]+)?$", RegexOptions.IgnoreCase);
                    Match m = reg.Match(value.Trim());
                    if (!m.Success)
                    {
                        throw new Exception("Expression '" + value + "' is not valid for alias definition. sintax: [<alias>]");
                    }

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
            return this.ToString(ToStringFormat.SQLSelect);
        }

        public string ToString(ToStringFormat format)
        {
            string ret = null;
            if (format.Equals(ToStringFormat.ResultColumnName))
            {
                if (!string.IsNullOrEmpty(this.ColumnAlias))
                {
                    ret = this.ColumnAlias;
                }
                else
                {
                    ret = this.ColumnName;
                }

            }
            else if (format.Equals(ToStringFormat.SQLSelect))
            {
                ret = this.FQDNColumnName;
                ret = (ret + (!string.IsNullOrEmpty(this.ColumnAlias) ? (" as " + this.ColumnAlias) : ""));
            }
            else if (format.Equals(ToStringFormat.SQLWhere))
            {
                ret = this.FQDNColumnName;
            }
            else
            {
                ret = this.ToString();
            }

            return ret;
        }

        public static implicit operator String(ColumnDefinition v)
        {
            return v.ToString(ToStringFormat.SQLSelect);
        }

        public static implicit operator ColumnDefinition(String v){
            return new ColumnDefinition(v);
        }

        public override bool Equals(object obj)
        {
            if (obj != null) {
                if (typeof(ColumnDefinition).IsAssignableFrom(obj.GetType()))
                {
                    return this.ToString().Equals((ColumnDefinition)obj);
                }
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}