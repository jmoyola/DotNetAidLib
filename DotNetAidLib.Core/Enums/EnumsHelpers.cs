using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Enums
{
    public static class EnumsHelpers
    {
        public static T ToEnum<T>(this Int64 v)
        {
            Type enumType = typeof(T);
            return (T)Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this Int32 v)
        {
            Type enumType = typeof(T);
            return (T)Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this Int16 v)
        {
            Type enumType = typeof(T);
            return (T)Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this byte v)
        {
            Type enumType = typeof(T);
            return (T)Convert.ChangeType(v, enumType);
        }

        public static T ToEnum<T>(this String v) {
            return v.ToEnum<T>(false);
        }

        public static T ToEnum<T>(this String v, bool ignoreCase)
        {
            Type enumType = typeof(T);
            return (T)Enum.Parse(enumType, v, ignoreCase);
        }


        public static String GetBaseName(this Enum v)
        {
            return v.GetType().Name;
        }

        public static String GetFullQualifieName(this Enum v)
        {
            return v.GetBaseName() + "." + v.ToString();
        }

        public static int GetValue(this Enum v)
        {
            return (int)(Object)v;
        }

        public static bool IsAnyOf<T>(this T v, params T[] includeEnums){
            return v.IsAnyOf<T>((IEnumerable<T>)includeEnums);
        }

        public static bool IsAnyOf<T>(this T v, IEnumerable<T> includeEnums)
        {
            return includeEnums.Any(e=>e.Equals(v));
        }
    }
}