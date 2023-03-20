using System;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Text;
using Newtonsoft.Json;

namespace DotNetAidLib.Core.Serializer
{
    public class XmlDCStringParser:IStringParser
    {
        private static IStringParser _Instance = null;
        
        private XmlDCStringParser() { }
        
        public override string Syntax => "Xml Syntax";
        public override String Parse(Object value)
        {
            DataContractSerializer ser = new DataContractSerializer(value.GetType());
            using (StringStream ss = new StringStream())
            {
                ser.WriteObject(ss, value);
                return ss.ToString();
            }
        }

        public override Object Unparse(String value, Type type=null)
        {
            Assert.NotNull(type, nameof(type));
            
            using (StringStream ss = new StringStream(value))
            {
                DataContractSerializer ser = new DataContractSerializer(type);
                
                return ser.ReadObject(ss);
            }
        }
        
        public static IStringParser Instance()
        {
            if (_Instance == null)
                _Instance = new XmlDCStringParser();
            return _Instance;
        }
    }
}
