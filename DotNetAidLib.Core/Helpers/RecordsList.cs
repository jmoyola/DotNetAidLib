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
        private String recordsSeparator = Environment.NewLine;
        private bool includeHeaderRow = false;
        private char[] fieldsSeparator = new char[] { ',' };
        private NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;
        private DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        private String nullValue = "\0";
        private bool allStringsDoubleQuoted = false;
        private bool tryParseValues = false;

        public string RecordsSeparator
        {
            get => recordsSeparator;
            set
            {
                Assert.NotNull( value, nameof(value));

                recordsSeparator = value;
            }
        }

        public char[] FieldsSeparator
        {
            get => fieldsSeparator;
            set
            {
                Assert.NotNull( value, nameof(value));

                fieldsSeparator = value;
            }
        }

        public DateTimeFormatInfo DateTimeFormat
        {
            get => this.dateTimeFormat;
            set
            {
                Assert.NotNull( value, nameof(value));

                this.dateTimeFormat = value;
            }
        }

        public NumberFormatInfo NumberFormat
        {
            get => this.numberFormat;
            set
            {
                Assert.NotNull( value, nameof(value));

                this.numberFormat = value;
            }
        }

        public string NullValue
        {
            get => nullValue;
            set
            {
                Assert.NotNull( value, nameof(value));
                nullValue = value;
            }
        }

        public bool IncludeHeaderRow { get => includeHeaderRow; set => includeHeaderRow = value; }
        public bool AllStringsDoubleQuoted { get => allStringsDoubleQuoted; set => allStringsDoubleQuoted = value; }
        public bool TryParseValues { get => tryParseValues; set => tryParseValues = value; }

        private static RecordsParserOptions defaultOptions = null;
        public static RecordsParserOptions DefaultOptions
        {
            get
            {
                if (defaultOptions == null)
                    defaultOptions = new RecordsParserOptions();
                return defaultOptions;
            }
        }

        private static RecordsParserOptions csvOptions = null;
        public static RecordsParserOptions CSVOptions
        {
            get
            {
                if (csvOptions == null)
                    csvOptions = new RecordsParserOptions() {
                        DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat,
                        NumberFormat = CultureInfo.InvariantCulture.NumberFormat,
                        FieldsSeparator = new char[] { ',' },
                        RecordsSeparator = Environment.NewLine,
                        IncludeHeaderRow = true,
                        NullValue="",
                        TryParseValues=true
                    };
                return csvOptions;
            }
        }


        public void Validate()
        {
            Assert.When(() =>
                !this.FieldsSeparator.Any(fs => this.DateTimeFormat.DateSeparator.Contains("" + fs))
            , "DateTimeFormat (date separator) must not contains fields separator char.");

            Assert.When(() =>
                !this.FieldsSeparator.Any(fs => this.DateTimeFormat.TimeSeparator.Contains("" + fs))
            , "DateTimeFormat (time separator) must not contains fields separator char.");

            Assert.When(() =>
                !this.FieldsSeparator.Any(fs => this.NumberFormat.NumberDecimalSeparator.Contains("" + fs))
            , "NumberFormat decimal separator must not contains fields separator chars.");
        }

    }

    public class RecordsList : List<IList<object>>
    {
        private IList<String> fieldNames = new List<String>();

        public RecordsList() { }

        public RecordsList(IList<String> fieldNames)
        {
            Assert.NotNull( fieldNames, nameof(fieldNames));
            this.fieldNames = fieldNames;
        }

        public RecordsList(IList<String> fieldNames, IEnumerable<IList<object>> content)
            : this(fieldNames)
        {
            Assert.NotNull( content, nameof(content));
            this.AddRange(content);
        }

        public RecordsList(IEnumerable<IList<object>> content)
            : this()
        {
            Assert.NotNull( content, nameof(content));

            this.AddRange(content);
        }

        public IList<string> FieldNames { get => fieldNames; }

        public int GetOrdinal(String fieldName)
        {
            int i = this.fieldNames.IndexOf(fieldName);

            if (i > -1)
                return i;
            else
                throw new Exception("Don't exist field with name '" + fieldName + "'");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(this.fieldNames.Count>0)
                sb.AppendLine(this.fieldNames.ToStringJoin("\t"));
            this.ToList().ForEach(v => sb.AppendLine(v.ToStringJoin("\t")));
            return sb.ToString();
        }

        public void ToString(StreamWriter sw)
        {
            if (this.fieldNames.Count > 0)
                sw.WriteLine(this.fieldNames.ToStringJoin("\t"));
            this.ToList().ForEach(v => sw.WriteLine(v.ToStringJoin("\t")));
        }

        public static implicit operator RecordsList(List<List<object>> v)
        {
            return new RecordsList(v);
        }
    }

}
