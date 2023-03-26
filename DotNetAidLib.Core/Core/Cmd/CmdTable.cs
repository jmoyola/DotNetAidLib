using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Drawing;

namespace DotNetAidLib.Core.Cmd
{
    public class CmdTableCell
    {
        public CmdTableCell(object value)
            : this(value, null, HorizontalAlignment.Left)
        {
        }

        public CmdTableCell(object value, string format)
            : this(value, format, HorizontalAlignment.Left)
        {
        }

        public CmdTableCell(object value, string format, HorizontalAlignment aligment)
        {
            this.Value = value;
            this.Format = format;
            this.Aligment = aligment;
        }

        public object Value { get; set; }

        public HorizontalAlignment Aligment { get; set; } = HorizontalAlignment.Left;

        public string Format { get; set; }

        public override string ToString()
        {
            if (Value.Equals(null))
                return null;
            return Value.ToString().FormatString(Format);
        }

        public string ToString(int width)
        {
            return ToString().Align(Aligment, width);
        }

        public override bool Equals(object obj)
        {
            return Equals(Value, obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            if (Value.Equals(null))
                return 0;
            return Value.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }
    }

    public class CmdTableRow : TableRow
    {
        public CmdTableRow()
        {
        }

        public CmdTableRow(string id)
            : base(id)
        {
            Title = id;
        }

        public string Title { get; set; }
    }

    public class CmdTableColumn : TableColumn
    {
        public CmdTableColumn(string id)
            : base(id)
        {
        }

        public HorizontalAlignment Alignment { get; set; }

        public int Width { get; set; } = 5;

        public string Format { get; set; }
    }

    public class CmdTable : Table<CmdTableRow, CmdTableColumn>
    {
        public int RowTitleWidth { get; set; } = 5;

        public string Title { get; set; }

        public char ColumnSeparator { get; set; } = '│';

        public char RowSeparator { get; set; } = '─';

        public char CrossSeparator { get; set; } = '┼';

        public override string ToString()
        {
            var sb = new StringBuilder();

            // Caption
            if (!string.IsNullOrEmpty(Title))
                sb.AppendLine(Title.Align(HorizontalAlignment.Center,
                    RowTitleWidth + Columns.Sum(v => v.Width) + Columns.Count));

            // Headers
            if (Headers.Count > 0)
            {
                AddRows(sb, Headers);
                sb.AppendLine(new string(RowSeparator, RowTitleWidth) +
                              Columns.Select(v => CrossSeparator + new string(RowSeparator, v.Width)).ToStringJoin());
            }

            // Rows
            AddRows(sb, Rows);

            // Footers
            if (Footers.Count > 0)
            {
                sb.AppendLine(new string(RowSeparator, RowTitleWidth) +
                              Columns.Select(v => CrossSeparator + new string(RowSeparator, v.Width)).ToStringJoin());
                AddRows(sb, Footers);
            }

            return sb.ToString();
        }

        private void AddRows(StringBuilder sb, IList<CmdTableRow> rows)
        {
            foreach (var row in rows)
            {
                sb.Append(row.Title.PadRight(RowTitleWidth).Substring(0, RowTitleWidth));

                foreach (var column in Columns)
                {
                    sb.Append(ColumnSeparator);
                    if (row[column.Id] == null)
                    {
                        sb.Append(new string(' ', column.Width));
                    }
                    else
                    {
                        if (typeof(CmdTableCell).IsAssignableFrom(row[column.Id].GetType()))
                            ((CmdTableCell) row[column.Id]).ToString(column.Width);
                        else
                            sb.Append(row[column.Id].ToString().FormatString(column.Format)
                                .Align(column.Alignment, column.Width));
                    }
                }

                sb.AppendLine();
            }
        }
    }
}