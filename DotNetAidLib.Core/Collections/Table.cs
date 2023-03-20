using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Drawing;

namespace DotNetAidLib.Core.Collections
{
    public class TableRow : Dictionary<String, Object>
    {
        private String id;

        public TableRow() { }
        public TableRow(String id)
        {
            this.id = id;
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }
    }

    public class TableColumn
    {
        private String id;

        public TableColumn(String id)
        {
            Assert.NotNullOrEmpty( id, nameof(id));

            this.id = id;
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        public override string ToString()
        {
            return this.id; ;
        }

        public override bool Equals(object obj)
        {
            return this.id.Equals(obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return this.id.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }
    }
    public class Table<R,C> where R:TableRow where C:TableColumn
    {
        private IList<C> columns = new List<C>();
        private IList<R> headers = new List<R>();
        private IList<R> footers = new List<R>();
        private IList<R> rows = new List<R>();
        public Table()
        {
        }

        public IList<C> Columns
        {
            get
            {
                return columns;
            }
        }

        public IList<R> Headers
        {
            get
            {
                return headers;
            }
        }

        public IList<R> Footers
        {
            get
            {
                return footers;
            }
        }

        public IList<R> Rows
        {
            get
            {
                return rows;
            }
        }



        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // Headers
            if (this.headers.Count > 0)
            {
                foreach (TableRow headerRow in this.headers)
                {
                    foreach (TableColumn column in this.columns)
                    {
                        sb.Append("\t");
                        sb.Append(headerRow[column.Id]);
                    }

                    sb.AppendLine();
                }
            }

            // Rows
            foreach (TableRow row in this.rows)
            {
                foreach (TableColumn column in this.columns)
                {
                    sb.Append("\t");
                    sb.Append(row[column.Id]);
                }

                sb.AppendLine();
            }

            // Footers
            if (this.footers.Count > 0)
            {
                foreach (TableRow footerRow in this.footers)
                {
                    foreach (TableColumn column in this.columns)
                    {
                        sb.Append("\t");
                        sb.Append(footerRow[column.Id]);
                    }

                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}
