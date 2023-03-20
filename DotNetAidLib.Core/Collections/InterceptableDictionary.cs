using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public delegate void KeyValueEventHandler<K,V>(Object sender, KeyValueEventArgs<K,V> args);

    public class KeyValueEventArgs<K,V>
    {
        private KeyValuePair<K,V> item;

        public KeyValueEventArgs(KeyValuePair<K, V> item)
        {
            this.item = item;
        }

        public KeyValueEventArgs(K key, V value)
        {
            this.item = new KeyValuePair<K, V>(key, value);
        }

        public KeyValuePair<K, V> Item
        {
            get
            {
                return item;
            }
        }
    }

    public class InterceptableDictionary<K,V>:Dictionary<K,V>
    {
        public event KeyValueEventHandler<K,V> BeforeAdd;
        public event KeyValueEventHandler<K,V> AfterAdd;
        public event KeyValueEventHandler<K,V> BeforeRemove;
        public event KeyValueEventHandler<K,V> AfterRemove;

        public InterceptableDictionary()
        {
        }

        public new void Add(K key, V value){
            this.OnBeforeAdd(new KeyValueEventArgs<K, V>(key, value));
            base.Add(key, value);
            this.OnAfterAdd(new KeyValueEventArgs<K, V>(key, value));
        }

        public new void Remove(K key)
        {
            if (!this.ContainsKey(key))
                throw new Exception("Key don't exists.");
            V value = this[key];
            this.OnBeforeRemove(new KeyValueEventArgs<K, V>(key, value));
            base.Remove(key);
            this.OnAfterRemove(new KeyValueEventArgs<K, V>(key, value));
        }

        public new void Clear()
        {
            this.Keys.ToList().ForEach(v => this.Remove(v));
        }

        protected void OnBeforeAdd(KeyValueEventArgs<K, V> args)
        {
            if (this.BeforeAdd != null)
                this.BeforeAdd(this, args);
        }

        protected void OnAfterAdd(KeyValueEventArgs<K, V> args)
        {
            if (this.AfterAdd != null)
                this.AfterAdd(this, args);
        }

        protected void OnBeforeRemove(KeyValueEventArgs<K, V> args)
        {
            if (this.BeforeRemove != null)
                this.BeforeRemove(this, args);
        }

        protected void OnAfterRemove(KeyValueEventArgs<K, V> args)
        {
            if (this.AfterRemove != null)
                this.AfterRemove(this, args);
        }
    }
}
