using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Text;

namespace DotNetAidLib.Core.Helpers
{
    public static class SystemHelper
    {
        public enum BaseOutputFormat
        {
            Decimal,
            Octal,
            Hexadecimal,
            Binary,
            ASCII,
            Mnemonic
        }

        [Flags]
        public enum BaseOutputFormatOptions
        {
            None = 0,
            LiteralPrefix = 1,
            EscapePrefix = 2,
            ASCIIQuotes = 4
        }

        
        public static readonly Type[] UIntegerTypes =
        {
            typeof(ushort),
            typeof(uint),
            typeof(ulong)
        };

        public static readonly Type[] IntegerTypes =
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(ushort),
            typeof(uint),
            typeof(ulong)
        };

        public static readonly Type[] DecimalTypes =
        {
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public static readonly Type[] NumberTypes = new List<Type>(IntegerTypes).Union(DecimalTypes).ToArray();
        
        public static bool IsNumber(this Type v)
        {
            Assert.NotNull(v, nameof(v));
            return NumberTypes.Any(t => t.Equals(v));
        }

        public static bool IsInteger(this Type v)
        {
            Assert.NotNull(v, nameof(v));
            return IntegerTypes.Any(t => t.Equals(v));
        }

        public static bool IsUInteger(this Type v)
        {
            Assert.NotNull(v, nameof(v));
            return UIntegerTypes.Any(t => t.Equals(v));
        }

        public static bool IsDecimal(this Type v)
        {
            Assert.NotNull(v, nameof(v));
            return DecimalTypes.Any(t => t.Equals(v));
        }

        public static string ToOctal(this byte v,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None)
        {
            return (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.EscapePrefix) ? "\\" : "")
                   + (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.LiteralPrefix) ? "0" : "")
                   + Convert.ToString(v, 8).PadLeft(3, '0');
        }

        public static string ToHexadecimal(this byte v,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None)
        {
            return (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.EscapePrefix) ? "\\" : "")
                   + (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.LiteralPrefix) ? "x" : "")
                   + Convert.ToString(v, 16).PadLeft(2, '0');
        }

        public static string ToBinary(this byte v,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None)
        {
            return (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.EscapePrefix) ? "\\" : "")
                   + (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.LiteralPrefix) ? "b" : "")
                   + Convert.ToString(v, 2).PadLeft(8, '0');
        }

        public static string ToASCII(this byte v,
            BaseOutputFormat controlCharsBaseOutputFormat = BaseOutputFormat.ASCII,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None)
        {
            string ret = null;
            if (ASCII.ControlSet.ContainsKey(v))
            {
                if (controlCharsBaseOutputFormat == BaseOutputFormat.Hexadecimal)
                    ret = v.ToHexadecimal(baseOutputFormatOptions);
                else if (controlCharsBaseOutputFormat == BaseOutputFormat.Octal)
                    ret = v.ToOctal(baseOutputFormatOptions);
                else if (controlCharsBaseOutputFormat == BaseOutputFormat.Binary)
                    ret = v.ToBinary(baseOutputFormatOptions);
                else if (controlCharsBaseOutputFormat == BaseOutputFormat.Decimal)
                    ret = "" + v;
                else if (controlCharsBaseOutputFormat == BaseOutputFormat.Mnemonic)
                    ret = "[" + ASCII.ControlSet[v] + "]";
                else if (controlCharsBaseOutputFormat == BaseOutputFormat.ASCII)
                    ret = baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.ASCIIQuotes)
                        ? "'" + ASCII.FullSet[v] + "'"
                        : ASCII.FullSet[v];
            }
            else
            {
                ret = baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.ASCIIQuotes)
                    ? "'" + ASCII.FullSet[v] + "'"
                    : ASCII.FullSet[v];
            }

            return ret;
        }

        public static string ToStringBase(this byte v, IList<BaseOutputFormat> baseOutputFormats = null,
            BaseOutputFormat controlCharsBaseOutputFormat = BaseOutputFormat.ASCII,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None,
            string baseOutputFormatSeparator = "/")
        {
            Assert.NotNullOrEmpty(baseOutputFormats, nameof(baseOutputFormats));

            IList<string> ret = new List<string>();

            foreach (var outputFormat in baseOutputFormats)
                if (outputFormat == BaseOutputFormat.Decimal)
                    ret.Add(v.ToString());
                else if (outputFormat == BaseOutputFormat.Octal)
                    ret.Add(v.ToOctal(baseOutputFormatOptions));
                else if (outputFormat == BaseOutputFormat.Hexadecimal)
                    ret.Add(v.ToHexadecimal(baseOutputFormatOptions));
                else if (outputFormat == BaseOutputFormat.Binary)
                    ret.Add(v.ToBinary(baseOutputFormatOptions));
                else if (outputFormat == BaseOutputFormat.ASCII)
                    ret.Add(v.ToASCII(controlCharsBaseOutputFormat, baseOutputFormatOptions));

            return ret.ToStringJoin(baseOutputFormatSeparator);
        }

        public static void ToStringList(this IList<byte> v, StreamWriter outStream,
            IList<BaseOutputFormat> baseOutputFormats = null,
            BaseOutputFormat controlCharsBaseOutputFormat = BaseOutputFormat.ASCII,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None,
            string baseOutputFormatSeparator = "/", string itemSeparator = ", ")
        {
            Assert.NotNull(v, nameof(v));
            Assert.NotNull(outStream, nameof(outStream));
            Assert.NotNullOrEmpty(baseOutputFormats, nameof(baseOutputFormats));

            for (var i = 0; i < v.Count; i++)
            {
                if (!string.IsNullOrEmpty(itemSeparator) && i > 0)
                    outStream.Write(itemSeparator);
                outStream.Write(v[i].ToStringBase(baseOutputFormats, controlCharsBaseOutputFormat,
                    baseOutputFormatOptions, baseOutputFormatSeparator));
            }
        }

        public static string ToStringList(this IList<byte> v, IList<BaseOutputFormat> baseOutputFormats = null,
            BaseOutputFormat controlCharsBaseOutputFormat = BaseOutputFormat.ASCII,
            BaseOutputFormatOptions baseOutputFormatOptions = BaseOutputFormatOptions.None,
            string baseOutputFormatSeparator = "/", string itemSeparator = ", ")
        {
            Assert.NotNull(v, nameof(v));

            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                var sw = new StreamWriter(ms);
                v.ToStringList(sw, baseOutputFormats, controlCharsBaseOutputFormat, baseOutputFormatOptions,
                    baseOutputFormatSeparator, itemSeparator);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }
        }
        
        public static T Clone<T>(this T objectToClone) where T : class
        {
            try
            {
                Assert.NotNull(objectToClone, nameof(objectToClone));

                var m = typeof(T).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

                return (T) m.Invoke(objectToClone, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error cloning object of type '" + typeof(T).Name + "'.", ex);
            }
        }

        public static bool Invoke(this Action action, int timeoutMs, bool timeoutException = true)
        {
            var ret = false;
            var actionTh = new Thread(new ThreadStart(action));

            var d = DateTime.Now;
            actionTh.Start();

            while (actionTh.IsAlive && DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs)
                Thread.Sleep(1);

            if (actionTh.IsAlive)
            {
                actionTh.Abort();
                if (timeoutException)
                    throw new TimeoutException("Timeout exceded " + timeoutMs + "ms executing action '" +
                                               (action.Method != null ? action.Method.Name : "anonym") + "'");
                ret = true;
            }

            return ret;
        }

        public static bool Invoke<T>(this Action<T> action, T parameter, int timeoutMs, bool timeoutException = true)
        {
            var ret = false;
            var actionTh = new Thread(v => action(parameter));

            var d = DateTime.Now;
            actionTh.Start(parameter);

            while (actionTh.IsAlive && DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs)
                Thread.Sleep(1);

            if (actionTh.IsAlive)
            {
                actionTh.Abort();
                if (timeoutException)
                    throw new TimeoutException("Timeout exceded " + timeoutMs + "ms executing action '" +
                                               (action.Method != null ? action.Method.Name : "anonym") + "'");
                ret = true;
            }

            return ret;
        }

        public static bool Start(this Thread thread, int timeoutMs, bool timeoutException = true)
        {
            var ret = false;
            var d = DateTime.Now;
            thread.Start();

            while (thread.IsAlive && DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs)
                Thread.Sleep(1);

            if (thread.IsAlive)
            {
                thread.Abort();
                if (timeoutException)
                    throw new TimeoutException("Timeout exceded " + timeoutMs + "ms executing thread '" + thread.Name +
                                               "'");
                ret = true;
            }

            return ret;
        }

        public static bool Start(this Thread thread, object parameter, int timeoutMs, bool timeoutException = true)
        {
            var ret = false;
            var d = DateTime.Now;
            thread.Start(parameter);

            while (thread.IsAlive && DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs)
                Thread.Sleep(1);

            if (thread.IsAlive)
            {
                thread.Abort();
                if (timeoutException)
                    throw new TimeoutException("Timeout exceded " + timeoutMs + "ms executing thread '" + thread.Name +
                                               "'");
                ret = true;
            }

            return ret;
        }

        public static string IfNull(this string v, string valueIfNull)
        {
            if (v == null)
                return valueIfNull;
            return v;
        }

        public static string IfNullOrEmpty(this string v, string valueIfNullOrEmpty)
        {
            if (string.IsNullOrEmpty(v))
                return valueIfNullOrEmpty;
            return v;
        }

        public static string IfNotNull(this string v, string valueIfNotNull)
        {
            if (v != null)
                return valueIfNotNull;
            return v;
        }

        public static string IfNotNullOrEmpty(this string v, string valueIfNotNullOrEmpty)
        {
            if (!string.IsNullOrEmpty(v))
                return valueIfNotNullOrEmpty;
            return v;
        }

        public static string ToString<E>(this E v, char charPrefixOmmit) where E : Enum
        {
            return v.ToString()[0] == charPrefixOmmit ? v.ToString().Substring(1) : v.ToString();
        }

        public static string ToStringFlags<E>(this E v, bool includeZeroValue = false) where E : Enum
        {
            if (v.GetType().GetCustomAttribute<FlagsAttribute>(true) != null)
            {
                var ret = new List<string>();

                var ev = (int) (object) v;
                foreach (var e in Enum.GetNames(v.GetType()).Select(s => Enum.Parse(v.GetType(), s)))
                    if ((int) e == 0 && includeZeroValue)
                        ret.Add(e.ToString());
                    else if ((int) e != 0 && ((int) e & ev) == (int) e)
                        ret.Add(e.ToString());
                return ret.ToStringJoin(", ");
            }

            return v.ToString();
        }

        public static int GetWeekOfMonth(this DateTime v, CultureInfo culture)
        {
            var dayOfMonth = new DateTime(v.Year, v.Month, 1);
            var weekNumber = 1;
            while (dayOfMonth.Day <= v.Day)
            {
                if (dayOfMonth.DayOfWeek == culture.DateTimeFormat.FirstDayOfWeek)
                    weekNumber++;
                dayOfMonth = dayOfMonth.AddDays(1);
            }

            return weekNumber;
        }

        public static int GetWeekOfYear(this DateTime v)
        {
            var ci = CultureInfo.CurrentCulture;
            var weekNumber = ci.Calendar.GetWeekOfYear(v, CalendarWeekRule.FirstDay, ci.DateTimeFormat.FirstDayOfWeek);
            return weekNumber;
        }
        
    }
}