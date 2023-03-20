using System;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Serializer
{
    public class SimpleStringParser:IStringParser
    {
        private static IStringParser _Instance = null;
        private string _Syntax = @"Serialized syntax:
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

        private static Regex rgEnum = new Regex(@"^\[.+\]$");
        private static Regex rgString = new Regex("^\".+\"$");
        private static Regex rgTime = new Regex(@"^#.+#$");
        private static Regex rgList = new Regex(@"^\{(([^;\}]+);?)+\}$");
        private static Regex rgInteger = new Regex(@"^[\+\-]?\d+$");
        private static Regex rgDecimal = new Regex(@"^[\+\-]?\d+\.\d+$");
        private static Regex rgKeyValue = new Regex(@"^([^:\s]+)\s*:\s*(.+)$");

        public override string Syntax => _Syntax;
        
        public override String Parse(Object value){
            String ret = null;



            if (value == null)
                ret = "<null>";
            else
            {
                Type tValue = value.GetType();
                if (tValue.IsEnum)
                {
                    ret = "[" + tValue.Assembly.GetName().Name + "," + tValue.FullName + "." + value.ToString() + "]";
                }
                else if (value is String)
                    ret = "\"" + value.ToString() + "\"";
                else if (value is DateTime)
                    ret = "#" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss:fff") + "#";
                else if (value is IEnumerable)
                    ret = "{" + ((IEnumerable)value).Cast<Object>().Select(v => this.Parse(v)).ToStringJoin(";") + "}";
                else if (value is KeyValuePair<String, Object>)
                    ret = ((KeyValuePair<String, Object>) value).Key + ":" + this.Parse(((KeyValuePair<String, Object>) value).Value);
                else if (tValue.IsPrimitive)
                {
                    CultureInfo culture = Thread.CurrentThread.CurrentCulture;
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

        public override Object Unparse(String value, Type type=null)
        {
            if (value == null)
                return null;

            value = value.Trim();

            Object ret=null;

            if (value.Equals("<null>"))
            {
                ret = null;
            }
            else if ((type!=null && type.IsEnum) || rgEnum.IsMatch(value))
            {
                String svalue = value.Substring(1, value.Length - 2);
                String[] sAssType = svalue.Split(',');
                String sAssembly = sAssType[0];
                String sType = sAssType[1].Substring(0, sAssType[1].LastIndexOf(".", StringComparison.InvariantCulture));
                String sTypeItem = sAssType[1].Substring(sAssType[1].LastIndexOf(".", StringComparison.InvariantCulture) + 1);

                Assembly ass = Assembly.Load(sAssembly);

                if (ass == null)
                    throw new Exception("Can't load assembly '" + sAssembly + "' for create Enum type '" + sType + "'.");

                Type enumType = ass.GetType(sType);

                if (enumType == null)
                    throw new Exception("Can't found Enum type '" + sType + "' in any assembly.");

                ret = Enum.Parse(enumType, sTypeItem);
            }
            else if ((type!=null && typeof(String).IsAssignableFrom(type)) || rgString.IsMatch(value))
            {
                ret = value.Substring(1, value.Length - 2);
            }
            else if ((type!=null && typeof(DateTime).IsAssignableFrom(type)) || rgTime.IsMatch(value)){
                ret = DateTime.ParseExact(value.Substring(1, value.Length - 2), new string[] {
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
            else if ((type!=null && typeof(Boolean).IsAssignableFrom(type)) || value.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                                                                             || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                ret = bool.Parse(value.ToLower());
            }
            else if ( (type!=null && ((type.IsGenericType && typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                       || typeof(ICollection).IsAssignableFrom(type)))
                      || rgList.IsMatch(value))
            {
                IList<Object> oRet = rgList.Match(value).Groups[2].Captures.Cast<Capture>()
                    .Select(v => this.Unparse(v.Value)).ToList();

                return oRet;
            }
            else if ((type!=null && type.IsDecimal()) || rgDecimal.IsMatch(value))
            {
                ret = decimal.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            else if ((type!=null && type.IsInteger()) || rgInteger.IsMatch(value))
            {
                ret = long.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            else if ((type!=null && typeof(KeyValuePair<String, Object>).IsAssignableFrom(type)) || rgKeyValue.IsMatch(value))
            {
                Match kvm = rgKeyValue.Match(value);
                ret = new KeyValuePair<String, Object>(kvm.Groups[1].Value, this.Unparse(kvm.Groups[2].Value));
            }
            else
                throw new Exception("Invalid value '" + value + "' parse." + _Syntax);

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
