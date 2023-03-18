using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Develop;

namespace System.Collections
{
    public static class CollectionHelpers
    {
        public static String ToStringJoin(this IEnumerable value,  String separator = ", ")
        {
            return String.Join((separator==null?"":separator)
                , value.Cast<Object>().Select(v => v == null ? "[NULL]" : v.ToString()));
        }
        
        public static String ToStringJoin<T>(this IEnumerable<T> value,  String separator = ", ")
        {
            return String.Join((separator==null?"":separator)
                , value.Cast<Object>().Select(v => v == null ? "[NULL]" : v.ToString()));
        }
    }
}