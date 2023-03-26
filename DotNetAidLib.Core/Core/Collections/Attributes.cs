using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class Attributes : IEnumerable<KeyValuePair<string, object>>
    {
        public Attributes()
            : this(new Dictionary<string, object>())
        {
        }

        public Attributes(IDictionary<string, object> baseDictionary)
        {
            Assert.NotNull(baseDictionary, nameof(baseDictionary));
            BaseDictionary = baseDictionary;
        }

        public IDictionary<string, object> BaseDictionary { get; }

        public object this[string key]
        {
            get => BaseDictionary.ContainsKey(key) ? BaseDictionary[key] : null;
            set
            {
                if (BaseDictionary.ContainsKey(key))
                    BaseDictionary[key] = value;
                else
                    BaseDictionary.Add(key, value);
            }
        }

        public IEnumerable<string> Keys => BaseDictionary.Keys;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return BaseDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return BaseDictionary.GetEnumerator();
        }

        public T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        public T Get<T>(string key, T defaultValueIfNotExists)
        {
            if (!BaseDictionary.ContainsKey(key))
                return defaultValueIfNotExists;
            return (T) BaseDictionary[key];
        }

        public void Set<T>(string key, T value)
        {
            if (!BaseDictionary.ContainsKey(key))
                BaseDictionary.Add(key, value);
            else
                BaseDictionary[key] = value;
        }

        public void UnSet(string key)
        {
            if (BaseDictionary.ContainsKey(key))
                BaseDictionary.Remove(key);
        }

        public bool ContainsKey(string key)
        {
            return BaseDictionary.ContainsKey(key);
        }

        public override string ToString()
        {
            return BaseDictionary.Select(kv => kv.Key + "=>" + (kv.Value == null ? "<null>" : kv.ToString()))
                .ToStringJoin(", ");
        }
    }
}