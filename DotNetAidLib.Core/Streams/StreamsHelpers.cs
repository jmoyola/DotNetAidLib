using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Proc;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace DotNetAidLib.Core.Streams{

    public static class StreamsHelpers
    {

        public static String ExtractString(this Stream stream, long streamIndex = -1, bool closeStream = false,
            Encoding encoding = null)
        {
            Assert.When(stream, v => v.CanSeek, "Stream must be seekable.", nameof(stream));
            StreamReader sr = null;
            try
            {
                stream.Flush();
                if (streamIndex > -1)
                    stream.Seek(streamIndex, SeekOrigin.Begin);
                if (encoding == null)
                    sr = new StreamReader(stream);
                else
                    sr = new StreamReader(stream, encoding);
                String ret = sr.ReadToEnd();
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error extracting string from stream.", ex);
            }
            finally
            {
                if (closeStream)
                    stream.Close();
            }

        }

        public static void AppendString(this Stream stream, String value, Encoding encoding = null)
        {
            StreamWriter sw = null;
            try
            {
                if (encoding == null)
                    sw = new StreamWriter(stream);
                else
                    sw = new StreamWriter(stream, encoding);

                sw.Write(value);
                sw.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("Error appening string to stream.", ex);
            }
        }


        public delegate int ReadBytesTransferHandler(byte[] buffer, int offset, int length);

        public delegate void WriteBytesTransferHandler(byte[] buffer, int offset, int length);

        public static StreamReader ToReader(this String v)
        {
            var ms = new MemoryStream();
            ms.Seek(0, SeekOrigin.Begin);
            return new StreamReader(ms);
        }



        public static StreamReader ToStreamReader(this String v)
        {
            return v.ToStreamReader(Encoding.Unicode);
        }

        public static StreamReader ToStreamReader(this String v, Encoding encoding)
        {
            MemoryStream ms = new MemoryStream(encoding.GetBytes(v));
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);

            return sr;
        }

        public static IList<Object> ReadRecord(this StreamReader v)
        {
            return v.ReadRecord(RecordsParserOptions.DefaultOptions);
        }

        public static IList<Object> ReadRecord(this StreamReader v, RecordsParserOptions parseOptions)
        {
            Assert.NotNull( parseOptions, nameof(parseOptions));

            String cvsPattern =
                @"(?:[,]""|^""|[,] "")(""""|[\w\W]*?)(?=""[,]|""$)|(?:[,](?!"")|^(?!""))([^,]*?)(?=$|[,])|(\r\n|\n)";
            cvsPattern = cvsPattern.Replace(",",
                String.Join("", parseOptions.FieldsSeparator.Select(f => Regex.Escape("" + f))));

            Regex cvsRegex = new Regex(cvsPattern);
            IList<Object> ret = null;

            String line = null;
            //line = v.ReadLine();
            line = v.ReadLine(parseOptions.RecordsSeparator);
            if (!String.IsNullOrEmpty(line) && cvsRegex.IsMatch(line))
            {
                ret = new List<Object>();
                foreach (Match m in cvsRegex.Matches(line))
                {
                    Object columnValue = null;
                    if (!String.IsNullOrEmpty(m.Groups[1].Value)) // String with quotes
                        columnValue = m.Groups[1].Value.Replace("\"\"", "\"");
                    else if (!String.IsNullOrEmpty(m.Groups[2].Value))
                    {
                        // Other values without quotes
                        if (parseOptions.TryParseValues)
                        {
                            DateTime dt;
                            decimal d;
                            if (Decimal.TryParse(
                                    m.Groups[2].Value.Replace(parseOptions.NumberFormat.NumberGroupSeparator, ""),
                                    NumberStyles.Any, parseOptions.NumberFormat, out d)) // Decimal value
                                columnValue = d;
                            else if (DateTime.TryParse(m.Groups[2].Value, parseOptions.DateTimeFormat,
                                         DateTimeStyles.AssumeUniversal, out dt)) // DateTime value
                                columnValue = dt;
                            else // String value
                                columnValue = m.Groups[2].Value;
                        }
                        else
                            columnValue = m.Groups[2].Value;
                    }

                    // null parser
                    if (parseOptions.NullValue.Equals(columnValue))
                        ret = null;

                    ret.Add(columnValue);
                }
            }

            return ret;
        }

        public static RecordsList ReadAllRecords(this StreamReader v, bool closeStream = false)
        {
            return v.ReadAllRecords(RecordsParserOptions.DefaultOptions, closeStream);
        }

        public static RecordsList ReadAllRecords(this StreamReader v, RecordsParserOptions parseOptions,
            bool closeStream = false)
        {
            Assert.NotNull( parseOptions, nameof(parseOptions));
            parseOptions.Validate();

            RecordsList ret = null;

            // Si incluimos la fila de cabecera
            if (parseOptions.IncludeHeaderRow)
                ret = new RecordsList(v.ReadRecord(parseOptions).Select(c => c.ToString()).ToList());
            else
                ret = new RecordsList();

            IList<Object> record = v.ReadRecord(parseOptions);
            while (record != null)
            {
                ret.Add(record);
                record = v.ReadRecord(parseOptions);
            }

            if (closeStream)
                v.Close();

            return ret;
        }

        public static void WriteRecord(this StreamWriter v, IList<Object> record)
        {
            v.WriteRecord(record, RecordsParserOptions.DefaultOptions);
        }

        public static void WriteRecord(this StreamWriter v, IList<Object> record, RecordsParserOptions parseOptions)
        {
            Assert.NotNull( parseOptions, nameof(parseOptions));

            for (int i = 0; i < record.Count; i++)
            {
                if (i > 0)
                    v.Write(parseOptions.FieldsSeparator);

                if (record[i] == null)
                    v.Write(parseOptions.NullValue);
                else if (record[i] is DateTime)
                    v.Write(((DateTime) record[i]).ToString(
                        parseOptions.DateTimeFormat.UniversalSortableDateTimePattern));
                else if (record[i].GetType().IsNumber())
                    v.Write(((Decimal) record[i]).ToString(parseOptions.NumberFormat));
                else
                {
                    if (parseOptions.AllStringsDoubleQuoted ||
                        parseOptions.FieldsSeparator.Any(fs => record[i].ToString().Contains(fs)))
                        v.Write("\"" + record[i].ToString().Replace("\"", "\"\"") + "\"");
                    else
                        v.Write(record[i].ToString());
                }
            }
        }

        public static void WriteAllRecords(this StreamWriter v, RecordsList records, bool closeStream = false)
        {
            v.WriteAllRecords(records, RecordsParserOptions.DefaultOptions, closeStream);
        }

        public static void WriteAllRecords(this StreamWriter v, RecordsList records, RecordsParserOptions parseOptions,
            bool closeStream = false)
        {
            Assert.NotNull( parseOptions, nameof(parseOptions));
            parseOptions.Validate();

            // Escribimos el encabezado
            if (parseOptions.IncludeHeaderRow)
            {
                v.WriteRecord(records.FieldNames.Select(c => (Object) c).ToList(), parseOptions);
                v.Write(parseOptions.RecordsSeparator);
            }

            for (int i = 0; i < records.Count; i++)
            {
                if (i > 0)
                    v.Write(parseOptions.RecordsSeparator);
                v.WriteRecord(records[i], parseOptions);
            }

            if (closeStream)
                v.Close();
        }

        public static String ReadLine(this StreamReader v, int timeoutMs)
        {
            TimeOutWatchDog to = new TimeOutWatchDog(timeoutMs);
            String ret = null;

            Task<String> rl = v.ReadLineAsync();
            while (!rl.IsCompleted)
            {
                if (to.IsTimeOut(false, false))
                    break;
            }

            if (rl.IsCompleted)
                ret = rl.Result;
            else
                rl.Dispose();

            return ret;
        }

        public static StreamWriter WriteFluent(this StreamWriter v, String text)
        {
            return v.WriteFluent(text, false);
        }

        public static StreamWriter WriteFluent(this StreamWriter v, String text, bool closeStream)
        {
            v.Write(text);
            if (closeStream)
                v.Close();
            return v;
        }

        public static StreamWriter WriteLineFluent(this StreamWriter v, String text)
        {
            return v.WriteLineFluent(text, false);
        }

        public static StreamWriter WriteLineFluent(this StreamWriter v, String text, bool closeStream)
        {
            return v.WriteFluent(text + Environment.NewLine, closeStream);
        }


        public static bool TryExpect(this StreamReader v, String expect, int timeOutMs)
        {
            try
            {
                v.Expect(expect, timeOutMs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Expect(this StreamReader v, String expect, int timeOutMs)
        {
            char[] cExpect = expect.ToCharArray();
            int i = 0;
            TimeOutWatchDog towd = new TimeOutWatchDog(timeOutMs);
            do
            {
                char c = (char) v.Read();
                if (c == cExpect[i])
                    i++;
                else
                    i = 0;

                if (i == cExpect.Length)
                    break;
                towd.IsTimeOut(true);
            } while (true);
        }

        public static bool TryExpect(this Stream v, byte[] expect, int timeOutMs)
        {
            try
            {
                v.Expect(expect, timeOutMs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Expect(this Stream v, byte[] expect, int timeOutMs)
        {
            int i = 0;
            TimeOutWatchDog towd = new TimeOutWatchDog(timeOutMs);
            do
            {
                byte c = (byte) v.ReadByte();
                if (c == expect[i])
                    i++;
                else
                    i = 0;

                if (i == expect.Length)
                    break;
                towd.IsTimeOut(true);
            } while (true);
        }

        public static IList<String> Lines(this StreamReader v, bool closeStream)
        {
            List<String> ret = new List<String>();

            String line = v.ReadLine();
            while (line != null)
            {
                ret.Add(line);
                line = v.ReadLine();
            }

            if (closeStream)
                v.Close();

            return ret;
        }

        public static void Lines(this StreamReader v, Action<String> lineAction, bool closeStream)
        {
            String line = v.ReadLine();
            while (line != null)
            {
                lineAction.Invoke(line);
                line = v.ReadLine();
            }

            if (closeStream)
                v.Close();

        }

        public static void Lines(this StreamReader v, Func<String, bool> lineAction, bool closeStream)
        {
            String line = v.ReadLine();
            while (line != null)
            {
                bool cancel = lineAction.Invoke(line);
                if (cancel)
                    break;
                line = v.ReadLine();
            }

            if (closeStream)
                v.Close();
        }

        public static Match LineSearch(this StreamReader v, Regex lineRegex, bool closeStream)
        {
            Match ret = null;

            String line = v.ReadLine();
            while (line != null)
            {
                ret = lineRegex.Match(line);
                if (ret.Success)
                    break;

                line = v.ReadLine();
            }

            if (closeStream)
                v.Close();

            return ret;
        }

        public static void LineSearch(this StreamReader v, Regex lineRegex, Func<MatchCollection, bool> matches,
            bool closeStream)
        {
            MatchCollection ret = null;

            String line = v.ReadLine();
            while (line != null)
            {
                ret = lineRegex.Matches(line);
                if (ret.Count > 0)
                {
                    bool cancel = matches.Invoke(ret);
                    if (cancel)
                        break;
                }

                line = v.ReadLine();
            }

            if (closeStream)
                v.Close();
        }

        public static void Discard(this Stream v, int length)
        {
            byte[] buffer = new byte [1024];

            while (length > 0)
                if (length > buffer.Length)
                    length -= v.Read(buffer, 0, buffer.Length);
                else
                    length -= v.Read(buffer, 0, length);
        }

        public static int Read(this Stream v, Stream toStream, int length)
        {
            byte[] buffer = new byte[length];
            int n = v.Read(buffer, 0, length);
            toStream.Write(buffer, 0, n);
            return n;
        }

        public static int ReadAll(this Stream v, Stream toStream)
        {
            return v.ReadAll(toStream.Write, 1024);
        }

        public static int ReadAll(this Stream v, Stream toStream, int bufferLength)
        {
            return v.ReadAll(toStream.Write, bufferLength);
        }

        public static int ReadAll(this Stream v, WriteBytesTransferHandler toBytes, int bufferLength)
        {
            byte[] buffer = new byte [bufferLength];
            int ret = 0;

            int n = v.Read(buffer, 0, buffer.Length);
            while (n > 0)
            {
                ret += n;
                toBytes.Invoke(buffer, 0, n);
                n = v.Read(buffer, 0, buffer.Length);
            }

            return ret;
        }


        public static byte[] Read(this Stream v, int length)
        {
            MemoryStream ret = new MemoryStream();
            v.Read(ret, length);
            return ret.ToArray();
        }

        public static byte[] ReadAll(this Stream v)
        {
            return v.ReadAll(false);
        }

        public static byte[] ReadAll(this Stream v, bool closeStream)
        {
            MemoryStream ret = new MemoryStream();
            v.ReadAll(ret);
            if (closeStream)
                v.Close();
            return ret.ToArray();
        }

        public static int Write(this Stream v, Stream fromStream, int length)
        {
            byte[] buffer = new byte[length];
            int n = fromStream.Read(buffer, 0, length);
            v.Write(buffer, 0, n);
            return n;
        }

        public static int WriteAll(this Stream v, Stream fromStream)
        {
            return v.WriteAll(fromStream.Read, 1024);
        }

        public static int WriteAll(this Stream v, Stream fromStream, int bufferLength)
        {
            return v.WriteAll(fromStream.Read, bufferLength);
        }

        public static int WriteAll(this Stream v, ReadBytesTransferHandler fromBytes, int bufferLength)
        {
            byte[] buffer = new byte [bufferLength];
            int ret = 0;

            int n = fromBytes.Invoke(buffer, 0, buffer.Length);
            while (n > 0)
            {
                ret += n;
                v.Write(buffer, 0, n);
                n = fromBytes.Invoke(buffer, 0, buffer.Length);
            }

            return ret;
        }


        public static int Write(this Stream v, byte[] fromArray, int length)
        {
            MemoryStream ret = new MemoryStream(fromArray);
            return v.Write(ret, length);
        }

        public static int WriteAll(this Stream v, byte[] fromArray)
        {
            MemoryStream ret = new MemoryStream(fromArray);
            return v.WriteAll(ret);
        }

        public static byte[] ReadUntil(this Stream v, byte finalMark, int timeOutMs)
        {
            const int bufferSize = 1024;

            TimeOutWatchDog twd = null;

            if (timeOutMs > 0)
                twd = new TimeOutWatchDog(timeOutMs);

            byte[] buffer = new byte[bufferSize];

            MemoryStream ms = new MemoryStream();

            int n = 0;
            while (true)
            {

                if (twd != null)
                    twd.IsTimeOut(true, false);

                n = v.Read(buffer, 0, bufferSize);

                if (n > 0)
                {
                    if (buffer[n - 1] == finalMark)
                    {
                        ms.Write(buffer, 0, n - 1);
                        break;
                    }
                    else
                        ms.Write(buffer, 0, n);
                }
            }

            return ms.ToArray();
        }

        public static String ReadUntil(this StreamReader v, String terminator)
        {
            return v.ReadUntil(terminator, 5000);
        }

        public static String ReadUntil(this StreamReader v, String terminator, int timeOutMs)
        {
            StringBuilder sb = new StringBuilder();
            TimeOutWatchDog twd = new TimeOutWatchDog(timeOutMs);
            int ic;

            while (true)
            {
                twd.IsTimeOut(true, false);
                Thread.Sleep(1);

                ic = v.Read();

                if (ic > -1)
                    sb.Append((char) ic);

                if (sb.ToString().EndsWith(terminator, StringComparison.InvariantCulture))
                    break;
            }

            return sb.Replace(terminator, "").ToString();
        }

        public static String ReadLine(this StreamReader v, String newLineTerminator)
        {
            StringBuilder sb = new StringBuilder();
            int ic;

            while (!v.EndOfStream)
            {
                ic = v.Read();

                if (ic > -1)
                    sb.Append((char) ic);

                if (sb.ToString().EndsWith(newLineTerminator, StringComparison.InvariantCulture))
                    break;
            }

            return sb.Replace(newLineTerminator, "").ToString();
        }

        public static String ReadUntilNull(this StreamReader v)
        {
            return v.ReadUntilNull(5000);
        }

        public static String ReadUntilNull(this StreamReader v, int timeOutMs)
        {
            StringBuilder sb = new StringBuilder();
            TimeOutWatchDog twd = new TimeOutWatchDog(timeOutMs);
            int ic;

            while (true)
            {
                twd.IsTimeOut(true, false);
                Thread.Sleep(1);
                ic = v.Read();

                if (ic == -1)
                {
                    break;
                }
                else
                    sb.Append((char) ic);

            }

            return sb.ToString();
        }

        public static String ReadToEnd(this StreamReader v, bool closeStream)
        {
            String ret = v.ReadToEnd();
            if (closeStream)
                v.Close();
            return ret;
        }

        public static IList<String> Grep(this StreamReader v, String pattern)
        {
            return v.Grep(pattern, RegexOptions.None, false);
        }

        public static IList<String> Grep(this StreamReader v, String pattern, bool closeStream)
        {
            return v.Grep(pattern, RegexOptions.None, closeStream);
        }

        public static IList<String> Grep(this StreamReader v, String pattern, RegexOptions regexOptions,
            bool closeStream)
        {
            Regex regex = new Regex(pattern, regexOptions);
            IList<String> ret = new List<String>();

            String line = v.ReadLine();
            while (line != null)
            {
                if (regex.IsMatch(line))
                    ret.Add(line);
                line = v.ReadLine();
            }

            if (closeStream)
                v.Close();

            return ret;
        }
    }
}