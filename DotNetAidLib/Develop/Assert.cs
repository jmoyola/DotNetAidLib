using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Develop
{
    /// <summary>
    /// Assert exception class
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
    /// Assert class
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Assert when value is not null
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <typeparam name="T">Typed value</typeparam>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T NotNull<T>(T value, String parameterName = null)
        {
            if (value == null)
                throw new AssertException("Value can't be null"
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is null
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <typeparam name="T">Typed value</typeparam>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Null<T>(T value, String parameterName = null)
        {
            if (value != null)
                throw new AssertException("Value can't be null"
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when string value is not null or empty
        /// </summary>
        /// <param name="value">String value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static String NotNullOrEmpty(String value, String parameterName = null)
        {
            if (String.IsNullOrEmpty(value))
                throw new AssertException("String value can't be null or empty"
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when collection value is not empty
        /// </summary>
        /// <param name="value">Collection value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static ICollection NotNullOrEmpty(ICollection value, String parameterName = null)
        {
            if (value==null || value.Count==0)
                throw new AssertException("Collection value can't be null or empty"
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when typed collection value is not empty
        /// </summary>
        /// <param name="value">Typed collection value to test</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> value, String parameterName = null)
        {
            if (value==null || value.Count==0)
                throw new AssertException("Collection value can't be null or empty"
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is greater than minValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Greater<T>(T value, T minValue, String parameterName = null) where T: IComparable
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(minValue, nameof(minValue));
            
            if (value.CompareTo(minValue)<0)
                throw new AssertException("Value must be greater than '" + minValue + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is greater or equals than minValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T GreaterOrEquals<T>(T value, T minValue, String parameterName = null) where T: IComparable
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(minValue, nameof(minValue));
            
            if (value.CompareTo(minValue)<=0)
                throw new AssertException("Value must be greater or equals than '" + minValue + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is less than maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Less<T>(T value, T maxValue, String parameterName = null) where T: IComparable
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(maxValue, nameof(maxValue));
            
            if (value.CompareTo(maxValue)>0)
                throw new AssertException("Value must be less than '" + maxValue + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is less or equals than maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T LowerOrEquals<T>(T value, T maxValue, String parameterName = null) where T: IComparable
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(maxValue, nameof(maxValue));
            
            if (value.CompareTo(maxValue)>=0)
                throw new AssertException("Value must be less or equals than '" + maxValue + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is between than minValue and maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T Between<T>(T value, T minValue, T maxValue, String parameterName = null) where T: IComparable
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(minValue, nameof(minValue));
            Assert.NotNull(maxValue, nameof(maxValue));
            
            if (value.CompareTo(maxValue)>=0 || value.CompareTo(minValue)<=0)
                throw new AssertException("Value must be between than '" + minValue + " and '" + maxValue + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when value is between or equals than minValue and maxValue
        /// </summary>
        /// <param name="value">IComparable value to test</param>
        /// <param name="minValue">IComparable minimum value to compare</param>
        /// <param name="maxValue">IComparable maximum value to compare</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static T BetweenOrEquals<T>(T value, T minValue, T maxValue, String parameterName = null) where T: IComparable
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(minValue, nameof(minValue));
            Assert.NotNull(maxValue, nameof(maxValue));
            
            if (value.CompareTo(maxValue)>0 || value.CompareTo(minValue)<0)
                throw new AssertException("Value must be between or equals than '" + minValue + " and '" + maxValue + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when string value match regex pattern
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="regexPattern">Regex patter to match</param>
        /// <param name="regexOptions">Regex options to match</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static String RegexMatch(String value, String regexPattern, RegexOptions regexOptions= RegexOptions.None, String parameterName = null)
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(regexPattern, nameof(regexPattern));

            if (!Regex.IsMatch(value, regexPattern, regexOptions))
                throw new AssertException("Value must match regex patter '" + regexPattern + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
        
        /// <summary>
        /// Assert when string value match regex
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="regex">Regex to match</param>
        /// <param name="parameterName">Parameter name (optional)</param>
        /// <returns>Return tested value (for fluent)</returns>
        /// <exception cref="AssertException">Error if not assert</exception>
        public static String RegexMatch(String value, Regex regex, String parameterName = null)
        {
            Assert.NotNull(value, nameof(value));
            Assert.NotNull(regex, nameof(regex));

            if (!regex.IsMatch(value))
                throw new AssertException("Value must match regex '" + regex + "'."
                                          + (parameterName==null?"":" in parameter '" + parameterName + "'") + ".");
            return value;
        }
    }
}
