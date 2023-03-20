using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Drawing;

namespace DotNetAidLib.Core.Cmd
{
    public class CmdTableCell {
        private Object value;
        private HorizontalAlignment aligment = HorizontalAlignment.Left;
        private String format;

        public CmdTableCell(Object value)
            :this(value,null, HorizontalAlignment.Left) {}

        public CmdTableCell(Object value, String format)
            : this(value, format, HorizontalAlignment.Left) {}

        public CmdTableCell(Object value, String format, HorizontalAlignment aligment)
        {
            this.value = value;
            this.format = format;
            this.aligment = aligment;
        }

        public Object Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        public HorizontalAlignment Aligment
        {
            get
            {
                return aligment;
            }

            set
            {
                aligment = value;
            }
        }

        public string Format
        {
            get
            {
                return format;
            }

            set
            {
                format = value;
            }
        }

        public override string ToString()
        {
            if (this.value.Equals(null))
                return null;
            else
                return this.value.ToString().FormatString(this.format);
        }

        public string ToString(int width)
        {
            return this.ToString().Align(this.aligment, width);
        }

        public override bool Equals(object obj)
        {
            return Object.Equals(this.value, obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            if (this.value.Equals(null))
                return 0;
            else
                return this.value.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }
    }

    public class CmdTableRow : TableRow
    {
        private String title;

        public CmdTableRow ()
        :base() { }

        public CmdTableRow (String id)
            :base(id)
        {
            this.title = id;
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }
    }

    public class CmdTableColumn: TableColumn
    {
        private int width = 5;
        private HorizontalAlignment alignment;
        private String format;

        public CmdTableColumn (String id)
            :base(id)
        {
        }

        public HorizontalAlignment Alignment
        {
            get
            {
                return alignment;
            }

            set
            {
                alignment = value;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
            }
        }

        public string Format
        {
            get
            {
                return format;
            }

            set
            {
                format = value;
            }
        }
    }
    public class CmdTable:Table<CmdTableRow, CmdTableColumn>
    {
        private String title;
        private int rowTitleWidth = 5;
        private char columnSeparator = '│';
        private char rowSeparator = '─';
        private char crossSeparator = '┼';
        public CmdTable ()
        {
        }

        public int RowTitleWidth
        {
            get
            {
                return rowTitleWidth;
            }

            set
            {
                rowTitleWidth = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        public char ColumnSeparator
        {
            get
            {
                return columnSeparator;
            }

            set
            {
                columnSeparator = value;
            }
        }

        public char RowSeparator
        {
            get
            {
                return rowSeparator;
            }

            set
            {
                rowSeparator = value;
            }
        }

        public char CrossSeparator
        {
            get
            {
                return crossSeparator;
            }

            set
            {
                crossSeparator = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // Caption
            if (!String.IsNullOrEmpty(this.title))
                sb.AppendLine(this.title.Align(HorizontalAlignment.Center, this.RowTitleWidth + this.Columns.Sum(v=>v.Width) + this.Columns.Count));

            // Headers
            if (this.Headers.Count > 0)
            {
                AddRows(sb, this.Headers);
                sb.AppendLine(new string(this.rowSeparator, this.RowTitleWidth) + this.Columns.Select(v =>this.crossSeparator+new String(this.rowSeparator, v.Width)).ToStringJoin(""));
            }

            // Rows
            AddRows(sb, this.Rows);

            // Footers
            if (this.Footers.Count > 0)
            {
                sb.AppendLine(new string(this.rowSeparator, this.RowTitleWidth) + this.Columns.Select(v => this.crossSeparator + new String(this.rowSeparator, v.Width)).ToStringJoin(""));
                AddRows(sb, this.Footers);
            }

            return sb.ToString();
        }

        private void AddRows(StringBuilder sb, IList<CmdTableRow> rows) {
            foreach (CmdTableRow row in rows)
            {
                sb.Append(row.Title.PadRight(this.rowTitleWidth).Substring(0, this.rowTitleWidth));

                foreach (CmdTableColumn column in this.Columns)
                {
                    sb.Append(this.columnSeparator);
                    if (row[column.Id] == null)
                        sb.Append(new String(' ', column.Width));
                    else {
                        if (typeof(CmdTableCell).IsAssignableFrom(row[column.Id].GetType()))
                            ((CmdTableCell)row[column.Id]).ToString(column.Width);
                        else
                            sb.Append(row[column.Id].ToString().FormatString(column.Format).Align(column.Alignment, column.Width));
                    }
                }

                sb.AppendLine();
            }
        }


    }
}
