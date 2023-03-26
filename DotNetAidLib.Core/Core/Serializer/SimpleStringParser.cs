using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Serializer
{
    public class SimpleStringParser : IStringParser
    {
        private static IStringParser _Instance;

        private static readonly Regex rgEnum = new Regex(@"^\[.+\]$");
        private static readonly Regex rgString = new Regex("^\".+\"$");
        private static readonly Regex rgTime = new Regex(@"^#.+#$");
        private static readonly Regex rgList = new Regex(@"^\{(([^;\}]+);?)+\}$");
        private static readonly Regex rgInteger = new Regex(@"^[\+\-]?\d+$");
        private static readonly Regex rgDecimal = new Regex(@"^[\+\-]?\d+\.\d+$");
        private static readonly Regex rgKeyValue = new Regex(@"^([^:\s]+)\s*:\s*(.+)$");

        private readonly string _Syntax = @"Serialized syntax:
    - Char strings: ""chars string value""
    - Boolean (true/false): true
    - Date/Time(24h, separators are optionals) (yyyy-MM-dd [HH:mm[:ss[:fff]]]): #Date/Time value#
    - Null: <null>
    - Decimal number (#0.0): 3.141516
    - Integer number (#): 3
    - Enum item [AssemblyName,EnumTypeNameSpace.EnumItem]: [MyAssembly,WeekName.Monday]
    - Key(String)-Value K:V : Name:""pepe""
    - Collections {A;B;C;...}: {1,""pepe"",3.4D,True}
";

        private SimpleStringParser()
        {
        }

        public override string Syntax => _Syntax;

        public override string Parse(object value)
        {
            string ret = null;


            if (value == null)
            {
                ret = "<null>";
            }
            else
            {
                var tValue = value.GetType();
                if (tValue.IsEnum)
                {
                    ret = "[" + tValue.Assembly.GetName().Name + "," + tValue.FullName + "." + value + "]";
                }
                else if (value is string)
                {
                    ret = "\"" + value + "\"";
                }
                else if (value is DateTime)
                {
                    ret = "#" + ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss:fff") + "#";
                }
                else if (value is IEnumerable)
                {
                    ret = "{" + ((IEnumerable) value).Cast<object>().Select(v => Parse(v)).ToStringJoin(";") + "}";
                }
                else if (value is KeyValuePair<string, object>)
                {
                    ret = ((KeyValuePair<string, object>) value).Key + ":" +
                          Parse(((KeyValuePair<string, object>) value).Value);
                }
                else if (tValue.IsPrimitive)
                {
                    var culture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    ret = value.ToString();
                    Thread.CurrentThread.CurrentCulture = culture;
                }
                else
                {
                    ret = "<null>";
                }
            }

            return ret;
        }

        public override object Unparse(string value, Type type = null)
        {
            if (value == null)
                return null;

            value = value.Trim();

            object ret = null;

            if (value.Equals("<null>"))
            {
                ret = null;
            }
            else if ((type != null && type.IsEnum) || rgEnum.IsMatch(value))
            {
                var svalue = value.Substring(1, value.Length - 2);
                var sAssType = svalue.Split(',');
                var sAssembly = sAssType[0];
                var sType = sAssType[1].Substring(0, sAssType[1].LastIndexOf(".", StringComparison.InvariantCulture));
                var sTypeItem = sAssType[1]
                    .Substring(sAssType[1].LastIndexOf(".", StringComparison.InvariantCulture) + 1);

                var ass = Assembly.Load(sAssembly);

                if (ass == null)
                    throw new Exception("Can't load assembly '" + sAssembly + "' for create Enum type '" + sType +
                                        "'.");

                var enumType = ass.GetType(sType);

                if (enumType == null)
                    throw new Exception("Can't found Enum type '" + sType + "' in any assembly.");

                ret = Enum.Parse(enumType, sTypeItem);
            }
            else if ((type != null && typeof(string).IsAssignableFrom(type)) || rgString.IsMatch(value))
            {
                ret = value.Substring(1, value.Length - 2);
            }
            else if ((type != null && typeof(DateTime).IsAssignableFrom(type)) || rgTime.IsMatch(value))
            {
                ret = DateTime.ParseExact(value.Substring(1, value.Length - 2), new[]
                {
                    "yyyyMMdd",
                    "yyyyMMddHHmm",
                    "yyyyMMddHHmmss",
                    "yyyyMMddHHmmssfff",
                    "yyyy-MM-dd",
                    "yyyy-MM-dd HH:mm",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss:fff"
                }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);
            }
            else if ((type != null && typeof(bool).IsAssignableFrom(type)) || value.Equals("true",
                                                                               StringComparison
                                                                                   .InvariantCultureIgnoreCase)
                                                                           || value.Equals("false",
                                                                               StringComparison
                                                                                   .InvariantCultureIgnoreCase))
            {
                ret = bool.Parse(value.ToLower());
            }
            else if ((type != null && ((type.IsGenericType &&
                                        typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                                       || typeof(ICollection).IsAssignableFrom(type)))
                     || rgList.IsMatch(value))
            {
                IList<object> oRet = rgList.Match(value).Groups[2].Captures.Cast<Capture>()
                    .Select(v => Unparse(v.Value)).ToList();

                return oRet;
            }
            else if ((type != null && type.IsDecimal()) || rgDecimal.IsMatch(value))
            {
                ret = decimal.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            else if ((type != null && type.IsInteger()) || rgInteger.IsMatch(value))
            {
                ret = long.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            else if ((type != null && typeof(KeyValuePair<string, object>).IsAssignableFrom(type)) ||
                     rgKeyValue.IsMatch(value))
            {
                var kvm = rgKeyValue.Match(value);
                ret = new KeyValuePair<string, object>(kvm.Groups[1].Value, Unparse(kvm.Groups[2].Value));
            }
            else
            {
                throw new Exception("Invalid value '" + value + "' parse." + _Syntax);
            }

            return ret;
        }

        public static IStringParser Instance()
        {
            if (_Instance == null)
                _Instance = new SimpleStringParser();
            return _Instance;
        }
    }
}