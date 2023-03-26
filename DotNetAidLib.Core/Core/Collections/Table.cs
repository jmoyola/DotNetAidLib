using System.Collections.Generic;
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class TableRow : Dictionary<string, object>
    {
        public TableRow()
        {
        }

        public TableRow(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public class TableColumn
    {
        public TableColumn(string id)
        {
            Assert.NotNullOrEmpty(id, nameof(id));

            Id = id;
        }

        public string Id { get; }

        public override string ToString()
        {
            return Id;
            ;
        }

        public override bool Equals(object obj)
        {
            return Id.Equals(obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return Id.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }
    }

    public class Table<R, C> where R : TableRow where C : TableColumn
    {
        public IList<C> Columns { get; } = new List<C>();

        public IList<R> Headers { get; } = new List<R>();

        public IList<R> Footers { get; } = new List<R>();

        public IList<R> Rows { get; } = new List<R>();


        public override string ToString()
        {
            var sb = new StringBuilder();

            // Headers
            if (Headers.Count > 0)
                foreach (TableRow headerRow in Headers)
                {
                    foreach (TableColumn column in Columns)
                    {
                        sb.Append("\t");
                        sb.Append(headerRow[column.Id]);
                    }

                    sb.AppendLine();
                }

            // Rows
            foreach (TableRow row in Rows)
            {
                foreach (TableColumn column in Columns)
                {
                    sb.Append("\t");
                    sb.Append(row[column.Id]);
                }

                sb.AppendLine();
            }

            // Footers
            if (Footers.Count > 0)
                foreach (TableRow footerRow in Footers)
                {
                    foreach (TableColumn column in Columns)
                    {
                        sb.Append("\t");
                        sb.Append(footerRow[column.Id]);
                    }

                    sb.AppendLine();
                }

            return sb.ToString();
        }
    }
}