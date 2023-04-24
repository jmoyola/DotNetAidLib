using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Collections
{
    public static class CollectionHelpers
    {
        public static int IndexOfSequence<T>(this IList<T> v, IList<T> listToFind)
        {
            Assert.NotNullOrEmpty(listToFind, nameof(listToFind));

            var vCount = v.Count;
            var listToFindCount = listToFind.Count;

            var ret = -1;
            var index = 0;

            while (ret == -1 && vCount - index > listToFindCount)
                if (v.EqualsAll(index, listToFind))
                    ret = index;
                else
                    index++;

            return ret;
        }

        public static void ForEach<T>(this IList<T> v, Action<T, int> action)
        {
            Assert.NotNull(action, nameof(action));

            for (var i = 0; i < v.Count; i++) action.Invoke(v[i], i);
        }

        public static bool EqualsAll<T>(this IList<T> v, IList<T> o)
        {
            return v.EqualsAll(0, o);
        }

        public static bool EqualsAll<T>(this IList<T> v, int index, IList<T> o)
        {
            Assert.NotNull(o, nameof(o));

            var ret = false;
            ret = v.Count - index == o.Count;
            if (ret)
                for (var i = index; i < v.Count; i++)
                {
                    if (v[i] != null)
                        ret = ret && v[i].Equals(o[i]);
                    else
                        ret = ret && Equals(v[i], o[i]);
                    if (!ret)
                        break;
                }

            return ret;
        }

        public static IEnumerable<T> NotIn<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            IList<T> ret = new List<T>();
            foreach (var aItem in a)
                if (!b.Any(bItem => bItem.Equals(aItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static IEnumerable<TA> NotIn<TA, TB>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TB, bool> match)
        {
            IList<TA> ret = new List<TA>();
            foreach (var aItem in a)
                if (!b.Any(bItem => match.Invoke(aItem, bItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static IEnumerable<T> In<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            IList<T> ret = new List<T>();
            foreach (var aItem in a)
                if (b.Any(bItem => bItem.Equals(aItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static IEnumerable<TA> In<TA, TB>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TB, bool> match)
        {
            IList<TA> ret = new List<TA>();
            foreach (var aItem in a)
                if (b.Any(bItem => match.Invoke(aItem, bItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static void AddAll<K, V>(this IDictionary<K, V> v, IDictionary<K, V> itemsToAdd)
        {
            v.AddAll(itemsToAdd, false);
        }

        public static void AddAll<K, V>(this IDictionary<K, V> v, IDictionary<K, V> itemsToAdd,
            bool excludeDuplicateKeys)
        {
            foreach (var key in itemsToAdd.Keys)
                if (!v.ContainsKey(key))
                    v.Add(key, itemsToAdd[key]);
                else if (!excludeDuplicateKeys)
                    throw new ArgumentException("An item with the same key has already been added.");
        }

        public static int IndexOf(this IList<string> v, string item, StringComparison comparison)
        {
            var ret = -1;
            for (var i = 0; i < v.Count; i++)
                if (v[i].Equals(item, comparison))
                {
                    ret = i;
                    break;
                }

            return ret;
        }

        public static int IndexOf<T>(this IList<T> v, T item)
        {
            var ret = -1;
            for (var i = 0; i < v.Count; i++)
                if (v[i].Equals(item))
                {
                    ret = i;
                    break;
                }

            return ret;
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> v, int start, int length)
        {
            return v.TakeWhile((e, i) => i >= start && i <= start + length);
        }

        public static string ToStringReflection<T>(this IEnumerable<T> instance)
        {
            return instance.ToList().Select(v => v.ToStringReflection(false))
                .ToStringJoin(Environment.NewLine + Environment.NewLine);
        }

        public static bool TryGetValueIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase, out V value)
        {
            var key = c.KeyIgnoreCase(keyIgnoreCase);
            if (key == null)
            {
                value = default;
                return false;
            }

            value = c[key];
            return true;
        }

        public static V GetValueIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase)
        {
            var key = c.KeyIgnoreCase(keyIgnoreCase);
            if (key == null)
                throw new Exception("Don't found Key '" + keyIgnoreCase + "' ignoring case.");

            return c[key];
        }

        public static void SetValueIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase, V value)
        {
            var key = c.KeyIgnoreCase(keyIgnoreCase);
            if (key == null)
                throw new Exception("Don't found Key '" + keyIgnoreCase + "' ignoring case.");

            c[key] = value;
        }

        public static void RemoveIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase)
        {
            var key = c.KeyIgnoreCase(keyIgnoreCase);
            if (key == null)
                throw new Exception("Don't found Key '" + keyIgnoreCase + "' ignoring case.");

            c.Remove(key);
        }

        public static string KeyIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase)
        {
            string key = null;
            key = c.Keys.FirstOrDefault(v => v.Equals(keyIgnoreCase, StringComparison.InvariantCultureIgnoreCase));
            return key;
        }

        public static bool ContainsKeyIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase)
        {
            string key = null;
            key = c.Keys.FirstOrDefault(v => v.Equals(keyIgnoreCase, StringComparison.InvariantCultureIgnoreCase));
            return key != null;
        }

        public static bool ContainsKeyTyped<T>(this IDictionary<string, object> c, string keyIgnoreCase)
        {
            string key = null;
            key = c.Keys.FirstOrDefault(v => v.Equals(keyIgnoreCase, StringComparison.InvariantCultureIgnoreCase));
            return key != null && typeof(T).IsAssignableFrom(c[key].GetType());
        }

        public static V GetValue<V>(this IDictionary<string, object> c, string key)
        {
            object ret = null;
            if (c.TryGetValue(key, out ret))
                return (V) ret;

            throw new Exception("Don't exists entry with key '" + key + "'");
        }

        public static void AddOrUpdate<K, V>(this IDictionary<K, V> c, K key, V value)
        {
            if (c.ContainsKey(key))
                c[key] = value;
            else
                c.Add(key, value);
        }

        public static V GetValue<V>(this IDictionary<string, object> c, string key, V defaultValueIfNotExists)
        {
            object ret = null;
            if (c.TryGetValue(key, out ret))
                return (V) ret;
            return defaultValueIfNotExists;
        }

        public static bool TryGetValue<V>(this IDictionary<string, object> c, string key, out V output)
        {
            output = default;
            var ret = false;
            object oret = null;
            ret = c.TryGetValue(key, out oret);
            if (ret)
                output = (V) oret;

            return ret;
        }

        public static void AddOrUpdateIgnoreCase<V>(this IDictionary<string, V> c, string key, V value)
        {
            var keyig = c.KeyIgnoreCase(key);
            if (!string.IsNullOrEmpty(keyig))
                c[keyig] = value;
            else
                c.Add(key, value);
        }

        public static V GetValueIfExists<V>(this IDictionary<string, V> c, string key, V defaultValueIfNotExists)
        {
            var ret = default(V);
            if (c.TryGetValue(key, out ret))
                return ret;
            return defaultValueIfNotExists;
        }

        public static V GetValueIfExistsIgnoreCase<V>(this IDictionary<string, V> c, string keyIgnoreCase,
            V defaultValueIfNotExists)
        {
            var ret = default(V);
            if (c.TryGetValueIgnoreCase(keyIgnoreCase, out ret))
                return ret;
            return defaultValueIfNotExists;
        }

        public static string ToQueryString<V>(this IDictionary<string, V> c)
        {
            var ret = (c.Count > 0 ? "?" : " ")
                      + c.Select(kv => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value.ToString()))
                          .ToStringJoin("&");

            return ret;
        }

        public static IDictionary<K, V> AddFluent<K, V>(this IDictionary<K, V> c, K key, V value)
        {
            c.Add(key, value);
            return c;
        }

        public static IDictionary<K, V> AddFluent<K, V>(this IDictionary<K, V> c, KeyValuePair<K, V> item)
        {
            c.Add(item);
            return c;
        }

        public static void LoadFromKeyValue(this IDictionary<string, string> v, string content,
            char keyValueSeparator = '=')
        {
            v.LoadFromKeyValue(content, Encoding.UTF8, keyValueSeparator);
        }

        public static void LoadFromKeyValue(this IDictionary<string, string> v, string content, Encoding encoding,
            char keyValueSeparator = '=')
        {
            try
            {
                Assert.NotNullOrEmpty(content, nameof(content));
                Assert.NotNull(encoding, nameof(encoding));

                using (var sr = new StreamReader(
                           new MemoryStream(encoding.GetBytes(content)), encoding))
                {
                    v.LoadFromKeyValue(sr, keyValueSeparator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading from key value string.", ex);
            }
        }

        public static void LoadFromKeyValue(this IDictionary<string, string> v, FileInfo file,
            char keyValueSeparator = '=')
        {
            try
            {
                Assert.Exists(file, nameof(file));

                using (var reader = file.OpenText())
                {
                    v.LoadFromKeyValue(reader, keyValueSeparator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading from key value file.", ex);
            }
        }

        public static void LoadFromKeyValue(this IDictionary<string, string> v, StreamReader reader,
            char keyValueSeparator = '=')
        {
            try
            {
                Assert.NotNull(reader, nameof(reader));

                var keyValueSeparatorPattern = Regex.Escape("" + keyValueSeparator);
                var kvRegex = new Regex(@"^\s*([^" + keyValueSeparatorPattern + @"\s]+)\s*" +
                                        keyValueSeparatorPattern + @"\s*(.+)\s*$");
                string linea = null;
                linea = reader.ReadLine();
                while (linea != null)
                {
                    if (!string.IsNullOrEmpty(linea))
                    {
                        var m = kvRegex.Match(linea);
                        if (m.Success) v.Add(m.Groups[1].Value, m.Groups[2].Value);
                    }

                    linea = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading from stream.", ex);
            }
        }

        public static void SaveToKeyValue<V>(this IDictionary<string, V> v, FileInfo file, char keyValueSeparator = '=')
        {
            try
            {
                Assert.Exists(file, nameof(file));

                if (file.Exists)
                    file.Delete();

                using (var writer = file.CreateText())
                {
                    v.SaveToKeyValue(writer, keyValueSeparator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving to file.", ex);
            }
        }

        public static void SaveToKeyValue<V>(this IDictionary<string, V> v, StreamWriter writer,
            char keyValueSeparator = '=')
        {
            try
            {
                foreach (var kv in v) writer.WriteLine(kv.Key + keyValueSeparator + kv.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving to stream.", ex);
            }
        }

        public static void SaveToKeyValue<V>(this IDictionary<string, V> v, out string outputString,
            char keyValueSeparator = '=')
        {
            v.SaveToKeyValue(out outputString, Encoding.UTF8);
        }

        public static void SaveToKeyValue<V>(this IDictionary<string, V> v, out string outputString, Encoding encoding,
            char keyValueSeparator = '=')
        {
            try
            {
                Assert.NotNull(encoding, nameof(encoding));

                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms, encoding))
                    {
                        v.SaveToKeyValue(sw);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    outputString = new string(encoding.GetChars(ms.ToArray()));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving to string.", ex);
            }
        }

        public static string ToStringJoin(this IEnumerable v, string separator = "")
        {
            return v.ToStringJoin(separator, o => Equals(o, null) ? null : o.ToString());
        }

        public static string ToStringJoin<T>(this IEnumerable<T> v, string separator = "")
        {
            return v.ToStringJoin(separator, o => Equals(o, null) ? null : o.ToString());
        }

        public static string ToStringJoin(this IEnumerable v, string separator, Func<object, string> toStringFunction)
        {
            var ret = new StringBuilder();

            foreach (var o in v)
            {
                ret.Append(toStringFunction.Invoke(o));
                ret.Append(separator);
            }

            if (ret.Length > 0)
                ret.Remove(ret.Length - separator.Length, separator.Length);

            return ret.ToString();
        }

        public static string ToStringJoin<K, V>(this IDictionary<K, V> v, string separator)
        {
            return v.ToStringJoin(separator, o => o.Key + "=>" + o.Value);
        }

        public static string ToStringJoin<K, V>(this IDictionary<K, V> v, string separator,
            Func<KeyValuePair<K, V>, string> select)
        {
            var ret = new StringBuilder();

            foreach (var o in v)
            {
                ret.Append(select.Invoke(o));
                ret.Append(separator);
            }

            if (ret.Length > 0)
                ret.Remove(ret.Length - separator.Length, separator.Length);

            return ret.ToString();
        }

        public static Dictionary<string, T> ToDictionary<T>(this IEnumerable<T> v)
        {
            var ret = new Dictionary<string, T>();
            var e = v.GetEnumerator();
            while (e.MoveNext())
            {
                var key = e.Current;
                if (key == null)
                    throw new NullReferenceException("A Key can't be null.");
                if (!e.MoveNext())
                    break;
                ret.Add(key.ToString(), e.Current);
            }

            return ret;
        }

        public static IDictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> v)
        {
            IDictionary<K, V> ret = new Dictionary<K, V>();
            foreach (var kv in v)
                ret.Add(kv);
            return ret;
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in col.AllKeys) dict.Add(k, col[k]);
            return dict;
        }

        public static bool All<T>(this IList<T> v, Func<T, int, bool> predicate)
        {
            var ret = true;
            for (var i = 0; i < v.Count; i++)
            {
                ret = ret && predicate.Invoke(v[i], i);
                if (!ret)
                    break;
            }

            return ret;
        }

        public static bool Any<T>(this IList<T> v, Func<T, int, bool> predicate)
        {
            var ret = false;
            for (var i = 0; i < v.Count; i++)
            {
                ret = ret || predicate.Invoke(v[i], i);
                if (ret)
                    break;
            }

            return ret;
        }

        public static T GetOrDefault<T>(this IList<T> v, int index)
        {
            var ret = default(T);

            if (index > -1 && index < v.Count)
                ret = v[index];

            return ret;
        }

        public static int IndexWhere<T>(this IList<T> v, Func<T, bool> where)
        {
            var ret = -1;
            for (var i = 0; i < v.Count; i++)
                if (where.Invoke(v[i]))
                {
                    ret = i;
                    break;
                }

            return ret;
        }

        public static bool TryMax<T>(this IEnumerable<T> v, out T value)
        {
            value = default;
            try
            {
                value = v.Max();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryMin<T>(this IEnumerable<T> v, out T value)
        {
            value = default;
            try
            {
                value = v.Min();
                return true;
            }
            catch
            {
                return false;
            }
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
    }
}