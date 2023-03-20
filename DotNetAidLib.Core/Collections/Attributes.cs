using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class Attributes : IEnumerable<KeyValuePair<String, Object>>
    {
        private IDictionary<String, Object> baseDictionary=null;

        public Attributes()
            : this(new Dictionary<string, object>()) { }

        public Attributes(IDictionary<String, Object> baseDictionary)
        {
            Assert.NotNull( baseDictionary, nameof(baseDictionary));
            this.baseDictionary = baseDictionary;
        }

        public IDictionary<String, Object> BaseDictionary
        {
            get => this.baseDictionary;
        }

        public Object this[String key]
        {
            get =>this.baseDictionary.ContainsKey(key) ? this.baseDictionary[key] : null;
            set
            {
                if(this.baseDictionary.ContainsKey(key))
                    this.baseDictionary[key] = value;
                else
                    this.baseDictionary.Add(key, value);
            }
        }

        public T Get<T>(String key)
        {
            return this.Get<T>(key, default(T));
        }

        public T Get<T>(String key, T defaultValueIfNotExists)
        {
            if (!this.baseDictionary.ContainsKey(key))
                return defaultValueIfNotExists;
            return (T)this.baseDictionary[key];
        }

        public void Set<T>(String key, T value)
        {
            if (!baseDictionary.ContainsKey(key))
                this.baseDictionary.Add(key, value);
            else
                this.baseDictionary[key]=value;
        }

        public void UnSet(String key)
        {
            if (baseDictionary.ContainsKey(key))
                this.baseDictionary.Remove(key);
        }
        
        public bool ContainsKey(String key)
        {
            return this.baseDictionary.ContainsKey(key);
        }

        public IEnumerable<String> Keys { get => this.baseDictionary.Keys; }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.baseDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.baseDictionary.GetEnumerator();
        }

        public override string ToString()
        {
            return this.baseDictionary.Select(kv=>kv.Key + "=>" + (kv.Value==null?"<null>":kv.ToString())).ToStringJoin((", "));
        }
    }
}