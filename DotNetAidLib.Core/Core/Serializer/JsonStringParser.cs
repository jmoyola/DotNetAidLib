using System;
using Newtonsoft.Json;

namespace DotNetAidLib.Core.Serializer
{
    public class JsonStringParser : IStringParser
    {
        private static IStringParser _Instance;

        private JsonStringParser()
        {
        }

        public override string Syntax => "JSon Syntax";

        public override string Parse(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public override object Unparse(string value, Type type = null)
        {
            if (type == null)
                return JsonConvert.DeserializeObject(value);
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