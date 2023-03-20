using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Core.Time;

namespace DotNetAidLib
{
    public class DataReaderXmlWriter:DataReaderWriterAdapter
    {

        private Encoding encoding;
        private StreamWriter sWriter = null;
        private IDataReader dataReader;
        private String tableName;
        public DataReaderXmlWriter () : this (Encoding.UTF8) { }
        public DataReaderXmlWriter (Encoding encoding)
        {
            Assert.NotNull( encoding, nameof(encoding));
            this.encoding = encoding;
        }

        public bool AutoFlush {
            get { return this.sWriter.AutoFlush; }
            set { this.sWriter.AutoFlush = value; }
        }

        public override void Init(Stream baseStream, IDataReader dataReader, String tableName)
        {
            Assert.NotNull( baseStream, nameof(baseStream));
            Assert.NotNull( dataReader, nameof(dataReader));
            Assert.NotNullOrEmpty( tableName, nameof(tableName));

            this.sWriter = new StreamWriter(baseStream, this.encoding);
            this.dataReader = dataReader;
            this.tableName = tableName;
        }


        public override void BeforeSerialize ()
        {
            this.sWriter.WriteLine();
        }

        public override void BeforeSerializeRow (int rowIndex)
        {
            this.sWriter.Write ("<");
            this.sWriter.Write (this.tableName);
            this.sWriter.WriteLine (">");
        }

        public override void AfterSerializeRow (int rowIndex)
        {
            this.sWriter.Write ("</");
            this.sWriter.Write (this.tableName);
            this.sWriter.WriteLine(">");
        }

        public override void BeforeSerializeColumn (int columnIndex)
        {
            if (!this.dataReader.IsDBNull(columnIndex))
            {
                this.sWriter.Write("<");
                this.sWriter.Write(this.dataReader.GetName(columnIndex));
                this.sWriter.Write(">");
            }
        }
        public override void AfterSerializeColumn (int columnIndex)
        {
            if (!this.dataReader.IsDBNull(columnIndex))
            {
                this.sWriter.Write("</");
                this.sWriter.Write(this.dataReader.GetName(columnIndex));
                this.sWriter.WriteLine(">");
            }
        }

        public override void SerializeColumn (int columnIndex)
        {
            if (!this.dataReader.IsDBNull(columnIndex))
            {

                IFormatProvider nfmt = CultureInfo.InvariantCulture.NumberFormat;
                IFormatProvider dtfmt = CultureInfo.InvariantCulture.DateTimeFormat;
                Type ctype = this.dataReader.GetFieldType(columnIndex);
                if (ctype == typeof(byte))
                    this.sWriter.Write(this.dataReader.GetByte(columnIndex).ToString(nfmt));
                else if (ctype == typeof(Int16))
                    this.sWriter.Write(this.dataReader.GetInt16(columnIndex).ToString(nfmt));
                else if (ctype == typeof(Int32))
                    this.sWriter.Write(this.dataReader.GetInt32(columnIndex).ToString(nfmt));
                else if (ctype == typeof(Int64))
                    this.sWriter.Write(this.dataReader.GetInt64(columnIndex).ToString(nfmt));
                else if (ctype == typeof(Single))
                    this.sWriter.Write(this.dataReader.GetFloat(columnIndex).ToString(nfmt));
                else if (ctype == typeof(Double))
                    this.sWriter.Write(this.dataReader.GetDouble(columnIndex).ToString(nfmt));
                else if (ctype == typeof(Decimal))
                    this.sWriter.Write(this.dataReader.GetDecimal(columnIndex).ToString(nfmt));
                else if (ctype == typeof(String))
                    this.sWriter.Write(this.dataReader.GetString(columnIndex).Replace("<", "&lt;").Replace("<", "&gt;"));
                else if (ctype == typeof(Char))
                    this.sWriter.Write(this.dataReader.GetChar(columnIndex));
                else if (ctype == typeof(Boolean))
                    this.sWriter.Write(this.dataReader.GetBoolean(columnIndex).ToString().ToLower());
                else if (ctype == typeof(DateTime))
                    this.sWriter.Write(new DateTimeOffset(this.dataReader.GetDateTime(columnIndex)).ToStringISO8601(false, true));
                else if (ctype == typeof(byte[]))
                {
                    Stream input = this.dataReader.GetStream(columnIndex);
                    Stream output = new Base64StreamWriter(this.sWriter);
                    input.CopyTo(output);
                    this.sWriter.Flush();
                }
            }
        }

        public override void Flush() {
            this.sWriter.Flush();
        }
    }
}
