using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Configuration.Dictionary.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration.Dictionary.Impl
{
    [DataContract]
    public class MemoryDictionaryConfig:DictionaryConfig
    {
        [DataMember]
        private Dictionary<String, Object> content = new Dictionary<string, object> ();
        public MemoryDictionaryConfig ()
        {
        }

        public override object this [string key] {
            get {
                return this.content [key];
            }

            set {
                this.content [key] = value;
            }
        }

        public override IList<KeyValuePair<String, Object>> GetValueMatches(String pattern)
        {
            return this.content.Where(kv => kv.Key.RegexIsMatch(pattern)).ToList();
        }

        public override ICollection<string> Keys => this.content.Keys;

        public override ICollection<object> Values => this.content.Values;

        public override int Count => this.content.Count;

        public override bool IsReadOnly => false;

        public override void Add (string key, object value)
        {
            this.content.Add(key, value);
        }

        public override void Clear ()
        {
            this.content.Clear();
        }

        public override bool ContainsKey (string key)
        {
            return this.content.ContainsKey (key);
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
        {
            return this.content.GetEnumerator();
        }

        public override bool Remove (string key)
        {
            return this.content.Remove(key);
        }

        public override bool Remove (KeyValuePair<string, object> item)
        {
            return this.content.Remove(item.Key);
        }

        public override bool TryGetValue (string key, out object value)
        {
            return this.content.TryGetValue (key, out value);
        }

        protected override IList<KeyValuePair<string, object>> GetItems ()
        {
            return this.content.Where(v=>true).ToList();
        }
    }
}
