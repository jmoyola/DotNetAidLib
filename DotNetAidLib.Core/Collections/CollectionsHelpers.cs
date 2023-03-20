using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
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

            int vCount = v.Count;
            int listToFindCount = listToFind.Count;
         
            int ret = -1;
            int index = 0;

            while (ret==-1 && (vCount - index) > listToFindCount)
            {
                if (v.EqualsAll(index, listToFind))
                    ret = index;
                else
                    index++;
            }

            return ret;
        }
        
        public static void ForEach<T> (this IList<T> v, Action<T, int> action)
        {
            Assert.NotNull(action, nameof(action));

            for(int i = 0; i < v.Count; i++) {
                action.Invoke(v[i], i);
            }
        }

        public static bool EqualsAll<T>(this IList<T> v, IList<T> o)
        {
            return v.EqualsAll(0, o);
        }

        public static bool EqualsAll<T>(this IList<T> v, int index, IList<T> o)
        {
            Assert.NotNull(o, nameof(o));

            Boolean ret = false;
            ret = (v.Count-index) == o.Count;
            if (ret)
            {
                for (int i = index; i < v.Count; i++)
                {
                    if(v[i]!=null)
                        ret = ret && v[i].Equals(o[i]);
                    else
                        ret = ret && Object.Equals(v[i], o[i]);
                    if (!ret)
                        break;
                }
            }
            return ret;
        }

        public static IEnumerable<T> NotIn<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            IList<T> ret = new List<T>();
            foreach (T aItem in a)
                if (!b.Any(bItem => bItem.Equals(aItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static IEnumerable<TA> NotIn<TA,TB>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA,TB, bool> match)
        {
            IList<TA> ret = new List<TA>();
            foreach (TA aItem in a)
                if (!b.Any(bItem => match.Invoke(aItem, bItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static IEnumerable<T> In<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            IList<T> ret = new List<T>();
            foreach (T aItem in a)
                if (b.Any(bItem => bItem.Equals(aItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static IEnumerable<TA> In<TA, TB>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TB, bool> match)
        {
            IList<TA> ret = new List<TA>();
            foreach (TA aItem in a)
                if (b.Any(bItem => match.Invoke(aItem, bItem)))
                    ret.Add(aItem);

            return ret;
        }

        public static void AddAll<K, V>(this IDictionary<K, V> v, IDictionary<K, V> itemsToAdd) {
            v.AddAll(itemsToAdd, false);
        }

        public static void AddAll<K,V>(this IDictionary<K,V> v, IDictionary<K, V> itemsToAdd, bool excludeDuplicateKeys)
        {
            foreach (K key in itemsToAdd.Keys)
            {
                if(!v.ContainsKey(key))
                    v.Add(key, itemsToAdd[key]);
                else if (!excludeDuplicateKeys)
                    throw new ArgumentException("An item with the same key has already been added.");
            }
        }

        public static int IndexOf(this IList<String> v, String item, StringComparison comparison)
        {
            int ret = -1;
            for (int i = 0; i < v.Count; i++)
            {
                if (v[i].Equals(item, comparison))
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        public static int IndexOf<T>(this IList<T> v, T item)
        {
            int ret = -1;
            for (int i = 0; i < v.Count; i++)
            {
                if (v[i].Equals(item))
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> v, int start, int length)
        {
            return v.TakeWhile((e, i) => i >= start && i <= (start + length));
        }

        public static string ToStringReflection<T>(this IEnumerable<T> instance)
        {
            return instance.ToList().Select(v => v.ToStringReflection(false)).ToStringJoin(Environment.NewLine + Environment.NewLine);
        }

        public static bool TryGetValueIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase, out V value)
        {
            String key = c.KeyIgnoreCase<V>(keyIgnoreCase);
            if (key == null)
            {
                value = default(V);
                return false;
            }
            else
            {
                value = c[key];
                return true;
            }
        }

        public static V GetValueIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase)
        {
            String key = c.KeyIgnoreCase<V>(keyIgnoreCase);
            if (key == null)
                throw new Exception("Don't found Key '" + keyIgnoreCase + "' ignoring case.");

            return c[key];
        }

        public static void SetValueIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase, V value)
        {
            String key = c.KeyIgnoreCase<V>(keyIgnoreCase);
            if (key == null)
                throw new Exception("Don't found Key '" + keyIgnoreCase + "' ignoring case.");

            c[key] = value;
        }

        public static void RemoveIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase)
        {
            String key = c.KeyIgnoreCase<V>(keyIgnoreCase);
            if (key == null)
                throw new Exception("Don't found Key '" + keyIgnoreCase + "' ignoring case.");

            c.Remove(key);
        }

        public static String KeyIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase)
        {
            String key = null;
            key = c.Keys.FirstOrDefault(v => v.Equals(keyIgnoreCase, StringComparison.InvariantCultureIgnoreCase));
            return key;
        }

        public static bool ContainsKeyIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase)
        {
            String key = null;
            key = c.Keys.FirstOrDefault(v => v.Equals(keyIgnoreCase, StringComparison.InvariantCultureIgnoreCase));
            return key != null;
        }

        public static bool ContainsKeyTyped<T>(this IDictionary<String, Object> c, String keyIgnoreCase)
        {
            String key = null;
            key = c.Keys.FirstOrDefault(v => v.Equals(keyIgnoreCase, StringComparison.InvariantCultureIgnoreCase));
            return (key != null) && typeof(T).IsAssignableFrom(c[key].GetType());
        }

        public static V GetValue<V>(this IDictionary<String, Object> c, String key)
        {
            Object ret = null;
            if (c.TryGetValue(key, out ret))
                return (V)ret;

            throw new Exception("Don't exists entry with key '" + key + "'");
        }
        
        public static void AddOrUpdate<K,V>(this IDictionary<K, V> c, K key, V value)
        {
            if (c.ContainsKey (key))
                c [key] = value;
            else
                c.Add (key, value);
        }
        
        public static V GetValue<V>(this IDictionary<String, Object> c, String key, V defaultValueIfNotExists)
        {
            Object ret = null;
            if (c.TryGetValue(key, out ret))
                return (V) ret;
            else
                return defaultValueIfNotExists;

        }

        public static bool TryGetValue<V>(this IDictionary<String, Object> c, String key, out V output)
        {
            output = default(V);
            bool ret = false;
            Object oret = null;
            ret = c.TryGetValue(key, out oret);
            if(ret)
                output =(V)oret;

            return ret;
        }

        public static void AddOrUpdateIgnoreCase<V> (this IDictionary<String, V> c, String key, V value)
        {
            String keyig = c.KeyIgnoreCase<V> (key);
            if (!String.IsNullOrEmpty(keyig))
                c [keyig] = value;
            else
                c.Add (key, value);
        }

        public static V GetValueIfExists<V>(this IDictionary<String, V> c, String key, V defaultValueIfNotExists)
        {
			V ret = default(V);
            if (c.TryGetValue(key, out ret))
                return ret;
            else
                return defaultValueIfNotExists;
		}

		public static V GetValueIfExistsIgnoreCase<V>(this IDictionary<String, V> c, String keyIgnoreCase, V defaultValueIfNotExists)
        {
			V ret=default(V);
			if (c.TryGetValueIgnoreCase(keyIgnoreCase, out ret))
                return ret;
            else
                return defaultValueIfNotExists;
        }

        public static String ToQueryString<V>(this IDictionary<String, V> c)
        {
            String ret = (c.Count > 0 ? "?" : " ")
                + c.Select(kv=> Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString (kv.Value.ToString()))
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

        public static void LoadFromKeyValue(this IDictionary<String,String> v, String content, char keyValueSeparator='=')
        {
            v.LoadFromKeyValue(content, Encoding.UTF8, keyValueSeparator);
        }

        public static void LoadFromKeyValue(this IDictionary<String,String> v, String content, Encoding encoding, char keyValueSeparator='=')
        {
            try
            {
                Assert.NotNullOrEmpty(content, nameof(content));
                Assert.NotNull(encoding, nameof(encoding));

                using(StreamReader sr = new StreamReader(
                          new MemoryStream(encoding.GetBytes(content)), encoding)){
                    v.LoadFromKeyValue(sr, keyValueSeparator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading from key value string.", ex);
            }
        }

        public static void LoadFromKeyValue(this IDictionary<String,String> v, FileInfo file, char keyValueSeparator='=')
        {
            try
            {
                Assert.Exists(file, nameof(file));

                using(StreamReader reader = file.OpenText()){
                    v.LoadFromKeyValue(reader, keyValueSeparator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading from key value file.", ex);
            }
        }

        public static void LoadFromKeyValue(this IDictionary<String, String> v, StreamReader reader, char keyValueSeparator='=')
        {
            try
            {
                Assert.NotNull(reader, nameof(reader));

                String keyValueSeparatorPattern = Regex.Escape("" + keyValueSeparator);
                Regex kvRegex = new Regex(@"^\s*([^" + keyValueSeparatorPattern + @"\s]+)\s*" +
                                          keyValueSeparatorPattern + @"\s*(.+)\s*$");
                String linea = null;
                linea = reader.ReadLine();
                while (linea != null)
                {
                    if (!String.IsNullOrEmpty(linea))
                    {
                        Match m = kvRegex.Match(linea);
                        if (m.Success)
                        {
                            v.Add(m.Groups[1].Value, m.Groups[2].Value);
                        }
                    }
                    linea = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading from stream.", ex);
            }
        }

        public static void SaveToKeyValue<V>(this IDictionary<String, V> v, FileInfo file, char keyValueSeparator='=')
        {
            try
            {
                Assert.Exists(file, nameof(file));
                
                if (file.Exists)
                    file.Delete();

                using(StreamWriter writer = file.CreateText()){
                    v.SaveToKeyValue(writer, keyValueSeparator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving to file.", ex);
            }
        }

        public static void SaveToKeyValue<V>(this IDictionary<String, V> v, StreamWriter writer, char keyValueSeparator='=')
        {
            try
            {
                foreach (KeyValuePair<String, V> kv in v)
                {
                    writer.WriteLine(kv.Key + keyValueSeparator + kv.Value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving to stream.", ex);
            }
        }

        public static void SaveToKeyValue<V>(this IDictionary<String, V> v, out String outputString, char keyValueSeparator='=')
        {
            v.SaveToKeyValue(out outputString, Encoding.UTF8);
        }

        public static void SaveToKeyValue<V>(this IDictionary<String, V> v, out String outputString, Encoding encoding, char keyValueSeparator='=')
        {
            try
            {
                Assert.NotNull(encoding, nameof(encoding));
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(ms, encoding))
                        v.SaveToKeyValue(sw);
                    ms.Seek(0, SeekOrigin.Begin);
                    outputString = new string(encoding.GetChars(ms.ToArray()));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving to string.", ex);
            }
        }

        public static String ToStringJoin(this IEnumerable v, String separator="")
        {
            return v.ToStringJoin(separator, o => (Object.Equals(o, null) ? null : o.ToString()));
        }

        public static String ToStringJoin<T>(this IEnumerable<T> v, String separator="")
        {
            return v.ToStringJoin(separator, o => (Object.Equals(o, null) ? null : o.ToString()));
        }
        public static String ToStringJoin(this IEnumerable v, String separator, Func<Object, String> toStringFunction)
        {
            StringBuilder ret = new StringBuilder();

            foreach (Object o in v)
            {
                ret.Append(toStringFunction.Invoke(o));
                ret.Append(separator);
            }

            if (ret.Length > 0)
                ret.Remove(ret.Length - separator.Length, separator.Length);

            return ret.ToString();
        }

        public static String ToStringJoin<K, V>(this IDictionary<K, V> v, String separator)
        {
            return v.ToStringJoin(separator, (o) => o.Key.ToString() + "=>" + o.Value.ToString());
        }

        public static String ToStringJoin<K, V>(this IDictionary<K, V> v, String separator, Func<KeyValuePair<K, V>, String> select)
        {
            StringBuilder ret = new StringBuilder();

            foreach (KeyValuePair<K, V> o in v)
            {
                ret.Append(select.Invoke(o));
                ret.Append(separator);
            }

            if (ret.Length > 0)
                ret.Remove(ret.Length - separator.Length, separator.Length);

            return ret.ToString();
        }

        public static Dictionary<String, T> ToDictionary<T>(this IEnumerable<T> v)
        {
            Dictionary<String, T> ret = new Dictionary<String, T>();
            IEnumerator<T> e = v.GetEnumerator();
            while (e.MoveNext())
            {
                T key = e.Current;
                if (key == null)
                    throw new NullReferenceException("A Key can't be null.");
                if (!e.MoveNext())
                    break;
                ret.Add(key.ToString(), e.Current);
            }

            return ret;
        }

        public static IDictionary<K, V> ToDictionary<K, V> (this IEnumerable<KeyValuePair<K, V>> v)
        {
            IDictionary<K, V> ret = new Dictionary<K, V> ();
            foreach (KeyValuePair<K, V> kv in v)
                ret.Add (kv);
            return ret;
        }

        
    }
}