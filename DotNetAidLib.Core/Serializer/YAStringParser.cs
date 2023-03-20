using System;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace DotNetAidLib.Core.Serializer
{
    public class YAStringParser:IStringParser
    {
        private static IStringParser _Instance = null;
        
        private string _Syntax = @"Serialized syntax:
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
        
        private static Regex rgTime = new Regex(@"^(t):(\d{4}[-/]?\d{2}[-/]?\d{2}([\s,]?\d{2}[:\.]?\d{2}([:\.]?\d{2}(\.\d{3})?)?)?)$");
        private static Regex rgByte = new Regex(@"^(b):([\+]?(\d{1,3}))$");
        private static Regex rgShortInteger = new Regex(@"^(s):([\+\-]?\d+)$");
        private static Regex rgInteger = new Regex(@"^(i):([\+\-]?\d+)$");
        private static Regex rgLongInteger = new Regex(@"^(I):([\+\-]?\d+)$");
        private static Regex rgUnsignedShortInteger = new Regex(@"^(us):([\+]?\d+)$");
        private static Regex rgUnsignedInteger = new Regex(@"^(ui):([\+]?\d+)$");
        private static Regex rgUnsignedLongInteger = new Regex(@"^(uI):([\+]?\d+)$");
        private static Regex rgFloat = new Regex(@"^(f):([\+\-]?\d+\.\d+)$");
        private static Regex rgDoubleFloat = new Regex(@"^(F):([\+\-]?\d+\.\d+)$");
        private static Regex rgDecimal = new Regex(@"^(d):([\+\-]?\d+\.\d+)$");
        private static Regex rgBool = new Regex(@"^(b):((true|false))$");

        public override string Syntax => _Syntax;
        
        public override String Parse(Object value){
            String ret = null;



            if (value == null)
                ret = "\0";
            else
            {
                Type tValue = value.GetType();
                if (value is String)
                    ret = value.ToString();
                else if (value is DateTime)
                    ret = "t:" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss:fff");
                else if (tValue.IsPrimitive)
                {
                    CultureInfo culture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    if (value is byte)
                        ret = "b:";
                    else if (value is Int16)
                        ret = "s:";
                    else if (value is Int32)
                        ret = "i:";
                    else if (value is Int64)
                        ret = "I:";
                    else if (value is UInt16)
                        ret = "us:";
                    else if (value is UInt32)
                        ret = "ui:";
                    else if (value is UInt64)
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
                    throw new Exception("Parsing error: Type is not valid. " + this._Syntax);
            }
            return ret;
        }

        public override Object Unparse(String value, Type type=null)
        {
            if (value == null)
                return null;

            value = value.Trim();
            var lValue  = value.RegexGroupsMatches(@"^(\D+):(.*)");
            
            Object ret=null;

            if (value.Equals("\0"))
            {
                ret = null;
            }
            else if (rgTime.IsMatch(value)){
                ret = DateTime.ParseExact(value.RegexGroupsMatches(rgTime)[2].Replace(new String[]{".", "-", "/", ",", ":", " "}, "")
                    , new string[] {
                            "yyyyMMdd",
                            "yyyyMMddHHmm",
                            "yyyyMMddHHmmss",
                            "yyyyMMddHHmmssfff",
                        }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);
            }
            else if (rgBool.IsMatch(value))
            {
                ret = bool.Parse(value.RegexGroupsMatches(rgBool)[2].ToLower());
            }
            else if (rgByte.IsMatch(value))
            {
                ret = byte.Parse(value.RegexGroupsMatches(rgByte)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgShortInteger.IsMatch(value))
            {
                ret = Int16.Parse(value.RegexGroupsMatches(rgShortInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgInteger.IsMatch(value))
            {
                ret = Int32.Parse(value.RegexGroupsMatches(rgInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgLongInteger.IsMatch(value))
            {
                ret = Int64.Parse(value.RegexGroupsMatches(rgLongInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgUnsignedShortInteger.IsMatch(value))
            {
                ret = UInt16.Parse(value.RegexGroupsMatches(rgUnsignedShortInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgUnsignedInteger.IsMatch(value))
            {
                ret = UInt32.Parse(value.RegexGroupsMatches(rgUnsignedInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgUnsignedLongInteger.IsMatch(value))
            {
                ret = UInt64.Parse(value.RegexGroupsMatches(rgUnsignedLongInteger)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgDecimal.IsMatch(value))
            {
                ret = Decimal.Parse(value.RegexGroupsMatches(rgDecimal)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgFloat.IsMatch(value))
            {
                ret = Decimal.Parse(value.RegexGroupsMatches(rgFloat)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (rgDoubleFloat.IsMatch(value))
            {
                ret = Decimal.Parse(value.RegexGroupsMatches(rgDoubleFloat)[2], CultureInfo.InvariantCulture.NumberFormat);
            }
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
