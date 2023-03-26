using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Configuration.Dictionary.Core
{
    public abstract class DictionaryConfig : IDictionary<string, object>
    {
        public const string DEFAULT_INSTANCE_ID = "_appconfig";

        private static readonly Dictionary<string, DictionaryConfig> instances =
            new Dictionary<string, DictionaryConfig>();

        public abstract object this[string key] { get; set; }

        public abstract ICollection<string> Keys { get; }
        public abstract ICollection<object> Values { get; }
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }

        public abstract void Add(string key, object value);

        public abstract void Clear();

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ContainsKey(item.Key);
        }

        public abstract bool ContainsKey(string key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            try
            {
                var items = GetItems();
                for (var i = 0; i < items.Count; i++)
                    array[arrayIndex + i] = items[i];
            }
            catch (Exception ex)
            {
                throw new DictionaryConfigException("Error copy to.", ex);
            }
        }

        public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator();
        public abstract bool Remove(string key);
        public abstract bool Remove(KeyValuePair<string, object> item);

        public abstract bool TryGetValue(string key, out object value);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetItems().GetEnumerator();
        }

        public void Set<T>(string key, T value)
        {
            if (!ContainsKey(key))
                Add(key, value);
            else
                this[key] = value;
        }

        public T Get<T>(string key, T defaultValueIfNotExists)
        {
            if (!ContainsKey(key))
                return defaultValueIfNotExists;
            return (T) this[key];
        }

        public T Get<T>(string key)
        {
            if (!ContainsKey(key))
                throw new DictionaryConfigException("Key '" + key + "' don't exists.");

            return (T) this[key];
        }

        public bool IsSet(string key)
        {
            return ContainsKey(key);
        }

        public bool UnSet(string key)
        {
            return Remove(key);
        }

        public IList<string> GetKeyMatches(string pattern)
        {
            return Keys.Where(v => Willcards.SQLMatch(pattern, v)).ToList();
        }

        public abstract IList<KeyValuePair<string, object>> GetValueMatches(string pattern);

        protected abstract IList<KeyValuePair<string, object>> GetItems();

        public static DictionaryConfig Instance(string instanceId = DEFAULT_INSTANCE_ID)
        {
            if (!instances.ContainsKey(instanceId))
                throw new DictionaryConfigException("DictionaryConfig with id '" + instanceId + "' don't exists.");

            return Instance();
        }

        public static DictionaryConfig Instance(DictionaryConfig dictionaryConfigInstance,
            string instanceId = DEFAULT_INSTANCE_ID)
        {
            Assert.NotNullOrEmpty(instanceId, nameof(instanceId));
            Assert.NotNull(dictionaryConfigInstance, nameof(dictionaryConfigInstance));

            if (!instances.ContainsKey(instanceId))
                instances.Add(instanceId, dictionaryConfigInstance);

            return instances[instanceId];
        }
    }
}