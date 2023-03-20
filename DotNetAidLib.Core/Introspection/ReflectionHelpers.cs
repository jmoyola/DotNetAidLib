using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Introspection
{
    public static class ReflectionHelpers
    {
        public static bool IsAnonymous(this Type v)
        {
            return v.Name.Contains("AnonymousType") && v.Namespace == null;
        }
        
        public static IDictionary<String, Object> PropertiesToDictionary(this Object v) {
            return v.PropertiesToDictionary(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        }

        public static IDictionary<String, Object> PropertiesToDictionary(this Object v, BindingFlags bindingFlags)
        {
            IDictionary<String, Object> ret = new Dictionary<String, Object>();
            PropertyInfo[] properties = v.GetType().GetProperties(bindingFlags);
            foreach (PropertyInfo pi in properties)
                ret.Add(pi.Name, pi.GetValue(v));
            return ret;
        }
        
        public static string ToStringReflection<T>(this T instance) {
            return instance.ToStringReflection<T>(false);
        }

        public static string ToStringReflection<T>(this T instance, bool includeNonPublic)
        {
            return instance.ToStringReflection<T>(
                (includeNonPublic?BindingFlags.NonPublic:BindingFlags.Default)
                | (BindingFlags.Instance | BindingFlags.Public)
                , v=>true);
        }

        public static string ToStringReflection<T>(this T instance, BindingFlags bindingFlags, Func<PropertyInfo, bool> where)
        {
            String ret = "";

            if (((Object)instance) == null)
            {
                ret+="[null]";
            }
            else {
                IEnumerable<PropertyInfo> properties = null;

                properties = instance.GetType()
                    .GetProperties(bindingFlags)
                    .Where(prop => prop.CanRead
                                   && where(prop));

                foreach (PropertyInfo property in properties)
                {
                    Object value = null;
                    try
                    {
                        value = property.GetValue(instance);
                    }
                    catch (Exception ex)
                    {
                        value = ex.ToString();
                    }
                    ret += ", " + property.Name + " = " + ObjectToString(value);
                }
            
            }

            if (ret.StartsWith(", ", StringComparison.InvariantCulture))
                ret = ret.Substring(2);

            ret= typeof(T).Name + " { " + ret + " }";

            return ret;
        }
        
        private static String ObjectToString (Object value) {
            String ret = "[null]";

            if (value != null) {
                Type valueType = value.GetType ();
                if (typeof (String).IsAssignableFrom (valueType))
                    ret = "\"" + value.ToString () + "\"";
                else if (typeof (char).IsAssignableFrom (valueType))
                    ret = "'" + value.ToString () + "'";
                else if (valueType.IsGenericType
                         && typeof (Nullable<>).IsAssignableFrom (valueType)) {//.GetGenericTypeDefinition ()) {
                    if ((bool)valueType.GetProperty ("HasValue").GetValue (value))
                        ret = ObjectToString(valueType.GetProperty ("Value").GetValue (value));
                }
                else if (typeof(IDictionary).IsAssignableFrom(valueType))
                    ret = "(" + ((IDictionary)value).ToStringJoin(", ", v => ObjectToString(v)) + ")";
                else if (typeof (IEnumerable).IsAssignableFrom (valueType))
                    ret = "(" + ((IEnumerable)value).ToStringJoin (", ", v=>ObjectToString(v)) + ")";
                else if(valueType.IsPrimitive)
                    ret = value.ToString ();
                else
                    ret = "{" + value.ToString () + "}";
            }

            return ret;
        }
        
        public static Object GetDefault(this Type v)
        {
            object output = null;

            if (v.IsValueType)
            {
                output = Activator.CreateInstance(v);
            }

            return output;
        }
        
        public static T Invoke<T>(this MethodInfo v, Object instance)
        {
            return (T)v.Invoke(instance, new Object[] { });
        }

        public static T Invoke<T>(this MethodInfo v, Object instance, Object argument)
        {
            return (T)v.Invoke(instance, new Object[] { argument });
        }

        public static T Invoke<T>(this MethodInfo v, Object instance, Object[] arguments)
        {
            return (T)v.Invoke(instance, arguments);
        }

        public static Type FindTypeInAllAssemblies(this Assembly ass, String typeFullName)
        {
            Type ret = null;

            if (typeFullName == null)
                throw new ArgumentNullException(nameof(typeFullName));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                {
                    Type[] exportedTypes = null;
                    try
                    {
                        exportedTypes = assembly.GetExportedTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        if (e.Types != null)
                            exportedTypes = e.Types;
                    }

                    if (exportedTypes != null)
                    {
                        foreach (var type in exportedTypes)
                        {
                            if (typeFullName.Equals(type.FullName))
                                ret = type;
                        }
                    }
                }
            }

            return ret;
        }
        public static String GetTitleAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyTitleAttribute)attribs[0]).Title;
            return ret;
        }

        public static String GetDescriptionAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyDescriptionAttribute)attribs[0]).Description;
            return ret;
        }

        public static String GetConfigurationAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyConfigurationAttribute)attribs[0]).Configuration;
            return ret;
        }

        public static String GetCompanyAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyCompanyAttribute)attribs[0]).Company;
            return ret;
        }

        public static String GetProductAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyProductAttribute)attribs[0]).Product;
            return ret;
        }

        public static String GetCopyrightAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
            return ret;
        }

        public static String GetTrademarkAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyTrademarkAttribute)attribs[0]).Trademark;
            return ret;
        }

        public static String GetCultureAttribute(this Assembly v)
        {
            String ret = null;
            object[] attribs = v.GetCustomAttributes(typeof(AssemblyCultureAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyCultureAttribute)attribs[0]).Culture;
            return ret;
        }

    }
}