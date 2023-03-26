using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public class RecordsParserOptions
    {
        private static RecordsParserOptions defaultOptions;

        private static RecordsParserOptions csvOptions;
        private DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        private char[] fieldsSeparator = {','};
        private string nullValue = "\0";
        private NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;
        private string recordsSeparator = Environment.NewLine;

        public string RecordsSeparator
        {
            get => recordsSeparator;
            set
            {
                Assert.NotNull(value, nameof(value));

                recordsSeparator = value;
            }
        }

        public char[] FieldsSeparator
        {
            get => fieldsSeparator;
            set
            {
                Assert.NotNull(value, nameof(value));

                fieldsSeparator = value;
            }
        }

        public DateTimeFormatInfo DateTimeFormat
        {
            get => dateTimeFormat;
            set
            {
                Assert.NotNull(value, nameof(value));

                dateTimeFormat = value;
            }
        }

        public NumberFormatInfo NumberFormat
        {
            get => numberFormat;
            set
            {
                Assert.NotNull(value, nameof(value));

                numberFormat = value;
            }
        }

        public string NullValue
        {
            get => nullValue;
            set
            {
                Assert.NotNull(value, nameof(value));
                nullValue = value;
            }
        }

        public bool IncludeHeaderRow { get; set; }
        public bool AllStringsDoubleQuoted { get; set; } = false;
        public bool TryParseValues { get; set; }

        public static RecordsParserOptions DefaultOptions
        {
            get
            {
                if (defaultOptions == null)
                    defaultOptions = new RecordsParserOptions();
                return defaultOptions;
            }
        }

        public static RecordsParserOptions CSVOptions
        {
            get
            {
                if (csvOptions == null)
                    csvOptions = new RecordsParserOptions
                    {
                        DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat,
                        NumberFormat = CultureInfo.InvariantCulture.NumberFormat,
                        FieldsSeparator = new[] {','},
                        RecordsSeparator = Environment.NewLine,
                        IncludeHeaderRow = true,
                        NullValue = "",
                        TryParseValues = true
                    };
                return csvOptions;
            }
        }


        public void Validate()
        {
            Assert.When(() =>
                    !FieldsSeparator.Any(fs => DateTimeFormat.DateSeparator.Contains("" + fs))
                , "DateTimeFormat (date separator) must not contains fields separator char.");

            Assert.When(() =>
                    !FieldsSeparator.Any(fs => DateTimeFormat.TimeSeparator.Contains("" + fs))
                , "DateTimeFormat (time separator) must not contains fields separator char.");

            Assert.When(() =>
                    !FieldsSeparator.Any(fs => NumberFormat.NumberDecimalSeparator.Contains("" + fs))
                , "NumberFormat decimal separator must not contains fields separator chars.");
        }
    }

    public class RecordsList : List<IList<object>>
    {
        public RecordsList()
        {
        }

        public RecordsList(IList<string> fieldNames)
        {
            Assert.NotNull(fieldNames, nameof(fieldNames));
            FieldNames = fieldNames;
        }

        public RecordsList(IList<string> fieldNames, IEnumerable<IList<object>> content)
            : this(fieldNames)
        {
            Assert.NotNull(content, nameof(content));
            AddRange(content);
        }

        public RecordsList(IEnumerable<IList<object>> content)
            : this()
        {
            Assert.NotNull(content, nameof(content));

            AddRange(content);
        }

        public IList<string> FieldNames { get; } = new List<string>();

        public int GetOrdinal(string fieldName)
        {
            var i = FieldNames.IndexOf(fieldName);

            if (i > -1)
                return i;
            throw new Exception("Don't exist field with name '" + fieldName + "'");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (FieldNames.Count > 0)
                sb.AppendLine(FieldNames.ToStringJoin("\t"));
            this.ToList().ForEach(v => sb.AppendLine(v.ToStringJoin("\t")));
            return sb.ToString();
        }

        public void ToString(StreamWriter sw)
        {
            if (FieldNames.Count > 0)
                sw.WriteLine(FieldNames.ToStringJoin("\t"));
            this.ToList().ForEach(v => sw.WriteLine(v.ToStringJoin("\t")));
        }

        public static implicit operator RecordsList(List<List<object>> v)
        {
            return new RecordsList(v);
        }
    }
}