using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DotNetAidLib.Core.Binary;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Drawing;
using DotNetAidLib.Core.Text;

namespace System
{
    public static class TextHelper
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

        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(normalizedString.Length);

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string Escape(this string v, IList<EscapeDescription> escapeDescriptions)
        {
            foreach (var escapeDescription in escapeDescriptions)
                v = v.Escape(escapeDescription.EscapeChar, escapeDescription.Value);

            return v;
        }

        public static string Unescape(this string v, IList<EscapeDescription> escapeDescriptions)
        {
            foreach (var escapeDescription in escapeDescriptions.Reverse())
                v = v.Unescape(escapeDescription.EscapeChar, escapeDescription.Value);

            return v;
        }

        public static string Escape(this string v, char escapeChar, char value)
        {
            var i = 0;
            var ret = new StringBuilder();
            while (i < v.Length)
            {
                if (v[i] == value)
                    ret.Append(escapeChar);
                ret.Append(v[i]);
                i++;
            }

            return ret.ToString();
        }

        public static string Unescape(this string v, char escapeChar, char value)
        {
            var i = 0;
            var ret = new StringBuilder();
            while (i < v.Length)
            {
                if (v[i] == escapeChar
                    && i + 1 < v.Length && v[i + 1] == value)
                    i++;

                ret.Append(v[i]);
                i++;
            }

            return ret.ToString();
        }


        public static string EscapeList(this IList<string> v, char escapeChar, char separatorChar)
        {
            if (v == null)
                return null;

            var ret = new StringBuilder();
            for (var i = 0; i < v.Count; i++)
            {
                if (i > 0)
                    ret.Append(separatorChar);

                var s = v[i];
                ret.Append(s.Escape(escapeChar, separatorChar));
            }

            return ret.ToString();
        }

        public static IList<string> UnescapeList(this string v, char escapeChar, params char[] separatorChars)
        {
            List<string> ret = null;

            if (v != null)
            {
                ret = new List<string>();
                var i = 0;
                var item = new StringBuilder();
                while (i < v.Length)
                {
                    if (separatorChars.Any(c => c == v[i])) // Si es un separador, nuevo elemento
                    {
                        ret.Add(item.ToString());
                        item.Clear();
                        i++;
                        continue;
                    }

                    if (v[i] == escapeChar
                        && i + 1 < v.Length && separatorChars.Any(c => c == v[i + 1]))
                        i++;

                    item.Append(v[i]);
                    i++;
                }

                ret.Add(item.ToString());
            }

            return ret;
        }

        public static string EscapeCSV(this string v, char fieldSeparator)
        {
            var quote = v.Length == 0 // Si es longitud cero
                        || !v.Equals(v.Trim()) // o empieza o termina por espacios
                        || v.IndexOf(fieldSeparator) > -1; // o hay separadores de campo

            if (quote)
                v = v.Replace("\"", "\"\"");

            return quote ? "\"" + v + "\"" : v;
        }

        public static IList<string> SplitWidth(this string v, int width)
        {
            IList<string> ret = new List<string>();

            if (!string.IsNullOrEmpty(v))
            {
                var i = 0;
                var length = v.Length;
                var w = 0;
                while (i < length)
                {
                    if (length - i > width)
                        w = width;
                    else
                        w = length - i;

                    ret.Add(v.Substring(i, w));
                    i += w;
                }
            }

            return ret;
        }

        public static string Replace(this string v, IList<char> oldChars, char newChar)
        {
            var ret = v;
            oldChars.ToList().ForEach(c => ret = ret.Replace(c, newChar));
            return ret;
        }

        public static string Replace(this string v, IList<string> oldValue, string newValue)
        {
            var ret = v;
            oldValue.ToList().ForEach(c => ret = ret.Replace(c, newValue));
            return ret;
        }

        public static string Repeat(this string v, int count = 1)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
                sb.Append(v);

