using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace DotNetAidLib.Core.Serializer
{
    public class YAStringParser : IStringParser
    {
        private static IStringParser _Instance;

        private static readonly Regex rgTime =
            new Regex(@"^(t):(\d{4}[-/]?\d{2}[-/]?\d{2}([\s,]?\d{2}[:\.]?\d{2}([:\.]?\d{2}(\.\d{3})?)?)?)$");

        private static readonly Regex rgByte = new Regex(@"^(b):([\+]?(\d{1,3}))$");
        private static readonly Regex rgShortInteger = new Regex(@"^(s):([\+\-]?\d+)$");
        private static readonly Regex rgInteger = new Regex(@"^(i):([\+\-]?\d+)$");
        private static readonly Regex rgLongInteger = new Regex(@"^(I):([\+\-]?\d+)$");
        private static readonly Regex rgUnsignedShortInteger = new Regex(@"^(us):([\+]?\d+)$");
        private static readonly Regex rgUnsignedInteger = new Regex(@"^(ui):([\+]?\d+)$");
        private static readonly Regex rgUnsignedLongInteger = new Regex(@"^(uI):([\+]?\d+)$");
        private static readonly Regex rgFloat = new Regex(@"^(f):([\+\-]?\d+\.\d+)$");
        private static readonly Regex rgDoubleFloat = new Regex(@"^(F):([\+\-]?\d+\.\d+)$");
        private static readonly Regex rgDecimal = new Regex(@"^(d):([\+\-]?\d+\.\d+)$");
        private static readonly Regex rgBool = new Regex(@"^(b):((true|false))$");

        private readonly string _Syntax = @"Serialized syntax:
    - Null: \0
    - Char strings: <value>
    - Boolean (true/false): b:<value>
    - Date/Time(24h, separators are optionals) (yyyy-MM-dd [HH:mm[:ss[:fff]]]): t:<value>
    - Decimal number ([+/-]#0.0): d:<value>
    - Float number ([+/-]#0.0): f:<value>
    - Double Float number ([+/-]#0.0): F:<value>
    - Byte number ([+]#): b:<value>
    - Short Integer number ([+/-]#): s:<value>
    - Integer number ([+/-]#): i:<value>
    - Long Integer number ([+/-]#): I<value>
    - Unsigned Short Integer number ([+]#): us:<value>
    - Unsigned Integer number ([+]#): ui:<value>
    - Unsigned Long Integer number ([+]#): uI<value>
";

        private YAStringParser()
        {
        }

        public override string Syntax => _Syntax;

        public override string Parse(object value)
        {
            string ret = null;


            if (value == null)
            {
                ret = "\0";
            }
            else
            {
                var tValue = value.GetType();
                if (value is string)
                {
                    ret = value.ToString();
                }
                else if (value is DateTime)
                {
                    ret = "t:" + ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss:fff");
                }
                else if (tValue.IsPrimitive)
                {
                    var culture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    if (value is byte)
                        ret = "b:";
                    else if (value is short)
                        ret = "s:";
                    else if (value is int)
                        ret = "i:";
                    else if (value is long)
                        ret = "I:";
                    else if (value is ushort)
                        ret = "us:";
                    else if (value is uint)
                        ret = "ui:";
                    else if (value is ulong)
                        ret = "uI:";
                    else if (value is float)
                        ret = "f:";
                    else if (value is double)
                        ret = "F:";
                    else if (value is decimal)
                        ret = "d:";
                    else if (value is bool)
                        ret = "b:";
                    else if (value is decimal)
                        ret = "d:";

                    ret += value.ToString();
                    Thread.CurrentThread.CurrentCulture = culture;
                }
                else
                {
                    throw new Exception("Parsing error: Type is not valid. " + _Syntax);
                }
            }

            return ret;
        }

        public override object Unparse(string value, Type type = null)
        {
            if (value == null)
                return null;

            value = value.Trim();
            var lValue = value.RegexGroupsMatches(@"^(\D+):(.*)");

            object ret = null;

            if (value.Equals("\0"))
                ret = null;
            else if (rgTime.IsMatch(value))
                ret = DateTime.ParseExact(
                    value.RegexGroupsMatches(rgTime)[2].Replace(new[] {".", "-", "/", ",", ":", " "}, "")
                    , new[]
                    {
                        "yyyyMMdd",
                        "yyyyMMddHHmm",
                        "yyyyMMddHHmmss",
                        "yyyyMMddHHmmssfff"
                    }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);
            else if (rgBool.IsMatch(value))
                ret = bool.Parse(value.RegexGroupsMatches(rgBool)[2].ToLower());
            else if (rgByte.IsMatch(value))
                ret = byte.Parse(value.RegexGroupsMatches(rgByte)[2], CultureInfo.InvariantCulture.NumberFormat);
            else if (rgShortInteger.IsMatch(value))
                ret = short.Parse(value.RegexGroupsMatches(rgShortInteger)[2],
                    CultureInfo.InvariantCulture.NumberFormat);
            else if (rgInteger.IsMatch(value))
                ret = int.Parse(value.RegexGroupsMatches(rgInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            else if (rgLongInteger.IsMatch(value))
                ret = long.Parse(value.RegexGroupsMatches(rgLongInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            else if (rgUnsignedShortInteger.IsMatch(value))
                ret = ushort.Parse(value.RegexGroupsMatches(rgUnsignedShortInteger)[2],
                    CultureInfo.InvariantCulture.NumberFormat);
            else if (rgUnsignedInteger.IsMatch(value))
                ret = uint.Parse(value.RegexGroupsMatches(rgUnsignedInteger)[2],
                    CultureInfo.InvariantCulture.NumberFormat);
            else if (rgUnsignedLongInteger.IsMatch(value))
                ret = ulong.Parse(value.RegexGroupsMatches(rgUnsignedLongInteger)[2],
                    CultureInfo.InvariantCulture.NumberFormat);
            else if (rgDecimal.IsMatch(value))
                ret = decimal.Parse(value.RegexGroupsMatches(rgDecimal)[2], CultureInfo.InvariantCulture.NumberFormat);
            else if (rgFloat.IsMatch(value))
                ret = decimal.Parse(value.RegexGroupsMatches(rgFloat)[2], CultureInfo.InvariantCulture.NumberFormat);
            else if (rgDoubleFloat.IsMatch(value))
                ret = decimal.Parse(value.RegexGroupsMatches(rgDoubleFloat)[2],
                    CultureInfo.InvariantCulture.NumberFormat);
            else
                ret = value;

            return ret;
        }

        public static IStringParser Instance()
        {
            if (_Instance == null)
                _Instance = new YAStringParser();
            return _Instance;
        }
    }
}