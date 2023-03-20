using System;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data;
using System.Dynamic;
using System.Security;
using System.Globalization;
using System.Xml;
using DotNetAidLib.Core.Helpers;
using System.Net;
using System.Runtime.Serialization;
using System.Net.Http;
using System.Threading.Tasks;
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
		    Mnemonic,
	    }

	    [Flags]
	    public enum BaseOutputFormatOptions
	    {
		    None = 0,
		    LiteralPrefix = 1,
		    EscapePrefix = 2,
		    ASCIIQuotes = 4,
	    }

	    public class EscapeDescription
	    {
		    public EscapeDescription(char value, char escapeChar)
		    {
			    this.Value = value;
			    this.EscapeChar = escapeChar;
		    }

		    public char Value { get; }
		    public char EscapeChar { get; }
	    }

	    public static string RemoveDiacritics(this string text)
	    {
		    if (String.IsNullOrEmpty(text))
			    return text;
		    
		    string normalizedString = text.Normalize(NormalizationForm.FormD);
		    StringBuilder stringBuilder = new StringBuilder(capacity: normalizedString.Length);

		    for (int i = 0; i < normalizedString.Length; i++)
		    {
			    char c = normalizedString[i];
			    UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				    stringBuilder.Append(c);
		    }

		    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
	    }
	    
	    public static String Escape(this String v, IList<EscapeDescription> escapeDescriptions)
	    {
		    foreach (EscapeDescription escapeDescription in escapeDescriptions)
			    v = v.Escape(escapeDescription.EscapeChar, escapeDescription.Value);

		    return v;
	    }
	    
	    public static String Unescape(this String v, IList<EscapeDescription> escapeDescriptions)
	    {
		    foreach (EscapeDescription escapeDescription in escapeDescriptions.Reverse())
			    v = v.Unescape(escapeDescription.EscapeChar, escapeDescription.Value);

		    return v;
	    }
	    
	    public static String Escape(this String v, Char escapeChar, char value)
	    {
		    int i = 0;
		    StringBuilder ret = new StringBuilder();
		    while (i < v.Length)
		    {
			    if (v[i] == value)
				    ret.Append(escapeChar);
			    ret.Append(v[i]);
			    i++;
		    }

		    return ret.ToString();
	    }

	    public static String Unescape(this String v, Char escapeChar, char value)
	    {
		    int i = 0;
		    StringBuilder ret = new StringBuilder();
		    while (i < v.Length) {
			    if (v[i] == escapeChar
			        && (i + 1 < v.Length) && v[i + 1]==value)
				    i ++;
				    
			    ret.Append(v[i]);
			    i++;
		    }

		    return ret.ToString();
	    }
	    
	    
	    public static String EscapeList(this IList<String> v, char escapeChar, char separatorChar)
	    {
		    if (v == null)
			    return null;
		    
		    StringBuilder ret = new StringBuilder();
		    for (int i=0; i < v.Count;i++)
		    {
			    if (i > 0)
				    ret.Append(separatorChar);

			    String s = v[i];
			    ret.Append(s.Escape(escapeChar, separatorChar));
		    }
		    
		    return ret.ToString();
	    }
	    
	    public static IList<String> UnescapeList(this String v, char escapeChar, params char[] separatorChars)
	    {
		    List<String> ret = null;

		    if (v!=null)
		    {
			    ret = new List<String>();
			    int i = 0;
			    StringBuilder item = new StringBuilder();
			    while (i < v.Length) {
				    if (separatorChars.Any(c=> c==v[i])) // Si es un separador, nuevo elemento
				    {
					    ret.Add(item.ToString());
					    item.Clear();
					    i++;
					    continue;
				    }
				    else if (v[i] == escapeChar
				             && (i + 1 < v.Length) && separatorChars.Any(c=>c==v[i + 1]))
					    i ++;
				    
				    item.Append(v[i]);
				    i++;
			    }
			    ret.Add(item.ToString());
		    }

		    return ret;
	    }
	    
	    public static String EscapeCSV(this String v, Char fieldSeparator)
	    {
		    bool quote = (v.Length == 0 // Si es longitud cero
		                  || !v.Equals(v.Trim()) // o empieza o termina por espacios
		                  || v.IndexOf(fieldSeparator) > -1); // o hay separadores de campo

		    if (quote)
			    v = v.Replace("\"", "\"\"");
		    
		    return quote ? "\"" + v + "\"": v;
	    }

	    public static IList<String> SplitWidth(this String v, int width)
	    {
		    IList<String> ret = new List<string>();
		    
		    if (!String.IsNullOrEmpty(v))
		    {
			    int i = 0;
			    int length = v.Length;
			    int w = 0;
			    while (i < length)
			    {
				    if ((length - i) > width)
					    w = width;
				    else
					    w = length - i;

				    ret.Add(v.Substring(i, w));
				    i += w;
			    }
		    }

		    return ret;
	    }
	    
	    public static String Replace(this String v, IList<char> oldChars, char newChar)
	    {
		    String ret = v;
		    oldChars.ToList().ForEach(c=>ret=ret.Replace(c, newChar));
		    return ret;
	    }
	    
	    public static String Replace(this String v, IList<String> oldValue, String newValue)
	    {
		    String ret = v;
		    oldValue.ToList().ForEach(c=>ret=ret.Replace(c, newValue));
		    return ret;
	    }

	    public static String Repeat(this String v, int count = 1)
	    {
		    StringBuilder sb= new StringBuilder();
		    for (int i = 0; i < count; i++)
			    sb.Append(v);

		    return sb.ToString();
	    }

	    public static String Indent(this String v, int level, int columns=120, char indentChar=' ', int indentCharCount=2)
	    {
		    String ret = null;
		    String indentString = new String(indentChar, indentCharCount).Repeat(level);
		    
		    foreach (String l in v.GetLines())
		    {
			    ret += indentString + l.SplitWidth(columns).ToStringJoin(Environment.NewLine + indentString);
		    }

		    return ret;
	    }

	    public static String ToOctal(this byte v, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None)
	    {
		    return (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.EscapePrefix)?"\\":"")
		           + (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.LiteralPrefix)?"0":"")
		           + Convert.ToString(v, 8).PadLeft(3,'0');
	    }

	    public static String ToHexadecimal(this byte v, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None)
	    {
		    return (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.EscapePrefix)?"\\":"")
		           + (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.LiteralPrefix)?"x":"")
		           + Convert.ToString(v, 16).PadLeft(2,'0');
	    }

	    public static String ToBinary(this byte v, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None)
	    {
		    return (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.EscapePrefix)?"\\":"")
		           + (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.LiteralPrefix)?"b":"")
		           + Convert.ToString(v, 2).PadLeft(8,'0');
	    }

	    public static String ToASCII(this byte v, BaseOutputFormat controlCharsBaseOutputFormat= BaseOutputFormat.ASCII, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None)
	    {
		    String ret = null;
		    if (ASCII.ControlSet.ContainsKey(v))
		    {
			    if(controlCharsBaseOutputFormat==BaseOutputFormat.Hexadecimal)
				    ret= v.ToHexadecimal(baseOutputFormatOptions);
			    else if(controlCharsBaseOutputFormat==BaseOutputFormat.Octal)
				    ret= v.ToOctal(baseOutputFormatOptions);
			    else if(controlCharsBaseOutputFormat==BaseOutputFormat.Binary)
				    ret= v.ToBinary(baseOutputFormatOptions);
			    else if(controlCharsBaseOutputFormat==BaseOutputFormat.Decimal)
				    ret= ""+v;
			    else if (controlCharsBaseOutputFormat == BaseOutputFormat.Mnemonic)
				    ret= "[" + ASCII.ControlSet[v] + "]";
			    else if (controlCharsBaseOutputFormat == BaseOutputFormat.ASCII)
				    ret= (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.ASCIIQuotes)?"'" + ASCII.FullSet[v] + "'":ASCII.FullSet[v]);
		    }
		    else
			    ret= (baseOutputFormatOptions.HasFlag(BaseOutputFormatOptions.ASCIIQuotes)?"'" + ASCII.FullSet[v] + "'":ASCII.FullSet[v]);

		    return ret;
	    }

	    public static String ToStringBase(this byte v, IList<BaseOutputFormat> baseOutputFormats=null, BaseOutputFormat controlCharsBaseOutputFormat= BaseOutputFormat.ASCII, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None, String baseOutputFormatSeparator="/")
	    {
		    Assert.NotNullOrEmpty(baseOutputFormats, nameof(baseOutputFormats));
		    
		    IList<String> ret = new List<string>();

		    foreach (var outputFormat in baseOutputFormats)
		    {
			    if (outputFormat==BaseOutputFormat.Decimal)
				    ret.Add(v.ToString());
			    else if (outputFormat == BaseOutputFormat.Octal)
				    ret.Add(v.ToOctal(baseOutputFormatOptions));
			    else if (outputFormat==BaseOutputFormat.Hexadecimal)
				    ret.Add(v.ToHexadecimal(baseOutputFormatOptions));
			    else if (outputFormat==BaseOutputFormat.Binary)
				    ret.Add(v.ToBinary(baseOutputFormatOptions));
			    else if (outputFormat==BaseOutputFormat.ASCII)
				    ret.Add(v.ToASCII(controlCharsBaseOutputFormat,baseOutputFormatOptions));
		    }

		    return ret.ToStringJoin(baseOutputFormatSeparator);
	    }

	    public static void ToStringList(this IList<byte> v, StreamWriter outStream, IList<BaseOutputFormat> baseOutputFormats=null, BaseOutputFormat controlCharsBaseOutputFormat= BaseOutputFormat.ASCII, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None, String baseOutputFormatSeparator="/", String itemSeparator=", ")
	    {
		    Assert.NotNull(v, nameof(v));
		    Assert.NotNull(outStream, nameof(outStream));
		    Assert.NotNullOrEmpty(baseOutputFormats, nameof(baseOutputFormats));
		    
		    for (int i = 0; i < v.Count; i++)
		    {
			    if (!string.IsNullOrEmpty(itemSeparator) && i > 0)
				    outStream.Write(itemSeparator);
			    outStream.Write(v[i].ToStringBase(baseOutputFormats, controlCharsBaseOutputFormat, baseOutputFormatOptions, baseOutputFormatSeparator));
		    }
	    }

	    public static String ToStringList(this IList<byte> v, IList<BaseOutputFormat> baseOutputFormats=null, BaseOutputFormat controlCharsBaseOutputFormat= BaseOutputFormat.ASCII, BaseOutputFormatOptions baseOutputFormatOptions=BaseOutputFormatOptions.None, String baseOutputFormatSeparator="/", String itemSeparator=", ")
	    {
		    Assert.NotNull(v, nameof(v));

		    MemoryStream ms = null;
		    try
		    {
			    ms = new MemoryStream();
			    StreamWriter sw = new StreamWriter(ms);
			    v.ToStringList(sw, baseOutputFormats, controlCharsBaseOutputFormat, baseOutputFormatOptions, baseOutputFormatSeparator, itemSeparator);
			    sw.Flush();
			    ms.Seek(0, SeekOrigin.Begin);
			    StreamReader sr = new StreamReader(ms);
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
	    
	    public static readonly Type[] UIntegerTypes = new Type[] {
		    typeof(UInt16),
		    typeof(UInt32),
		    typeof(UInt64),
	    };
	    
	    public static readonly Type[] IntegerTypes = new Type[] {
            typeof(Byte),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64),
        };
        
        public static readonly Type[] DecimalTypes = new Type[] {
            typeof(Single),
            typeof(Double),
            typeof(Decimal)
        };
        
        public static readonly Type[] NumberTypes = new List<Type>(IntegerTypes).Union(DecimalTypes).ToArray();

        public static bool IsNumber(this Type v) {
            Assert.NotNull(v, nameof(v));
            return NumberTypes.Any(t => t.Equals(v));
        }

        public static bool IsInteger(this Type v) {
	        Assert.NotNull(v, nameof(v));
	        return IntegerTypes.Any(t => t.Equals(v));
        }
        
        public static bool IsUInteger(this Type v) {
	        Assert.NotNull(v, nameof(v));
	        return UIntegerTypes.Any(t => t.Equals(v));
        }

        public static bool IsDecimal(this Type v) {
	        Assert.NotNull(v, nameof(v));
	        return DecimalTypes.Any(t => t.Equals(v));
        }
        
        public static T Clone<T>(this T objectToClone) where T:class
        {
            try
            {
                Assert.NotNull(objectToClone, nameof(objectToClone));

                MethodInfo m = typeof(T).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

                return (T)m.Invoke(objectToClone, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error cloning object of type '" + typeof(T).Name + "'.", ex);
            }
        }

        public static bool Invoke(this Action action, int timeoutMs, bool timeoutException=true)
        {
            bool ret = false;
            Thread actionTh = new Thread(new ThreadStart(action));

            DateTime d = DateTime.Now;
            actionTh.Start();

            while (actionTh.IsAlive && (DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs))
                Thread.Sleep(1);

            if (actionTh.IsAlive){
                actionTh.Abort();
                if (timeoutException)
                    throw new System.TimeoutException("Timeout exceded " + timeoutMs + "ms executing action '" + (action.Method != null ? action.Method.Name : "anonym") + "'");
                else
                    ret = true;
            }

            return ret;
        }

        public static bool Invoke<T>(this Action<T> action, T parameter, int timeoutMs, bool timeoutException = true)
        {
            bool ret = false;
            Thread actionTh = new Thread(new ParameterizedThreadStart(new Action<Object>(v=>action(parameter))));

            DateTime d = DateTime.Now;
            actionTh.Start(parameter);

            while (actionTh.IsAlive && (DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs))
                Thread.Sleep(1);

            if (actionTh.IsAlive)
            {
                actionTh.Abort();
                if (timeoutException)
                    throw new System.TimeoutException("Timeout exceded " + timeoutMs + "ms executing action '" + (action.Method != null ? action.Method.Name : "anonym") + "'");
                else
                    ret = true;
            }

            return ret;
        }

        public static bool Start(this Thread thread, int timeoutMs, bool timeoutException = true)
        {
            bool ret = false;
            DateTime d = DateTime.Now;
            thread.Start();

            while (thread.IsAlive && (DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs))
                Thread.Sleep(1);

            if (thread.IsAlive)
            {
                thread.Abort();
                if (timeoutException)
                    throw new System.TimeoutException("Timeout exceded " + timeoutMs + "ms executing thread '" + thread.Name + "'");
                else
                    ret = true;
            }
            return ret;
        }

        public static bool Start(this Thread thread, Object parameter, int timeoutMs, bool timeoutException = true)
        {
            bool ret = false;
            DateTime d = DateTime.Now;
            thread.Start(parameter);

            while (thread.IsAlive && (DateTime.Now.Subtract(d).TotalMilliseconds < timeoutMs))
                Thread.Sleep(1);

            if (thread.IsAlive)
            {
                thread.Abort();
                if (timeoutException)
                    throw new System.TimeoutException("Timeout exceded " + timeoutMs + "ms executing thread '" + thread.Name + "'");
                else
                    ret = true;
            }
            return ret;
        }

        public static String IfNull(this String v, String valueIfNull){
            if (v == null)
                return valueIfNull;
            else
                return v;
        }

        public static String IfNullOrEmpty(this String v, String valueIfNullOrEmpty){
            if (String.IsNullOrEmpty(v))
                return valueIfNullOrEmpty;
            else
                return v;
        }

        public static String IfNotNull(this String v, String valueIfNotNull)
        {
            if (v != null)
                return valueIfNotNull;
            else
                return v;
        }

        public static String IfNotNullOrEmpty(this String v, String valueIfNotNullOrEmpty)
        {
            if (!String.IsNullOrEmpty(v))
                return valueIfNotNullOrEmpty;
            else
                return v;
        }
        
        public static String ToString<E> (this E v, char charPrefixOmmit) where E:Enum{
            return (v.ToString()[0]== charPrefixOmmit ? v.ToString().Substring(1):v.ToString());
        }

        public static String ToStringFlags<E>(this E v, bool includeZeroValue=false) where E:Enum
        {
            if (v.GetType().GetCustomAttribute<FlagsAttribute>(true) != null)
            {
                List<String> ret = new List<string>();

                Int32 ev = (Int32)(Object)v;
                foreach (Object e in Enum.GetNames(v.GetType()).Select(s => System.Enum.Parse(v.GetType(), s)))
                {
                    if ((Int32)e == 0 && includeZeroValue)
                        ret.Add(e.ToString());
                    else if (((Int32)e != 0) && ((Int32)e & ev) == (Int32)e)
                            ret.Add(e.ToString());
                }
                return ret.ToStringJoin(", ");
            }
            else
                return v.ToString();
        }

        public static E CharPositionToEnumFlag<E>(this String v, char nullChar) where E:Enum
        {
            int iret = 0;

            for (int i = 0; i < v.Length; i++) {
                char c = v[i];
                if (c != nullChar){
                    iret +=(int)Math.Pow(2, (v.Length - 1) - i);
                }
            }
            return (E)(Object)iret;
        }

        public static T Do<T>(this T v, Action<T> action){
            Assert.NotNull(action, nameof(action));
            action.Invoke(v);
            return v;
        }
        
        public static String Append(this String v, String value, char separator)
        {
            if (String.IsNullOrEmpty(v))
                return value;
            else
                return v + separator + value;
        }

        public static String ReplaceAllOccurrences(this String v, IEnumerable<String> occurrences, String replaceString)
        {
            String ret = v;
            occurrences.ToList().ForEach(oc=>ret=ret.Replace(oc,replaceString));
            return ret;
        }

        public static String ReplaceAllOccurrences(this String v, IEnumerable<char> occurrences, char replaceChar)
        {
            String ret = v;
            occurrences.ToList().ForEach(oc => ret = ret.Replace(oc, replaceChar));
            return ret;
        }

        public static String ReplaceAllOccurrences(this String v, IEnumerable<char> occurrences, String replaceString)
        {
            String ret = v;
            occurrences.ToList().ForEach(oc => ret = ret.Replace("" + oc, replaceString));
            return ret;
        }

        

        

        public static IDictionary<String, String> KeyValueAssigmentToDictionary(this String v, char keyValueAssigmentChar, char keyValueSeparatorChar) {
            String kvac = Regex.Escape(""+keyValueAssigmentChar);
            String kvsep = Regex.Escape("" + keyValueSeparatorChar);

            return new Regex(@"\s*([^" + kvac + @"\s]+)\s*" + kvac + @"\s*([^" + kvac + kvsep + @"\s]+)\s*;?", RegexOptions.Multiline)
                .Matches(v).Cast<Match>()
                .ToDictionary(vk => vk.Groups[1].Value, vk => vk.Groups[2].Value);
        }

        public static String FormatString(this String v, String format)
        {
            if (!String.IsNullOrEmpty(format))
                return String.Format("{0:" + format + "}", v);
            else
                return v.ToString();
        }

        public static String[] SplitFirst(this String v, char separator)
        {
            int i = v.IndexOf(separator);
            if (i > -1)
                return new string[] { v.Substring(0, i), v.Substring(i + 1) };
            else
                return new string[] { v };
        }

        public static String[] SplitLast(this String v, char separator)
        {
            int i = v.LastIndexOf(separator);
            if (i > -1)
                return new string[] { v.Substring(0, i), v.Substring(i + 1) };
            else
                return new string[] { v };
        }

        public static KeyValuePair<String, R> SplitKeyValuePair<R>(this String v, char separator, Func<String, R> valueParse)
        {
            String[] sp = v.SplitFirst(separator);
            if (sp.Length > 1)
                return new KeyValuePair<string, R>(sp[0].Trim(), valueParse.Invoke(sp[1].Trim()));
            else
                return new KeyValuePair<string, R>(sp[0].Trim(), valueParse.Invoke(null));
        }
        
        public static String Align(this String v, HorizontalAlignment align, int width)
        {
            if (v.Length > width)
            {
                v = v.Substring(0, width);
                return v;
            }

            if (align == HorizontalAlignment.Left)
                return v.PadRight(width);
            else if (align == HorizontalAlignment.Right)
                return v.PadLeft(width);
            else if (align == HorizontalAlignment.Center)
            {
                int i = (width - v.Length) / 2;
                return new string(' ', i) + v + new string(' ', (width - (i + v.Length)));
            }

            return v;
        }

        public static String RemoveTailLineBreaks(this String v) {

            String ret = v;
            if (ret != null)
            {
                while (ret.EndsWith("\r\n", StringComparison.InvariantCulture))
                {
                    ret = ret.Substring(0, ret.Length - 2);
                }

                while (ret.EndsWith("\n", StringComparison.InvariantCulture))
                {
                    ret = ret.Substring(0, ret.Length - 1);
                }
            }
            return ret;
        }

        public static String First(this String v, int length)
        {
            if (v == null)
                return null;
            else
            {
                int realLength = (length > v.Length ? v.Length : length);
                return v.Substring(0, realLength);
            }
        }

        public static String Last(this String v, int length)
        {
            if (v == null)
                return null;
            else
            {
                int realLength = (length > v.Length ? v.Length : length);
                return v.Substring(v.Length-realLength);
            }
        }

        public static String Concat<T>(this T v, Func<T, String> concatFunction)
        {
            return concatFunction.Invoke(v);
        }

        public static int GetWeekOfMonth(this DateTime v){
            return v.GetWeekOfMonth(CultureInfo.CurrentCulture);
        }

        public static int GetWeekOfMonth(this DateTime v, CultureInfo culture)
        {
            DateTime dayOfMonth = new DateTime(v.Year, v.Month, 1);
            int weekNumber = 1;
            while (dayOfMonth.Day<=v.Day)
            {
                if (dayOfMonth.DayOfWeek == culture.DateTimeFormat.FirstDayOfWeek)
                    weekNumber++;
                dayOfMonth = dayOfMonth.AddDays(1);
            }

            return weekNumber;
        }

        public static int GetWeekOfYear(this DateTime v)
        {

            CultureInfo ci = CultureInfo.CurrentCulture;
            int weekNumber = ci.Calendar.GetWeekOfYear(v, CalendarWeekRule.FirstDay, ci.DateTimeFormat.FirstDayOfWeek);
            return weekNumber;
        }
        
        public static IList<String> Grep(this String v, String pattern){
            return v.Grep(pattern, Encoding.Default);
        }

        public static IList<String> Grep(this String v, String pattern, Encoding encoding)
        {
            List<String> ret = new List<string>();
            foreach(String line in v.GetLines(encoding)){
                if (line.RegexIsMatch(pattern))
                    ret.Add(line);
            }
            return ret;
        }

        public static R TryFunc<T, R>(this T v, Func<T, R> function)
        {
            return v.TryFunc(function, default(R));
        }

        public static R TryFunc<T,R>(this T v, Func<T,R> function, R returnIfError)
        {
            Assert.NotNull(function, nameof(function));

            try {
                return function.Invoke(v);
            }
            catch {
                return returnIfError;
            }
        }

        public static bool TryCast<T>(this String v, ref T output){
            return v.TryCast<T>(ref output, (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(T)));
        }

        public static bool TryCast<T>(this String v, ref T output, IFormatProvider formatProvider){
            try{
                if (typeof(T) == typeof(bool)
                    && new String[] { "true", "y", "yes", "s", "si", "da", "gui", "1", "activo", "active", "ok", "selected" }
                        .Any(s => v.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
                    v = "True";
                
                if(formatProvider==null)
                    output = (T)Convert.ChangeType(v, typeof(T));
                else
                    output = (T)Convert.ChangeType(v, typeof(T), formatProvider);
                return true;
            }
            catch{
                return false;
            }
        }

        public static T Cast<T>(this String v) {
            return v.Cast<T>((IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(T)));
        }

        public static T Cast<T>(this String v, IFormatProvider formatProvider){
            if (typeof(T) == typeof(bool)
                && new String[] { "true", "y", "yes", "s", "si", "da", "gui", "1", "activo", "active", "ok", "selected" }
                    .Any(s => v.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
                v = "True";

            if (formatProvider == null)
                return (T)Convert.ChangeType(v, typeof(T));
            else
                return (T)Convert.ChangeType(v, typeof(T), formatProvider);
        }


        public static String TruncateEllipsis(this String v, int length){
    		return v.TruncateEllipsis(length, "...");
		}

        public static String TruncateEllipsis(this String v, int length, String ellipsis)
        {
            if (v.Length <= length)
                return v;
            else
                return v.Substring(0, length - ellipsis.Length) + ellipsis;
        }

        public static bool IsNullOrEmpty (this String v)
		{
			return String.IsNullOrEmpty(v);
		}

		public static bool IsNullOrWhiteSpace (this String v)
		{
			return String.IsNullOrWhiteSpace(v);
		}

		

		

		

        public static R IIF<T, R>(this T value, bool condition, Func<T,R> ifTrue, Func<T, R> ifFalse) {
            if (condition)
                return ifTrue.Invoke(value);
            else
                return ifFalse.Invoke(value);
        }

        public static R IIF<T, R>(this T value, Func<T, R> iifFunction)
        {
            return iifFunction.Invoke(value);
        }

        

		

		public static String Capitalize(this String v)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(v);
		}

		public static String Capitalize(this String v, CultureInfo cultureInfo)
		{
			return cultureInfo.TextInfo.ToTitleCase(v);
		}

		public static String CapitalizeFirst(this String v)
		{
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			return v.Substring(0, 1).ToUpper(cultureInfo) + v.Substring(1).ToLower(cultureInfo);
		}

		public static String CapitalizeFirst(this String v, CultureInfo cultureInfo)
		{
			return v.Substring(0, 1).ToUpper(cultureInfo) + v.Substring(1).ToLower(cultureInfo);
		}
		
        public static Dictionary<String, String> ToDictionary (this String v, String regexPattern, int keyGroup, int valueGroup) {
			Dictionary<String, String> ret = new Dictionary<String, String> ();
			Regex r = new Regex (regexPattern, RegexOptions.Multiline);
			MatchCollection mc = r.Matches (v);
			foreach (Match m in mc)
				ret.Add (m.Groups[keyGroup].Value, m.Groups [valueGroup].Value);
			
			return ret;
		}

        public static Object ParseToObject (this String value)
        {
	        Object ret = null;
	        long l = 0;
	        bool b = false;
	        decimal d = 0;
	        DateTime dt;
	        if (!String.IsNullOrEmpty (value)) {
		        if (value.StartsWith ("0x", StringComparison.InvariantCultureIgnoreCase)) {
			        value = value.Substring (2);
			        if (value.Length == 2)
				        ret = Convert.ToByte (value, 16);
			        else if (value.Length == 4)
				        ret = Convert.ToUInt16 (value, 16);
			        else if (value.Length == 8)
				        ret = Convert.ToUInt32 (value, 16);
			        else if (value.Length == 16)
				        ret = Convert.ToUInt64 (value, 16);
		        } else if (long.TryParse (value, out l))
			        ret = l;
		        else if (Decimal.TryParse (value, out d))
			        ret = d;
		        else if (Boolean.TryParse (value, out b))
			        ret = b;
		        else if (DateTime.TryParse (value, out dt))
			        ret = b;
		        else if (DateTime.TryParseExact (value,
			                 new String [] { "yyyy-MM-dd hh:mm:ss", "yyyy/MM/dd hh:mm:ss" },
			                 Thread.CurrentThread.CurrentCulture.DateTimeFormat,
			                 System.Globalization.DateTimeStyles.None, out dt))
			        ret = b;
		        else if (value.Equals ("yes", StringComparison.InvariantCultureIgnoreCase))
			        ret = true;
		        else
			        ret = value;
	        }

	        return ret;
        }
        
		public static SecureString ToSecureString (this String value) {
			SecureString ret = null;
			if (value != null) {
				ret = new SecureString ();
				value.ToList().ForEach(v=>ret.AppendChar(v));
			}
			return ret;
		}

        public static IList<String> SlashEndContinueLines(this IList<String> v) {
            List<String> ret = new List<String>();

            String outLine = null;
            foreach (String inLine in v) {
                if (inLine.EndsWith(@"\", StringComparison.InvariantCulture)){
                    outLine += inLine.Substring(0, inLine.Length - 1);
                }
                else {
                    outLine += inLine;
                    ret.Add(outLine);
                    outLine = null;
                }
            }
            if (outLine != null) // Si hay una cotinuacion de linea en la ultima linea
                ret.Add(outLine);

            return ret;
        }

        public static IList<String> GetLines(this String v){
			return v.GetLines(Encoding.UTF8);
		}

		public static IList<String> GetLines(this String v, Encoding enc)
		{
			IList<String> ret = new List<String> ();
			StreamReader sr = new StreamReader (new MemoryStream(enc.GetBytes(v)));
			String line = sr.ReadLine ();
			while (line != null) {
				ret.Add (line);
				line = sr.ReadLine ();
			}
			return ret;
		}

		public static String SubstringEx(this String v, int startIndex, int length){
			String ret = "";
			if (startIndex < v.Length) {
				if (v.Length < (startIndex + length))
					ret = v.Substring (startIndex);
				else
					ret = v.Substring (startIndex, length);
			}
			return ret;
		}

        public static String Truncate(this String v, int length) {
            return v.Truncate(length, 0);
        }

        public static String Truncate(this String v, int length, int startIndex)
        {
            if (v == null)
                return v;
            else if (v.Length < (startIndex + length))
                return v.Substring(startIndex);
            else
                return v.Substring(startIndex, length);
        }

        public static String Add(this String v, char value, int times){
			if (times < 0) {
				for (int n = 0; n < times; n++)
					v=value+v;
				return v;
			}
			else {
				for (int n = 0; n < times; n++)
					v=v+value;
				return v;
			}
		}
			
		public static String Substract(this String v, int length){
			if (length < 0)
				return v.Substring (0, v.Length + length);
			else
				return v.Substring (length);
		}

		public static String SubstringRev(this String v, int negativeStartIndex, int length){
			int startIndex=v.Length+negativeStartIndex;
			if (startIndex > -1 && startIndex < v.Length) {
				return v.Substring (startIndex, length);
			} else
				throw new InvalidOperationException ("negativeStartIndex is out of string range.");
		}

		public static String SubstringRev(this String v, int negativeStartIndex){
			int startIndex=v.Length+negativeStartIndex;
			if (startIndex > -1 && startIndex < v.Length) {
				return v.Substring (startIndex);
			} else
				throw new InvalidOperationException ("negativeStartIndex is out of string range.");
		}

		public static String HexNormalize (this String v)
        {
            if (String.IsNullOrEmpty (v))
                return null;

            v = v.Trim ();
            v = v.Replace ("0x", "");
            v = v.Replace (" ", "");
            v = v.Replace (":", "");
            v = v.Replace (",", "");
            v = v.Replace ("\r", "");
            v = v.Replace ("\n", "");
            v = v.ToUpper ();

            return v;
        }

        public static byte[] HexToByteArray(this String v)
		{
			v = v.HexNormalize();
			int NumberChars = v.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(v.Substring(i, 2), 16);
			return bytes;
		}

        public static T HexToNumber<T> (this String v) where T:struct
        {
            if (String.IsNullOrEmpty (v))
                return default(T);

            v = v.HexNormalize ();

            if (typeof (T).Equals (typeof (byte)))
                return (T)Convert.ChangeType (Convert.ToByte (v, 16), typeof (T));
            else if (typeof (T).Equals (typeof (UInt16)))
                return (T)Convert.ChangeType(Convert.ToUInt16 (v, 16), typeof(T));
            else if (typeof (T).Equals (typeof (UInt32)))
                return (T)Convert.ChangeType (Convert.ToUInt32 (v, 16), typeof (T));
            else if (typeof (T).Equals (typeof (UInt64)))
                return (T)Convert.ChangeType (Convert.ToUInt64 (v, 16), typeof (T));
            else if (typeof (T).Equals (typeof (Int16)))
                return (T)Convert.ChangeType (Convert.ToInt16 (v, 16), typeof (T));
            else if (typeof (T).Equals (typeof (Int32)))
                return (T)Convert.ChangeType (Convert.ToInt32 (v, 16), typeof (T));
            else if (typeof (T).Equals (typeof (Int64)))
                return (T)Convert.ChangeType (Convert.ToInt64 (v, 16), typeof (T));
            else if (typeof (T).Equals (typeof (Single)))
                return (T)Convert.ChangeType (Convert.ToUInt32 (v, 16).ToIEE754Float(), typeof (T));
            else if (typeof (T).Equals (typeof (Double)))
                return (T)Convert.ChangeType (Convert.ToUInt32 (v, 16).ToIEE754Float(), typeof (T));
            else
                throw new InvalidCastException ("Only byte, U/Int16,32,64, single and double types are allowed.");
        }

        public static String NumberToHex<T> (this T v, String prefix=null, int size=0) where T : struct
        {
            if (size <= 0) {
                if (typeof (T).Equals (typeof (byte)))
                    size = 1;
                else if (typeof (T).Equals (typeof (UInt16)) || typeof (T).Equals (typeof (Int16)))
                    size = 2;
                else if (typeof (T).Equals (typeof (UInt32)) || typeof (T).Equals (typeof (Int32))
                || typeof (T).Equals (typeof (Single)))
                    size = 4;
                else if (typeof (T).Equals (typeof (UInt64)) || typeof (T).Equals (typeof (Int64))
                || typeof (T).Equals (typeof (Double)))
                    size = 8;
                else
                    throw new InvalidCastException ("Only byte, U/Int16,32,64, single and double types are allowed for autosize.");
            }

            return prefix + String.Format ("{0:x" + (size > 0 ? "" + size*2 : "") + "}", v);
        }

        public static String NormalizeForFilename(this String v){
			if (v.Equals (".") || v.Equals (".."))
				throw new Exception ("File name can't be . or ..");
			
			StringBuilder ret = new StringBuilder ();

			Regex re=new Regex(@"[^\\/\:\*\?""\<\>\|]");

			foreach(Match m in re.Matches(v))
				ret.Append (m.Value);

			return ret.ToString ();
		}

		public static T[] ReadUntil<T>(this T[] v, T endMark){
			return v.ReadUntil (endMark, 0);
		}

		public static T[] ReadUntil<T>(this T[] v, T endMark, int index){
			int i = Array.IndexOf<T>(v, endMark, index);
			if (i == -1)
				throw new Exception ("Can find end of mark starting in '" + index + "'.");
			return v.Extract(i, v.Length-i);
		}

		public static T[] Extract<T>(this T[] v, int index, long length){
			T[] ret=new T[length];
			Array.Copy (v, index, ret, 0, ret.Length);
			return ret;
		}

		public static String ToHexadecimal(this IList<byte> v){
			return v.ToHexadecimal (" ");
		}

		public static String ToHexadecimal(this IList<byte> v, String byteSeparator){
			StringBuilder ret = new StringBuilder ();
            if (v != null)
            {
                for (int i = 0; i < v.Count; i++)
                {
                    ret.AppendFormat("{0:X2}", v[i]);
                    ret.Append(byteSeparator);
                }
                if (ret.Length > 0)
                    ret.Remove(ret.Length - byteSeparator.Length, byteSeparator.Length);
            }

			return ret.ToString();
		}

		public static T[] Concat<T>(this T[] v, T[] o){
			T[] ret=new T[v.Length+o.Length];

			Array.Copy (v, 0, ret, 0, v.Length);
			Array.Copy (o, 0, ret, v.Length, o.Length);

			return ret;
		}

		public static void Initialize<T> (this IList<T> c, T value) {
			c.Initialize (0, c.Count, value);
		}

		public static void Initialize<T>(this IList<T> c, int start, int length, T value)
		{
			for (int i = start; i < start + length; i++)
				c [i] = value;
		}

		public static void Initialize<T> (this IList<T> c, Func<T> method) {
			c.Initialize (0, c.Count, method);
		}

		public static void Initialize<T> (this IList<T> c, int start, int length, Func<T> method)
		{
			for (int i = start; i < start + length; i++)
				c [i] = method.Invoke();
		}

		public static Nullable<T> Parse<T>(this String c) where T:struct{
			Nullable<T> ret;

			if (String.IsNullOrEmpty (c))
				ret = new Nullable<T> ();
			else
			{
				ret = new Nullable<T> ((T)Convert.ChangeType (c, typeof(T)));
			}

			return ret;
		}

		public static Nullable<T> Parse<T> (this String c, CultureInfo culture) where T : struct
		{
			Nullable<T> ret;

			CultureInfo aux = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = culture;
			ret = c.Parse<T> ();
			Thread.CurrentThread.CurrentCulture=aux;

			return ret;
		}

		public static bool TryParse<T>(this String c, out Nullable<T> value) where T:struct{
			bool ret = false;

			value = new Nullable<T> ();
			try{
				if (!String.IsNullOrEmpty (c)){
					value = (T)Convert.ChangeType (c, typeof(T));
				}
				ret = true;
			}
			catch{
				ret = false;
			}

			return ret;
		}

        public static String SQLScape(String sqlSentence){
            String ret = sqlSentence;
            ret=  ret.Replace("\0", "\\0");
            ret = ret.Replace("\b", "\\b");
            ret = ret.Replace("\n", "\\n");
            ret = ret.Replace("\r", "\\r");
            ret = ret.Replace("\t", "\\t");
            ret = ret.Replace(""+(Char)26, "\\Z");
            ret = ret.Replace("\\", "\\\\");
            ret = ret.Replace("'", "\\'");
            ret = ret.Replace("%", "\\%");
            ret = ret.Replace("_", "\\_");
            return ret;
        }

        public static String ToSQLValue(this Object c) {
            return c.ToSQLValue(-1);
        }

        public static String ToSQLValue(this Object c, int maxLength)
		{
            CultureInfo ci = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			String ret = null;

			if (c == null)
			{
				ret = "NULL";
			}
			else
			{
				Type type = Nullable.GetUnderlyingType(c.GetType());
				if (type == null){ // Si es nullable
					type = c.GetType();
				}

                if (type.Equals(typeof(DBNull)))
                    ret = "NULL";
                else if (type.Equals(typeof(Boolean)))
                    ret = ((bool)c ? 1 : 0).ToString();
                else if (type.Equals(typeof(String)))
                    ret = "'" + SQLScape((maxLength > -1 ? c.ToString().First(maxLength) : c.ToString())) + "'";
                else if (type.Equals(typeof(DateTime)))
                    ret = "'" + (((DateTime)c).ToString("yyyyMMddHHmmss")) + "'";
                else if (type.Equals(typeof(Object)))
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c));
                else if (typeof(IList<byte>).IsAssignableFrom(type))
                {
                    IList<byte> lc = (IList<byte>)c;
                    ret = "0x" + (maxLength>-1?lc.Take(maxLength):lc).ToList().ToHexadecimal("");
                }
                else
                    ret = c.ToString();
			}
            Thread.CurrentThread.CurrentCulture = ci;
			return ret;
		}

        public static IList<String> RegexMatches(this String v, String pattern) {
            return v.RegexMatches(pattern, RegexOptions.None);
        }

        public static IList<String> RegexMatches (this String v, String pattern, RegexOptions options)
        {
            return v.RegexMatches (new Regex (pattern, options));
        }

        public static IList<String> RegexMatches(this String v, Regex regex){
			List<String> ret = new List<String> ();

			foreach (Match m in regex.Matches(v)) {
				ret.Add (m.Value);
			}

			return ret;
		}

        public static String RegexMatch(this String v, String pattern){
            return v.RegexMatch(pattern, RegexOptions.None);
        }

        public static String RegexMatch(this String v, String pattern, RegexOptions options){
            return v.RegexMatch(new Regex(pattern, options));
		}

        public static String RegexMatch (this String v, Regex regex)
        {
            return v.RegexMatches(regex).FirstOrDefault ();
        }

        public static bool RegexIsMatch(this String v, String pattern){
            return v.RegexIsMatch(pattern, RegexOptions.None);
		}

        public static bool RegexIsMatch(this String v, String pattern, RegexOptions options){
            return Regex.IsMatch(v, pattern, options);
        }

        public static bool RegexIsMatch (this String v, Regex regex){
            return regex.IsMatch (v);
        }

        public static IList<String> RegexGroupsMatches(this String v, String pattern) {
            return v.RegexGroupsMatches(pattern, RegexOptions.None);
        }

        public static IList<String> RegexGroupsMatches(this String v, String pattern, RegexOptions options)
		{
            return v.RegexGroupsMatches(new Regex (pattern, options));
		}

        public static IList<String> RegexGroupsMatches (this String v, Regex regex)
        {
            List<String> ret = new List<String> ();

            Match m = regex.Match (v);
            foreach (Group g in m.Groups)
                ret.Add (g.Value);

            return ret;
        }

        public static IList<String> RegexGroupMatchCaptures(this String v, String pattern, int regexGroupIndex) {
            return v.RegexGroupMatchCaptures(pattern, regexGroupIndex, RegexOptions.None);
        }

        public static IList<String> RegexGroupMatchCaptures(this String v, String pattern, int regexGroupIndex, RegexOptions options)
		{
            return v.RegexGroupMatchCaptures(regexGroupIndex, new Regex (pattern, options));
		}

        public static IList<String> RegexGroupMatchCaptures (this String v, int regexGroupIndex, Regex regex)
        {
            List<String> ret = new List<String> ();

            Match m = regex.Match (v);
            if (m.Groups.Count - 1 < regexGroupIndex)
                throw new Exception ("No valid group index number.");
            foreach (Capture c in m.Groups [regexGroupIndex].Captures) {
                ret.Add (c.Value);
            }

            return ret;
        }

        public static String RegexReplace(this String v, String pattern, String replacement) {
            return v.RegexReplace(pattern, replacement, RegexOptions.None);
        }

        public static String RegexReplace(this String v, String pattern, String replacement, RegexOptions options)
        {
            return Regex.Replace(v, pattern, replacement, options) ;
        }

        public static String RegexReplace(this String v, String pattern, MatchEvaluator evaluator) {
            return v.RegexReplace(pattern, evaluator, RegexOptions.None);
        }

        public static String RegexReplace(this String v, String pattern, MatchEvaluator evaluator, RegexOptions options)
        {
            return Regex.Replace(v, pattern, evaluator, options);
        }
    }
}