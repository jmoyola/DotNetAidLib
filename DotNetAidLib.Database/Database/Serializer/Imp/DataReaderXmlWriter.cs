using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Core.Time;
using DotNetAidLib.Database.Serializer.Core;

namespace DotNetAidLib.Database.Serializer.Imp
{
    public class DataReaderXmlWriter : DataReaderWriterAdapter
    {
        private IDataReader dataReader;

        private readonly Encoding encoding;
        private StreamWriter sWriter;
        private string tableName;

        public DataReaderXmlWriter() : this(Encoding.UTF8)
        {
        }

        public DataReaderXmlWriter(Encoding encoding)
        {
            Assert.NotNull(encoding, nameof(encoding));
            this.encoding = encoding;
        }

        public bool AutoFlush
        {
            get => sWriter.AutoFlush;
            set => sWriter.AutoFlush = value;
        }

        public override void Init(Stream baseStream, IDataReader dataReader, string tableName)
        {
            Assert.NotNull(baseStream, nameof(baseStream));
            Assert.NotNull(dataReader, nameof(dataReader));
            Assert.NotNullOrEmpty(tableName, nameof(tableName));

            sWriter = new StreamWriter(baseStream, encoding);
            this.dataReader = dataReader;
            this.tableName = tableName;
        }


        public override void BeforeSerialize()
        {
            sWriter.WriteLine();
        }

        public override void BeforeSerializeRow(int rowIndex)
        {
            sWriter.Write("<");
            sWriter.Write(tableName);
            sWriter.WriteLine(">");
        }

        public override void AfterSerializeRow(int rowIndex)
        {
            sWriter.Write("</");
            sWriter.Write(tableName);
            sWriter.WriteLine(">");
        }

        public override void BeforeSerializeColumn(int columnIndex)
        {
            if (!dataReader.IsDBNull(columnIndex))
            {
                sWriter.Write("<");
                sWriter.Write(dataReader.GetName(columnIndex));
                sWriter.Write(">");
            }
        }

        public override void AfterSerializeColumn(int columnIndex)
        {
            if (!dataReader.IsDBNull(columnIndex))
            {
                sWriter.Write("</");
                sWriter.Write(dataReader.GetName(columnIndex));
                sWriter.WriteLine(">");
            }
        }

        public override void SerializeColumn(int columnIndex)
        {
            if (!dataReader.IsDBNull(columnIndex))
            {
                IFormatProvider nfmt = CultureInfo.InvariantCulture.NumberFormat;
                IFormatProvider dtfmt = CultureInfo.InvariantCulture.DateTimeFormat;
                var ctype = dataReader.GetFieldType(columnIndex);
                if (ctype == typeof(byte))
                {
                    sWriter.Write(dataReader.GetByte(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(short))
                {
                    sWriter.Write(dataReader.GetInt16(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(int))
                {
                    sWriter.Write(dataReader.GetInt32(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(long))
                {
                    sWriter.Write(dataReader.GetInt64(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(float))
                {
                    sWriter.Write(dataReader.GetFloat(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(double))
                {
                    sWriter.Write(dataReader.GetDouble(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(decimal))
                {
                    sWriter.Write(dataReader.GetDecimal(columnIndex).ToString(nfmt));
                }
                else if (ctype == typeof(string))
                {
                    sWriter.Write(dataReader.GetString(columnIndex).Replace("<", "&lt;").Replace("<", "&gt;"));
                }
                else if (ctype == typeof(char))
                {
                    sWriter.Write(dataReader.GetChar(columnIndex));
                }
                else if (ctype == typeof(bool))
                {
                    sWriter.Write(dataReader.GetBoolean(columnIndex).ToString().ToLower());
                }
                else if (ctype == typeof(DateTime))
                {
                    sWriter.Write(new DateTimeOffset(dataReader.GetDateTime(columnIndex)).ToStringISO8601(false, true));
                }
                else if (ctype == typeof(byte[]))
                {
                    var input = dataReader.GetStream(columnIndex);
                    Stream output = new Base64StreamWriter(sWriter);
                    input.CopyTo(output);
                    sWriter.Flush();
                }
            }
        }

        public override void Flush()
        {
            sWriter.Flush();
        }
    }
}