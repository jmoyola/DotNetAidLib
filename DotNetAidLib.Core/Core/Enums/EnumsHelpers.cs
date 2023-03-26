using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Enums
{
    public static class EnumsHelpers
    {
        public static T ToEnum<T>(this long v)
        {
            var enumType = typeof(T);
            return (T) Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this int v)
        {
            var enumType = typeof(T);
            return (T) Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this short v)
        {
            var enumType = typeof(T);
            return (T) Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this byte v)
        {
            var enumType = typeof(T);
            return (T) Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this string v)
        {
            return v.ToEnum<T>(false);
        }

        public static T ToEnum<T>(this string v, bool ignoreCase)
        {
            var enumType = typeof(T);
            return (T) Enum.Parse(enumType, v, ignoreCase);
        }


        public static string GetBaseName(this Enum v)
        {
            return v.GetType().Name;
        }

        public static string GetFullQualifieName(this Enum v)
        {
            return v.GetBaseName() + "." + v;
        }

        public static int GetValue(this Enum v)
        {
            return (int) (object) v;
        }

        public static bool IsAnyOf<T>(this T v, params T[] includeEnums)
        {
            return v.IsAnyOf((IEnumerable<T>) includeEnums);
        }

        public static bool IsAnyOf<T>(this T v, IEnumerable<T> includeEnums)
        {
            return includeEnums.Any(e => e.Equals(v));
        }
    }
}