using System;
using Newtonsoft.Json;

namespace DotNetAidLib.Core.Serializer
{
    public static class SerializerHelpers
    {
        public static String ToJSON(this Object v)
        {
            if (v is String)
                throw new InvalidOperationException("String instance can not serialized to json.");
			
            return JsonConvert.SerializeObject(v);
        }

        public static dynamic FromJSON(this String v)
        {
            return JsonConvert.DeserializeObject(v);
        }

        public static T FromJSON<T>(this String v)
        {
            return JsonConvert.DeserializeObject<T>(v);
        }

    }
}