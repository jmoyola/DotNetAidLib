using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Develop
{
    /// <summary>
    ///     Assert exception class
    /// </summary>
    [Serializable]
    public class AssertException : Exception
    {
        public AssertException(string message) : base(message)
        {
        }

        public AssertException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssertException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    ///     Assert class
    /// </summary>
    public static class Assert
    {
        /// <summary>
        ///     Assert when value is not null
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <typeparam name="T">Typed value</typeparam>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T NotNull<T>(T value, string parameterName = null)
        {
            if (value == null)
                throw new AssertException("Value can't be null"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is null
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <typeparam name="T">Typed value</typeparam>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Null<T>(T value, string parameterName = null)
        {
            if (value != null)
                throw new AssertException("Value can't be null"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is not null or empty
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static string NotNullOrEmpty(string value, string parameterName = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new AssertException("Value can't be null or empty"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when typed collection value is not empty
        /// </summary>
        /// <param name="value">Typed collection value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T NotNullOrEmpty<T, U>(T value, string parameterName = null) where T : IEnumerable<U>
        {
            if (value == null || !value.Any())
                throw new AssertException("Collection value can't be null or empty"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when typed collection value is not empty
        /// </summary>
        /// <param name="value">Typed collection value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T NotNullOrEmpty<T>(T value, string parameterName = null) where T : IEnumerable
        {
            if (value == null || !value.Cast<object>().Any())
                throw new AssertException("Collection value can't be null or empty"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is greater than minValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T GreaterThan<T>(T value, T minValue, string parameterName = null) where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(minValue, nameof(minValue));

            if (value.CompareTo(minValue) < 0)
                throw new AssertException("Value must be greater than '" + minValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is greater or equals than minValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T GreaterOrEqualThan<T>(T value, T minValue, string parameterName = null) where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(minValue, nameof(minValue));

            if (value.CompareTo(minValue) <= 0)
                throw new AssertException("Value must be greater or equals than '" + minValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is less than maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T LessThan<T>(T value, T maxValue, string parameterName = null) where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(maxValue, nameof(maxValue));

            if (value.CompareTo(maxValue) > 0)
                throw new AssertException("Value must be less than '" + maxValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is less or equals than maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T LessOrEqualThan<T>(T value, T maxValue, string parameterName = null) where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(maxValue, nameof(maxValue));

            if (value.CompareTo(maxValue) >= 0)
                throw new AssertException("Value must be less or equals than '" + maxValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is between minValue and maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Between<T>(T value, T minValue, T maxValue, string parameterName = null) where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(minValue, nameof(minValue));
            NotNull(maxValue, nameof(maxValue));

            if (value.CompareTo(maxValue) >= 0 || value.CompareTo(minValue) <= 0)
                throw new AssertException("Value must be between '" + minValue + " and '" + maxValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is between or equal minValue and maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T BetweenOrEqual<T>(T value, T minValue, T maxValue, string parameterName = null)
            where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(minValue, nameof(minValue));
            NotNull(maxValue, nameof(maxValue));

            if (value.CompareTo(maxValue) > 0 || value.CompareTo(minValue) < 0)
                throw new AssertException("Value must be between or equals '" + minValue + " and '" + maxValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is excluding between minValue and maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Excluding<T>(T value, T minValue, T maxValue, string parameterName = null) where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(minValue, nameof(minValue));
            NotNull(maxValue, nameof(maxValue));

            if (value.CompareTo(maxValue) < -1 || value.CompareTo(minValue) > 1)
                throw new AssertException("Value must be excluding between '" + minValue + " and '" + maxValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when value is excluding or equal between minValue and maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T ExcludingOrEqual<T>(T value, T minValue, T maxValue, string parameterName = null)
            where T : IComparable
        {
            NotNull(value, nameof(value));
            NotNull(minValue, nameof(minValue));
            NotNull(maxValue, nameof(maxValue));

            if (value.CompareTo(maxValue) <= -1 || value.CompareTo(minValue) >= 1)
                throw new AssertException("Value must be excluding or equal between '" + minValue + " and '" +
                                          maxValue + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when string value match regex pattern
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="regexPattern">Regex patter to match</param>
        /// <param name="regexOptions">Regex options to match</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static string RegexMatch(string value, string regexPattern,
            RegexOptions regexOptions = RegexOptions.None, string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(regexPattern, nameof(regexPattern));

            if (!Regex.IsMatch(value, regexPattern, regexOptions))
                throw new AssertException("Value must match regex patter '" + regexPattern + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when string value match regex
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="regex">Regex to match</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static string RegexMatch(string value, Regex regex, string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(regex, nameof(regex));

            if (!regex.IsMatch(value))
                throw new AssertException("Value must match regex '" + regex + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Custom assert by function.
        /// </summary>
        /// <param name="assertFunction">Function to verify assert</param>
        /// <param name="assertErrorDescription">Asser error description assert</param>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static void When(Func<bool> assertFunction, string assertErrorDescription)
        {
            NotNull(assertFunction, nameof(assertFunction));
            NotNullOrEmpty(assertErrorDescription, nameof(assertErrorDescription));

            if (!assertFunction())
                throw new AssertException("Assert error '" + assertErrorDescription + "'.");
        }

        /// <summary>
        ///     Custom assert by valued function.
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="assertFunction">Function to verify assert</param>
        /// <param name="assertErrorDescription">Asser error description assert</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T When<T>(T value, Func<T, bool> assertFunction, string assertErrorDescription,
            string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(assertFunction, nameof(assertFunction));
            NotNullOrEmpty(assertErrorDescription, nameof(assertErrorDescription));

            if (!assertFunction(value))
                throw new AssertException("Assert error '" + assertErrorDescription + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        /// <summary>
        ///     Assert when File/Folder is not null and exists
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Exists<T>(T value, string parameterName = null) where T : FileSystemInfo
        {
            NotNull(value, nameof(value));

            value.Refresh();
            if (!value.Exists)
                throw new AssertException("Value must be not null and exists"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");
            return value;
        }

        public static T NotType<T>(T value, Type type, string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(type, nameof(type));

            if (type.IsInstanceOfType(value.GetType()))
                throw new AssertException("Value must not be of type '" + typeof(T).Name + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");


            return value;
        }

        public static T Type<T>(T value, Type type, string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(type, nameof(type));

            if (!type.IsInstanceOfType(value.GetType()))
                throw new AssertException("Value must be of type '" + typeof(T).Name + "'"
                                          + (parameterName == null ? "" : " in parameter '" + parameterName + "'") +
                                          ".");


            return value;
        }

        public static T Including<T>(T value, IEnumerable<T> values, string parameterName = null)
        {
            NotNull(values, nameof(values));
            if (values.FirstOrDefault(v => Equals(v, value)) == null)
                throw new AssertException("Value must be including in '" + values.ToStringJoin(", ") + "'");

            return value;
        }

        public static T Excluding<T>(string label, T value, IEnumerable<T> values, string parameterName = null)
        {
            NotNull(values, nameof(values));
            if (values.FirstOrDefault(v => Equals(v, value)) != null)
                throw new AssertException("Value must be excluding in '" + values.ToStringJoin(", ") + "'");

            return value;
        }


        public static T NotNullOrEmptyOrItemDefault<T>(T value, string parameterName = null) where T : IEnumerable
        {
            NotNullOrEmpty(value, nameof(value));

            if (value.Cast<object>().Any(v => Equals(v, default(T))))
                throw new AssertException("Item of enumeration" +
                                          (parameterName == null ? "" : " '" + parameterName + "'") + " can't be " +
                                          (default(T) == null ? "null" : default(T).ToString()) + " value.");

            return value;
        }

        public static T InstanceOfType<T>(T value, Type type, string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(type, nameof(type));

            if (value == null)
                throw new AssertException(
                    ((parameterName == null ? "" : "'" + parameterName + "' ") + "value can't be null.")
                    .CapitalizeFirst());
            if (!type.IsInstanceOfType(value.GetType()))
                throw new AssertException(((parameterName == null ? "" : "'" + parameterName + "' ") +
                                           "value must be a instance of type '" + typeof(T).Name + "'.")
                    .CapitalizeFirst());

            return value;
        }

        public static T InstanceIsAssignableFrom<T>(T value, Type type, string parameterName = null)
        {
            NotNull(value, nameof(value));
            NotNull(type, nameof(type));

            if (!type.IsAssignableFrom(value.GetType()))
                throw new AssertException(((parameterName == null ? "" : "'" + parameterName + "' ") +
                                           "value must be a instance assignable of type '" + typeof(T).Name + "'.")
                    .CapitalizeFirst());

            return value;
        }

        public static string IsValidPath(string value, string parameterName = null)
        {
            NotNullOrEmpty(value, nameof(value));

            if (!Helper.IsValidPath(value))
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") + "value '" +
                                          value + "' is not a valid path.");

            return value;
        }


        public static T ContainsKey<T, K, V>(T value, K key, string parameterName = null) where T : IDictionary<K, V>
        {
            NotNullOrEmpty(value, nameof(value));
            NotNull(key, nameof(key));

            if (!value.ContainsKey(key))
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") +
                                          " don't contains key '" + key + "'.");

            return value;
        }

        public static IDictionary<string, object> ContainsKeyTyped<T>(IDictionary<string, object> value, string key,
            string parameterName = null)
        {
            NotNullOrEmpty(value, nameof(value));
            NotNullOrEmpty(key, nameof(key));

            if (!value.ContainsKey(key))
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") +
                                          " don't contains key '" + key + "'.");

            if (value[key] != null && !typeof(T).IsAssignableFrom(value[key].GetType()))
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") +
                                          " is not assignable from type '" + typeof(T) + "'.");

            return value;
        }

        public static IDictionary<string, object> ContainsKeyTypedNotNull<T>(IDictionary<string, object> value,
            string key, string parameterName = null)
        {
            NotNullOrEmpty(value, nameof(value));
            NotNullOrEmpty(key, nameof(key));

            if (!value.ContainsKey(key))
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") +
                                          " don't contains key '" + key + "'.");

            if (value[key] == null)
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") + " is null.");

            if (!typeof(T).IsAssignableFrom(value[key].GetType()))
                throw new AssertException((parameterName == null ? "" : "'" + parameterName + "' ") +
                                          " is not assignable from type '" + typeof(T) + "'.");

            return value;
        }
    }
}