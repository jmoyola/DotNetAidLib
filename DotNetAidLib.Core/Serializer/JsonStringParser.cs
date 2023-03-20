using System;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Runtime.Serialization.Json;
using System.Text;
using DotNetAidLib.Core.Text;
using Newtonsoft.Json;

namespace DotNetAidLib.Core.Serializer
{
    public class JsonStringParser:IStringParser
    {
        private static IStringParser _Instance = null;

        private JsonStringParser() { }

        public override string Syntax => "JSon Syntax";

        public override String Parse(Object value){
            return JsonConvert.SerializeObject(value);
        }

        public override Object Unparse(String value, Type type=null)
        {
            if(type==null)
                return JsonConvert.DeserializeObject(value);
            else
                return JsonConvert.DeserializeObject(value, type);
        }
        
        public static IStringParser Instance()
        {
            if (_Instance == null)
                _Instance = new JsonStringParser();
            return _Instance;
        }
    }
}
