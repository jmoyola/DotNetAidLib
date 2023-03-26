using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public delegate void KeyValueEventHandler<K, V>(object sender, KeyValueEventArgs<K, V> args);

    public class KeyValueEventArgs<K, V>
    {
        public KeyValueEventArgs(KeyValuePair<K, V> item)
        {
            Item = item;
        }

        public KeyValueEventArgs(K key, V value)
        {
            Item = new KeyValuePair<K, V>(key, value);
        }

        public KeyValuePair<K, V> Item { get; }
    }

    public class InterceptableDictionary<K, V> : Dictionary<K, V>
    {
        public event KeyValueEventHandler<K, V> BeforeAdd;
        public event KeyValueEventHandler<K, V> AfterAdd;
        public event KeyValueEventHandler<K, V> BeforeRemove;
        public event KeyValueEventHandler<K, V> AfterRemove;

        public new void Add(K key, V value)
        {
            OnBeforeAdd(new KeyValueEventArgs<K, V>(key, value));
            base.Add(key, value);
            OnAfterAdd(new KeyValueEventArgs<K, V>(key, value));
        }

        public new void Remove(K key)
        {
            if (!ContainsKey(key))
                throw new Exception("Key don't exists.");
            var value = this[key];
            OnBeforeRemove(new KeyValueEventArgs<K, V>(key, value));
            base.Remove(key);
            OnAfterRemove(new KeyValueEventArgs<K, V>(key, value));
        }

        public new void Clear()
        {
            Keys.ToList().ForEach(v => Remove(v));
        }

        protected void OnBeforeAdd(KeyValueEventArgs<K, V> args)
        {
            if (BeforeAdd != null)
                BeforeAdd(this, args);
        }

        protected void OnAfterAdd(KeyValueEventArgs<K, V> args)
        {
            if (AfterAdd != null)
                AfterAdd(this, args);
        }

        protected void OnBeforeRemove(KeyValueEventArgs<K, V> args)
        {
            if (BeforeRemove != null)
                BeforeRemove(this, args);
        }

        protected void OnAfterRemove(KeyValueEventArgs<K, V> args)
        {
            if (AfterRemove != null)
                AfterRemove(this, args);
        }
    }
}