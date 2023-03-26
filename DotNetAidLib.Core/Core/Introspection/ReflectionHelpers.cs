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

        public static IDictionary<string, object> PropertiesToDictionary(this object v)
        {
            return v.PropertiesToDictionary(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        }

        public static IDictionary<string, object> PropertiesToDictionary(this object v, BindingFlags bindingFlags)
        {
            IDictionary<string, object> ret = new Dictionary<string, object>();
            var properties = v.GetType().GetProperties(bindingFlags);
            foreach (var pi in properties)
                ret.Add(pi.Name, pi.GetValue(v));
            return ret;
        }

        public static string ToStringReflection<T>(this T instance)
        {
            return instance.ToStringReflection(false);
        }

        public static string ToStringReflection<T>(this T instance, bool includeNonPublic)
        {
            return instance.ToStringReflection(
                (includeNonPublic ? BindingFlags.NonPublic : BindingFlags.Default)
                | BindingFlags.Instance | BindingFlags.Public
                , v => true);
        }

        public static string ToStringReflection<T>(this T instance, BindingFlags bindingFlags,
            Func<PropertyInfo, bool> where)
        {
            var ret = "";

            if (instance == null)
            {
                ret += "[null]";
            }
            else
            {
                IEnumerable<PropertyInfo> properties = null;

                properties = instance.GetType()
                    .GetProperties(bindingFlags)
                    .Where(prop => prop.CanRead
                                   && where(prop));

                foreach (var property in properties)
                {
                    object value = null;
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

            ret = typeof(T).Name + " { " + ret + " }";

            return ret;
        }

        private static string ObjectToString(object value)
        {
            var ret = "[null]";

            if (value != null)
            {
                var valueType = value.GetType();
                if (typeof(string).IsAssignableFrom(valueType))
                {
                    ret = "\"" + value + "\"";
                }
                else if (typeof(char).IsAssignableFrom(valueType))
                {
                    ret = "'" + value + "'";
                }
                else if (valueType.IsGenericType
                         && typeof(Nullable<>).IsAssignableFrom(valueType))
                {
                    //.GetGenericTypeDefinition ()) {
                    if ((bool) valueType.GetProperty("HasValue").GetValue(value))
                        ret = ObjectToString(valueType.GetProperty("Value").GetValue(value));
                }
                else if (typeof(IDictionary).IsAssignableFrom(valueType))
                {
                    ret = "(" + ((IDictionary) value).ToStringJoin(", ", v => ObjectToString(v)) + ")";
                }
                else if (typeof(IEnumerable).IsAssignableFrom(valueType))
                {
                    ret = "(" + ((IEnumerable) value).ToStringJoin(", ", v => ObjectToString(v)) + ")";
                }
                else if (valueType.IsPrimitive)
                {
                    ret = value.ToString();
                }
                else
                {
                    ret = "{" + value + "}";
                }
            }

            return ret;
        }

        public static object GetDefault(this Type v)
        {
            object output = null;

            if (v.IsValueType) output = Activator.CreateInstance(v);

            return output;
        }

        public static T Invoke<T>(this MethodInfo v, object instance)
        {
            return (T) v.Invoke(instance, new object[] { });
        }

        public static T Invoke<T>(this MethodInfo v, object instance, object argument)
        {
            return (T) v.Invoke(instance, new[] {argument});
        }

        public static T Invoke<T>(this MethodInfo v, object instance, object[] arguments)
        {
            return (T) v.Invoke(instance, arguments);
        }

        public static Type FindTypeInAllAssemblies(this Assembly ass, string typeFullName)
        {
            Type ret = null;

            if (typeFullName == null)
                throw new ArgumentNullException(nameof(typeFullName));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
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
                        foreach (var type in exportedTypes)
                            if (typeFullName.Equals(type.FullName))
                                ret = type;
                }

            return ret;
        }

        public static string GetTitleAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyTitleAttribute) attribs[0]).Title;
            return ret;
        }

        public static string GetDescriptionAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyDescriptionAttribute) attribs[0]).Description;
            return ret;
        }

        public static string GetConfigurationAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyConfigurationAttribute) attribs[0]).Configuration;
            return ret;
        }

        public static string GetCompanyAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyCompanyAttribute) attribs[0]).Company;
            return ret;
        }

        public static string GetProductAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyProductAttribute) attribs[0]).Product;
            return ret;
        }

        public static string GetCopyrightAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyCopyrightAttribute) attribs[0]).Copyright;
            return ret;
        }

        public static string GetTrademarkAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyTrademarkAttribute) attribs[0]).Trademark;
            return ret;
        }

        public static string GetCultureAttribute(this Assembly v)
        {
            string ret = null;
            var attribs = v.GetCustomAttributes(typeof(AssemblyCultureAttribute), true);
            if (attribs.Length > 0)
                ret = ((AssemblyCultureAttribute) attribs[0]).Culture;
            return ret;
        }
    }
}