using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Configuration.Dictionary.Core;

namespace DotNetAidLib.Core.Configuration.Dictionary.Impl
{
    [DataContract]
    public class MemoryDictionaryConfig : DictionaryConfig
    {
        [DataMember] private Dictionary<string, object> content = new Dictionary<string, object>();

        public override object this[string key]
        {
            get => content[key];

            set => content[key] = value;
        }

        public override ICollection<string> Keys => content.Keys;

        public override ICollection<object> Values => content.Values;

        public override int Count => content.Count;

        public override bool IsReadOnly => false;

        public override IList<KeyValuePair<string, object>> GetValueMatches(string pattern)
        {
            return content.Where(kv => kv.Key.RegexIsMatch(pattern)).ToList();
        }

        public override void Add(string key, object value)
        {
            content.Add(key, value);
        }

        public override void Clear()
        {
            content.Clear();
        }

        public override bool ContainsKey(string key)
        {
            return content.ContainsKey(key);
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return content.GetEnumerator();
        }

        public override bool Remove(string key)
        {
            return content.Remove(key);
        }

        public override bool Remove(KeyValuePair<string, object> item)
        {
            return content.Remove(item.Key);
        }

        public override bool TryGetValue(string key, out object value)
        {
            return content.TryGetValue(key, out value);
        }

        protected override IList<KeyValuePair<string, object>> GetItems()
        {
            return content.Where(v => true).ToList();
        }
    }
}