using System.Collections.Generic;

namespace DotNetAidLib.Core.Collections.Generic
{
    public class DictionaryEx<K, V> : Dictionary<K, V>
    {
        public DictionaryEx()
        {
        }

        public DictionaryEx(IDictionary<K, V> dictionary) : base(dictionary)
        {
        }


        public void Add(K key, V value, bool updateIfExists)
        {
            if (!ContainsKey(key))
            {
                Add(key, value);
            }
            else
            {
                if (updateIfExists)
                    this[key] = value;
                else
                    throw new KeyNotFoundException("Key already exists.");
            }
        }

        public V GetValue(K key)
        {
            var ret = default(V);

            if (ContainsKey(key))
                ret = this[key];
            else
                throw new KeyNotFoundException("Key doin't exists.");

            return ret;
        }

        public V GetValue(K key, V defaultValue)
        {
            var ret = defaultValue;

            if (ContainsKey(key)) ret = this[key];

            return ret;
        }

        public V GetValue(K key, V defaultValue, bool createIfNotExists)
        {
            var ret = defaultValue;

            if (!ContainsKey(key))
            {
                if (createIfNotExists) Add(key, defaultValue);
            }
            else
            {
                ret = this[key];
            }

            return this[key];
        }


        public void SetValue(K key, V value)
        {
            if (ContainsKey(key))
                this[key] = value;
            else
                throw new KeyNotFoundException("Key doin't exists.");
        }

        public void SetValue(K key, V value, bool createIfNotExists)
        {
            if (ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                if (createIfNotExists)
                    Add(key, value);
                else
                    throw new KeyNotFoundException("Key doin't exists.");
            }
        }

        public override string ToString()
        {
            var ret = "";

            foreach (var kv in this)
            {
                ret = ret + ", (";
                if (kv.Key == null)
                    ret = ret + "null";
                else
                    ret = ret + kv.Key;
                ret = ret + ", ";
                if (kv.Value == null)
                    ret = ret + "null";
                else
                    ret = ret + kv.Value;
                ret = ret + ")";
            }

            if (ret.Length > 0) ret = ret.Substring(2);

            return ret;
        }
    }
}