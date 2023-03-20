using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Configuration.Dictionary.Core
{

    public abstract class DictionaryConfig : IDictionary<String, Object>
    {
        public const String DEFAULT_INSTANCE_ID = "_appconfig";
        private static Dictionary<String, DictionaryConfig> instances = new Dictionary<String, DictionaryConfig> ();

        public void Set<T> (String key, T value)
        {
            if (!this.ContainsKey(key))
                this.Add (key, value);
            else
                this [key] = value;
        }

        public T Get<T> (String key, T defaultValueIfNotExists)
        {
            if (!this.ContainsKey(key))
                return defaultValueIfNotExists;
            else
                return (T)this [key];
        }

        public T Get<T> (String key)
        {
            if (!this.ContainsKey (key))
                throw new DictionaryConfigException ("Key '" + key + "' don't exists.");

            return (T)this [key];
        }

        public bool IsSet(String key)
        {
            return this.ContainsKey(key);
        }
        public bool UnSet(String key)
        {
            return this.Remove(key);
        }

        public IList<String> GetKeyMatches (String pattern)
        {
            return this.Keys.Where(v => Willcards.SQLMatch(pattern, v)).ToList();
        }

        public abstract IList<KeyValuePair<String, Object>> GetValueMatches(String pattern);

        public abstract object this [string key] { get; set; }

        public abstract ICollection<string> Keys { get; }
        public abstract ICollection<object> Values { get; }
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }

        public abstract void Add (string key, object value);

        public abstract void Clear ();

        public void Add (KeyValuePair<string, object> item)
        {
            this.Add (item.Key, item.Value);
        }

        public bool Contains (KeyValuePair<string, object> item) {
            return this.ContainsKey (item.Key);
        }

        public abstract bool ContainsKey (string key);

        public void CopyTo (KeyValuePair<string, Object> [] array, int arrayIndex)
        {
            try {
                IList<KeyValuePair<string, Object>> items = this.GetItems ();
                for (int i = 0; i < items.Count; i++)
                    array [arrayIndex + i] = items [i];
            } catch (Exception ex) {
                throw new DictionaryConfigException ("Error copy to.", ex);
            }
        }

        public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator ();
        public abstract bool Remove (string key);
        public abstract bool Remove (KeyValuePair<string, object> item);

        public abstract bool TryGetValue (string key, out object value);

        protected abstract IList<KeyValuePair<string, Object>> GetItems ();

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return this.GetItems ().GetEnumerator ();
        }

        public static DictionaryConfig Instance (String instanceId=DEFAULT_INSTANCE_ID)
        {
            if (!instances.ContainsKey (instanceId))
                throw new DictionaryConfigException ("DictionaryConfig with id '" + instanceId + "' don't exists.");
            
            return Instance (DEFAULT_INSTANCE_ID);
        }

        public static DictionaryConfig Instance (DictionaryConfig dictionaryConfigInstance, String instanceId=DEFAULT_INSTANCE_ID)
        {
            Assert.NotNullOrEmpty( instanceId, nameof(instanceId));
            Assert.NotNull( dictionaryConfigInstance, nameof(dictionaryConfigInstance));
            
            if (!instances.ContainsKey (instanceId))
                instances.Add(instanceId, dictionaryConfigInstance);
            
            return instances [instanceId];
        }
    }
}
