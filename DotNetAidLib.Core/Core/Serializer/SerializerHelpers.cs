using System;
using Newtonsoft.Json;

namespace DotNetAidLib.Core.Serializer
{
    public static class SerializerHelpers
    {
        public static string ToJSON(this object v)
        {
            if (v is string)
                throw new InvalidOperationException("String instance can not serialized to json.");

            return JsonConvert.SerializeObject(v);
        }

        public static dynamic FromJSON(this string v)
        {
            return JsonConvert.DeserializeObject(v);
        }

        public static T FromJSON<T>(this string v)
        {
            return JsonConvert.DeserializeObject<T>(v);
        }
    }
}