            return sb.ToString();
        }

        public static string Indent(this string v, int level, int columns = 120, char indentChar = ' ',
            int indentCharCount = 2)
        {
            string ret = null;
            var indentString = new string(indentChar, indentCharCount).Repeat(level);

            foreach (var l in v.GetLines())
                ret += indentString + l.SplitWidth(columns).ToStringJoin(Environment.NewLine + indentString);

            return ret;
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

        public static E CharPositionToEnumFlag<E>(this string v, char nullChar) where E : Enum
        {
            var iret = 0;

            for (var i = 0; i < v.Length; i++)
            {
                var c = v[i];
                if (c != nullChar) iret += (int) Math.Pow(2, v.Length - 1 - i);
            }

            return (E) (object) iret;
        }

        public static T Do<T>(this T v, Action<T> action)
        {
            Assert.NotNull(action, nameof(action));
            action.Invoke(v);
            return v;
        }

        public static string Append(this string v, string value, char separator)
        {
            if (string.IsNullOrEmpty(v))
                return value;
            return v + separator + value;
        }

        public static string ReplaceAllOccurrences(this string v, IEnumerable<string> occurrences, string replaceString)
        {
            var ret = v;
            occurrences.ToList().ForEach(oc => ret = ret.Replace(oc, replaceString));
            return ret;
        }

        public static string ReplaceAllOccurrences(this string v, IEnumerable<char> occurrences, char replaceChar)
        {
            var ret = v;
            occurrences.ToList().ForEach(oc => ret = ret.Replace(oc, replaceChar));
            return ret;
        }

        public static string ReplaceAllOccurrences(this string v, IEnumerable<char> occurrences, string replaceString)
        {
            var ret = v;
            occurrences.ToList().ForEach(oc => ret = ret.Replace("" + oc, replaceString));
            return ret;
        }


        public static IDictionary<string, string> KeyValueAssigmentToDictionary(this string v,
            char keyValueAssigmentChar, char keyValueSeparatorChar)
        {
            var kvac = Regex.Escape("" + keyValueAssigmentChar);
            var kvsep = Regex.Escape("" + keyValueSeparatorChar);

            return new Regex(@"\s*([^" + kvac + @"\s]+)\s*" + kvac + @"\s*([^" + kvac + kvsep + @"\s]+)\s*;?",
                    RegexOptions.Multiline)
                .Matches(v).Cast<Match>()
                .ToDictionary(vk => vk.Groups[1].Value, vk => vk.Groups[2].Value);
        }

        public static string FormatString(this string v, string format)
        {
            if (!string.IsNullOrEmpty(format))
                return string.Format("{0:" + format + "}", v);
            return v;
        }

        public static string[] SplitFirst(this string v, char separator)
        {
            var i = v.IndexOf(separator);
            if (i > -1)
                return new[] {v.Substring(0, i), v.Substring(i + 1)};
            return new[] {v};
        }

        public static string[] SplitLast(this string v, char separator)
        {
            var i = v.LastIndexOf(separator);
            if (i > -1)
                return new[] {v.Substring(0, i), v.Substring(i + 1)};
            return new[] {v};
        }

        public static KeyValuePair<string, R> SplitKeyValuePair<R>(this string v, char separator,
            Func<string, R> valueParse)
        {
            var sp = v.SplitFirst(separator);
            if (sp.Length > 1)
                return new KeyValuePair<string, R>(sp[0].Trim(), valueParse.Invoke(sp[1].Trim()));
            return new KeyValuePair<string, R>(sp[0].Trim(), valueParse.Invoke(null));
        }

        public static string Align(this string v, HorizontalAlignment align, int width)
        {
            if (v.Length > width)
            {
                v = v.Substring(0, width);
                return v;
            }

            if (align == HorizontalAlignment.Left) return v.PadRight(width);

            if (align == HorizontalAlignment.Right) return v.PadLeft(width);

            if (align == HorizontalAlignment.Center)
            {
                var i = (width - v.Length) / 2;
                return new string(' ', i) + v + new string(' ', width - (i + v.Length));
            }

            return v;
        }

        public static string RemoveTailLineBreaks(this string v)
        {
            var ret = v;
            if (ret != null)
            {
                while (ret.EndsWith("\r\n", StringComparison.InvariantCulture)) ret = ret.Substring(0, ret.Length - 2);

                while (ret.EndsWith("\n", StringComparison.InvariantCulture)) ret = ret.Substring(0, ret.Length - 1);
            }

            return ret;
        }

        public static string First(this string v, int length)
        {
            if (v == null) return null;

            var realLength = length > v.Length ? v.Length : length;
            return v.Substring(0, realLength);
        }

        public static string Last(this string v, int length)
        {
            if (v == null) return null;

            var realLength = length > v.Length ? v.Length : length;
            return v.Substring(v.Length - realLength);
        }

        public static string Concat<T>(this T v, Func<T, string> concatFunction)
        {
            return concatFunction.Invoke(v);
        }

        public static int GetWeekOfMonth(this DateTime v)
        {
            return v.GetWeekOfMonth(CultureInfo.CurrentCulture);
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

        public static IList<string> Grep(this string v, string pattern)
        {
            return v.Grep(pattern, Encoding.Default);
        }

        public static IList<string> Grep(this string v, string pattern, Encoding encoding)
        {
            var ret = new List<string>();
            foreach (var line in v.GetLines(encoding))
                if (line.RegexIsMatch(pattern))
                    ret.Add(line);
            return ret;
        }

        public static R TryFunc<T, R>(this T v, Func<T, R> function)
        {
            return v.TryFunc(function, default);
        }

        public static R TryFunc<T, R>(this T v, Func<T, R> function, R returnIfError)
        {
            Assert.NotNull(function, nameof(function));

            try
            {
                return function.Invoke(v);
            }
            catch
            {
                return returnIfError;
            }
        }

        public static bool TryCast<T>(this string v, ref T output)
        {
            return v.TryCast(ref output, (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(T)));
        }

        public static bool TryCast<T>(this string v, ref T output, IFormatProvider formatProvider)
        {
            try
            {
                if (typeof(T) == typeof(bool)
                    && new[] {"true", "y", "yes", "s", "si", "da", "gui", "1", "activo", "active", "ok", "selected"}
                        .Any(s => v.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
                    v = "True";

                if (formatProvider == null)
                    output = (T) Convert.ChangeType(v, typeof(T));
                else
                    output = (T) Convert.ChangeType(v, typeof(T), formatProvider);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T Cast<T>(this string v)
        {
            return v.Cast<T>((IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(T)));
        }

        public static T Cast<T>(this string v, IFormatProvider formatProvider)
        {
            if (typeof(T) == typeof(bool)
                && new[] {"true", "y", "yes", "s", "si", "da", "gui", "1", "activo", "active", "ok", "selected"}
                    .Any(s => v.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
                v = "True";

            if (formatProvider == null)
                return (T) Convert.ChangeType(v, typeof(T));
            return (T) Convert.ChangeType(v, typeof(T), formatProvider);
        }


        public static string TruncateEllipsis(this string v, int length)
        {
            return v.TruncateEllipsis(length, "...");
        }

        public static string TruncateEllipsis(this string v, int length, string ellipsis)
        {
            if (v.Length <= length)
                return v;
            return v.Substring(0, length - ellipsis.Length) + ellipsis;
        }

        public static bool IsNullOrEmpty(this string v)
        {
            return string.IsNullOrEmpty(v);
        }

        public static bool IsNullOrWhiteSpace(this string v)
        {
            return string.IsNullOrWhiteSpace(v);
        }


        public static R IIF<T, R>(this T value, bool condition, Func<T, R> ifTrue, Func<T, R> ifFalse)
        {
            if (condition)
                return ifTrue.Invoke(value);
            return ifFalse.Invoke(value);
        }

        public static R IIF<T, R>(this T value, Func<T, R> iifFunction)
        {
            return iifFunction.Invoke(value);
        }


        public static string Capitalize(this string v)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(v);
        }

        public static string Capitalize(this string v, CultureInfo cultureInfo)
        {
            return cultureInfo.TextInfo.ToTitleCase(v);
        }

        public static string CapitalizeFirst(this string v)
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            return v.Substring(0, 1).ToUpper(cultureInfo) + v.Substring(1).ToLower(cultureInfo);
        }

        public static string CapitalizeFirst(this string v, CultureInfo cultureInfo)
        {
            return v.Substring(0, 1).ToUpper(cultureInfo) + v.Substring(1).ToLower(cultureInfo);
        }

        public static Dictionary<string, string> ToDictionary(this string v, string regexPattern, int keyGroup,
            int valueGroup)
        {
            var ret = new Dictionary<string, string>();
            var r = new Regex(regexPattern, RegexOptions.Multiline);
            var mc = r.Matches(v);
            foreach (Match m in mc)
                ret.Add(m.Groups[keyGroup].Value, m.Groups[valueGroup].Value);

            return ret;
        }

        public static object ParseToObject(this string value)
        {
            object ret = null;
            long l = 0;
            var b = false;
            decimal d = 0;
            DateTime dt;
            if (!string.IsNullOrEmpty(value))
            {
                if (value.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = value.Substring(2);
                    if (value.Length == 2)
                        ret = Convert.ToByte(value, 16);
                    else if (value.Length == 4)
                        ret = Convert.ToUInt16(value, 16);
                    else if (value.Length == 8)
                        ret = Convert.ToUInt32(value, 16);
                    else if (value.Length == 16)
                        ret = Convert.ToUInt64(value, 16);
                }
                else if (long.TryParse(value, out l))
                {
                    ret = l;
                }
                else if (decimal.TryParse(value, out d))
                {
                    ret = d;
                }
                else if (bool.TryParse(value, out b))
                {
                    ret = b;
                }
                else if (DateTime.TryParse(value, out dt))
                {
                    ret = b;
                }
                else if (DateTime.TryParseExact(value,
                             new[] {"yyyy-MM-dd hh:mm:ss", "yyyy/MM/dd hh:mm:ss"},
                             Thread.CurrentThread.CurrentCulture.DateTimeFormat,
                             DateTimeStyles.None, out dt))
                {
                    ret = b;
                }
                else if (value.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret = true;
                }
                else
                {
                    ret = value;
                }
            }

            return ret;
        }

        public static SecureString ToSecureString(this string value)
        {
            SecureString ret = null;
            if (value != null)
            {
                ret = new SecureString();
                value.ToList().ForEach(v => ret.AppendChar(v));
            }

            return ret;
        }

        public static IList<string> SlashEndContinueLines(this IList<string> v)
        {
            var ret = new List<string>();

            string outLine = null;
            foreach (var inLine in v)
                if (inLine.EndsWith(@"\", StringComparison.InvariantCulture))
                {
                    outLine += inLine.Substring(0, inLine.Length - 1);
                }
                else
                {
                    outLine += inLine;
                    ret.Add(outLine);
                    outLine = null;
                }

            if (outLine != null) // Si hay una cotinuacion de linea en la ultima linea
                ret.Add(outLine);

            return ret;
        }

        public static IList<string> GetLines(this string v)
        {
            return v.GetLines(Encoding.UTF8);
        }

        public static IList<string> GetLines(this string v, Encoding enc)
        {
            IList<string> ret = new List<string>();
            var sr = new StreamReader(new MemoryStream(enc.GetBytes(v)));
            var line = sr.ReadLine();
            while (line != null)
            {
                ret.Add(line);
                line = sr.ReadLine();
            }

            return ret;
        }

        public static string SubstringEx(this string v, int startIndex, int length)
        {
            var ret = "";
            if (startIndex < v.Length)
            {
                if (v.Length < startIndex + length)
                    ret = v.Substring(startIndex);
                else
                    ret = v.Substring(startIndex, length);
            }

            return ret;
        }

        public static string Truncate(this string v, int length)
        {
            return v.Truncate(length, 0);
        }

        public static string Truncate(this string v, int length, int startIndex)
        {
            if (v == null)
                return v;
            if (v.Length < startIndex + length)
                return v.Substring(startIndex);
            return v.Substring(startIndex, length);
        }

        public static string Add(this string v, char value, int times)
        {
            if (times < 0)
            {
                for (var n = 0; n < times; n++)
                    v = value + v;
                return v;
            }

            for (var n = 0; n < times; n++)
                v = v + value;
            return v;
        }

        public static string Substract(this string v, int length)
        {
            if (length < 0)
                return v.Substring(0, v.Length + length);
            return v.Substring(length);
        }

        public static string SubstringRev(this string v, int negativeStartIndex, int length)
        {
            var startIndex = v.Length + negativeStartIndex;
            if (startIndex > -1 && startIndex < v.Length)
                return v.Substring(startIndex, length);
            throw new InvalidOperationException("negativeStartIndex is out of string range.");
        }

        public static string SubstringRev(this string v, int negativeStartIndex)
        {
            var startIndex = v.Length + negativeStartIndex;
            if (startIndex > -1 && startIndex < v.Length)
                return v.Substring(startIndex);
            throw new InvalidOperationException("negativeStartIndex is out of string range.");
        }

        public static string HexNormalize(this string v)
        {
            if (string.IsNullOrEmpty(v))
                return null;

            v = v.Trim();
            v = v.Replace("0x", "");
            v = v.Replace(" ", "");
            v = v.Replace(":", "");
            v = v.Replace(",", "");
            v = v.Replace("\r", "");
            v = v.Replace("\n", "");
            v = v.ToUpper();

            return v;
        }

        public static byte[] HexToByteArray(this string v)
        {
            v = v.HexNormalize();
            var NumberChars = v.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(v.Substring(i, 2), 16);
            return bytes;
        }

        public static T HexToNumber<T>(this string v) where T : struct
        {
            if (string.IsNullOrEmpty(v))
                return default;

            v = v.HexNormalize();

            if (typeof(T).Equals(typeof(byte)))
                return (T) Convert.ChangeType(Convert.ToByte(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(ushort)))
                return (T) Convert.ChangeType(Convert.ToUInt16(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(uint)))
                return (T) Convert.ChangeType(Convert.ToUInt32(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(ulong)))
                return (T) Convert.ChangeType(Convert.ToUInt64(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(short)))
                return (T) Convert.ChangeType(Convert.ToInt16(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(int)))
                return (T) Convert.ChangeType(Convert.ToInt32(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(long)))
                return (T) Convert.ChangeType(Convert.ToInt64(v, 16), typeof(T));
            if (typeof(T).Equals(typeof(float)))
                return (T) Convert.ChangeType(Convert.ToUInt32(v, 16).ToIEE754Float(), typeof(T));
            if (typeof(T).Equals(typeof(double)))
                return (T) Convert.ChangeType(Convert.ToUInt32(v, 16).ToIEE754Float(), typeof(T));
            throw new InvalidCastException("Only byte, U/Int16,32,64, single and double types are allowed.");
        }

        public static string NumberToHex<T>(this T v, string prefix = null, int size = 0) where T : struct
        {
            if (size <= 0)
            {
                if (typeof(T).Equals(typeof(byte)))
                    size = 1;
                else if (typeof(T).Equals(typeof(ushort)) || typeof(T).Equals(typeof(short)))
                    size = 2;
                else if (typeof(T).Equals(typeof(uint)) || typeof(T).Equals(typeof(int))
                                                        || typeof(T).Equals(typeof(float)))
                    size = 4;
                else if (typeof(T).Equals(typeof(ulong)) || typeof(T).Equals(typeof(long))
                                                         || typeof(T).Equals(typeof(double)))
                    size = 8;
                else
                    throw new InvalidCastException(
                        "Only byte, U/Int16,32,64, single and double types are allowed for autosize.");
            }

            return prefix + string.Format("{0:x" + (size > 0 ? "" + size * 2 : "") + "}", v);
        }

        public static string NormalizeForFilename(this string v)
        {
            if (v.Equals(".") || v.Equals(".."))
                throw new Exception("File name can't be . or ..");

            var ret = new StringBuilder();

            var re = new Regex(@"[^\\/\:\*\?""\<\>\|]");

            foreach (Match m in re.Matches(v))
                ret.Append(m.Value);

            return ret.ToString();
        }

        public static T[] ReadUntil<T>(this T[] v, T endMark)
        {
            return v.ReadUntil(endMark, 0);
        }

        public static T[] ReadUntil<T>(this T[] v, T endMark, int index)
        {
            var i = Array.IndexOf(v, endMark, index);
            if (i == -1)
                throw new Exception("Can find end of mark starting in '" + index + "'.");
            return v.Extract(i, v.Length - i);
        }

        public static T[] Extract<T>(this T[] v, int index, long length)
        {
            var ret = new T[length];
            Array.Copy(v, index, ret, 0, ret.Length);
            return ret;
        }

        public static string ToHexadecimal(this IList<byte> v)
        {
            return v.ToHexadecimal(" ");
        }

        public static string ToHexadecimal(this IList<byte> v, string byteSeparator)
        {
            var ret = new StringBuilder();
            if (v != null)
            {
                for (var i = 0; i < v.Count; i++)
                {
                    ret.AppendFormat("{0:X2}", v[i]);
                    ret.Append(byteSeparator);
                }

                if (ret.Length > 0)
                    ret.Remove(ret.Length - byteSeparator.Length, byteSeparator.Length);
            }

            return ret.ToString();
        }

        public static T[] Concat<T>(this T[] v, T[] o)
        {
            var ret = new T[v.Length + o.Length];

            Array.Copy(v, 0, ret, 0, v.Length);
            Array.Copy(o, 0, ret, v.Length, o.Length);

            return ret;
        }

        public static void Initialize<T>(this IList<T> c, T value)
        {
            c.Initialize(0, c.Count, value);
        }

        public static void Initialize<T>(this IList<T> c, int start, int length, T value)
        {
            for (var i = start; i < start + length; i++)
                c[i] = value;
        }

        public static void Initialize<T>(this IList<T> c, Func<T> method)
        {
            c.Initialize(0, c.Count, method);
        }

        public static void Initialize<T>(this IList<T> c, int start, int length, Func<T> method)
        {
            for (var i = start; i < start + length; i++)
                c[i] = method.Invoke();
        }

        public static T? Parse<T>(this string c) where T : struct
        {
            T? ret;

            if (string.IsNullOrEmpty(c))
                ret = new T?();
            else
                ret = (T) Convert.ChangeType(c, typeof(T));

            return ret;
        }

        public static T? Parse<T>(this string c, CultureInfo culture) where T : struct
        {
            T? ret;

            var aux = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            ret = c.Parse<T>();
            Thread.CurrentThread.CurrentCulture = aux;

            return ret;
        }

        public static bool TryParse<T>(this string c, out T? value) where T : struct
        {
            var ret = false;

            value = new T?();
            try
            {
                if (!string.IsNullOrEmpty(c)) value = (T) Convert.ChangeType(c, typeof(T));
                ret = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public static string SQLScape(string sqlSentence)
        {
            var ret = sqlSentence;
            ret = ret.Replace("\0", "\\0");
            ret = ret.Replace("\b", "\\b");
            ret = ret.Replace("\n", "\\n");
            ret = ret.Replace("\r", "\\r");
            ret = ret.Replace("\t", "\\t");
            ret = ret.Replace("" + (char) 26, "\\Z");
            ret = ret.Replace("\\", "\\\\");
            ret = ret.Replace("'", "\\'");
            ret = ret.Replace("%", "\\%");
            ret = ret.Replace("_", "\\_");
            return ret;
        }

        public static string ToSQLValue(this object c)
        {
            return c.ToSQLValue(-1);
        }

        public static string ToSQLValue(this object c, int maxLength)
        {
            var ci = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string ret = null;

            if (c == null)
            {
                ret = "NULL";
            }
            else
            {
                var type = Nullable.GetUnderlyingType(c.GetType());
                if (type == null) // Si es nullable
                    type = c.GetType();

                if (type.Equals(typeof(DBNull)))
                {
                    ret = "NULL";
                }
                else if (type.Equals(typeof(bool)))
                {
                    ret = ((bool) c ? 1 : 0).ToString();
                }
                else if (type.Equals(typeof(string)))
                {
                    ret = "'" + SQLScape(maxLength > -1 ? c.ToString().First(maxLength) : c.ToString()) + "'";
                }
                else if (type.Equals(typeof(DateTime)))
                {
                    ret = "'" + ((DateTime) c).ToString("yyyyMMddHHmmss") + "'";
                }
                else if (type.Equals(typeof(object)))
                {
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c));
                }
                else if (typeof(IList<byte>).IsAssignableFrom(type))
                {
                    var lc = (IList<byte>) c;
                    ret = "0x" + (maxLength > -1 ? lc.Take(maxLength) : lc).ToList().ToHexadecimal("");
                }
                else
                {
                    ret = c.ToString();
                }
            }

            Thread.CurrentThread.CurrentCulture = ci;
            return ret;
        }

        public static IList<string> RegexMatches(this string v, string pattern)
        {
            return v.RegexMatches(pattern, RegexOptions.None);
        }

        public static IList<string> RegexMatches(this string v, string pattern, RegexOptions options)
        {
            return v.RegexMatches(new Regex(pattern, options));
        }

        public static IList<string> RegexMatches(this string v, Regex regex)
        {
            var ret = new List<string>();

            foreach (Match m in regex.Matches(v)) ret.Add(m.Value);

            return ret;
        }

        public static string RegexMatch(this string v, string pattern)
        {
            return v.RegexMatch(pattern, RegexOptions.None);
        }

        public static string RegexMatch(this string v, string pattern, RegexOptions options)
        {
            return v.RegexMatch(new Regex(pattern, options));
        }

        public static string RegexMatch(this string v, Regex regex)
        {
            return v.RegexMatches(regex).FirstOrDefault();
        }

        public static bool RegexIsMatch(this string v, string pattern)
        {
            return v.RegexIsMatch(pattern, RegexOptions.None);
        }

        public static bool RegexIsMatch(this string v, string pattern, RegexOptions options)
        {
            return Regex.IsMatch(v, pattern, options);
        }

        public static bool RegexIsMatch(this string v, Regex regex)
        {
            return regex.IsMatch(v);
        }

        public static IList<string> RegexGroupsMatches(this string v, string pattern)
        {
            return v.RegexGroupsMatches(pattern, RegexOptions.None);
        }

        public static IList<string> RegexGroupsMatches(this string v, string pattern, RegexOptions options)
        {
            return v.RegexGroupsMatches(new Regex(pattern, options));
        }

        public static IList<string> RegexGroupsMatches(this string v, Regex regex)
        {
            var ret = new List<string>();

            var m = regex.Match(v);
            foreach (Group g in m.Groups)
                ret.Add(g.Value);

            return ret;
        }

        public static IList<string> RegexGroupMatchCaptures(this string v, string pattern, int regexGroupIndex)
        {
            return v.RegexGroupMatchCaptures(pattern, regexGroupIndex, RegexOptions.None);
        }

        public static IList<string> RegexGroupMatchCaptures(this string v, string pattern, int regexGroupIndex,
            RegexOptions options)
        {
            return v.RegexGroupMatchCaptures(regexGroupIndex, new Regex(pattern, options));
        }

        public static IList<string> RegexGroupMatchCaptures(this string v, int regexGroupIndex, Regex regex)
        {
            var ret = new List<string>();

            var m = regex.Match(v);
            if (m.Groups.Count - 1 < regexGroupIndex)
                throw new Exception("No valid group index number.");
            foreach (Capture c in m.Groups[regexGroupIndex].Captures) ret.Add(c.Value);

            return ret;
        }

        public static string RegexReplace(this string v, string pattern, string replacement)
        {
            return v.RegexReplace(pattern, replacement, RegexOptions.None);
        }

        public static string RegexReplace(this string v, string pattern, string replacement, RegexOptions options)
        {
            return Regex.Replace(v, pattern, replacement, options);
        }

        public static string RegexReplace(this string v, string pattern, MatchEvaluator evaluator)
        {
            return v.RegexReplace(pattern, evaluator, RegexOptions.None);
        }

        public static string RegexReplace(this string v, string pattern, MatchEvaluator evaluator, RegexOptions options)
        {
            return Regex.Replace(v, pattern, evaluator, options);
        }

        public static IList<string> GroupsMatches(this Regex regex, string v)
        {
            var ret = new List<string>();

            var m = regex.Match(v);
            foreach (Group g in m.Groups)
                ret.Add(g.Value);

            return ret;
        }

        public static IList<string> GroupMatchCaptures(this Regex regex, string v, int regexGroupIndex)
        {
            var ret = new List<string>();

            var m = regex.Match(v);
            if (m.Groups.Count - 1 < regexGroupIndex)
                throw new Exception("No valid group index number.");
            foreach (Capture c in m.Groups[regexGroupIndex].Captures) ret.Add(c.Value);

            return ret;
        }

        public class EscapeDescription
        {
            public EscapeDescription(char value, char escapeChar)
            {
                Value = value;
                EscapeChar = escapeChar;
            }

            public char Value { get; }
            public char EscapeChar { get; }
        }
    }
}