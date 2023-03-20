using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections 
{
    public class ThreadSafeDictionary<K,V> : IDictionary<K,V>
    {
        private readonly Dictionary<K,V> items;
        protected Object oLock = new object ();

        public Object Lock
        {
            get { return this.oLock; }
        }

        public void ForEach(Action<KeyValuePair<K,V>> action)
        {
            lock (oLock)
            {
                foreach(KeyValuePair<K, V> kv in items)
                    action(kv);
            }
        }

        public ThreadSafeDictionary () {
            items = new Dictionary<K,V> ();
        }
        public ThreadSafeDictionary (int capacity)
        {
            items = new Dictionary<K, V> (capacity);
        }
        public ThreadSafeDictionary (IDictionary<K,V> dictionary)
        {
            items = new Dictionary<K,V> (dictionary);
        }
        public ThreadSafeDictionary (IDictionary<K,V> dictionary, IEqualityComparer<K> comparer)
        {
            items = new Dictionary<K,V> (dictionary, comparer);
        }
        public ThreadSafeDictionary (IEqualityComparer<K> comparer)
        {
            items = new Dictionary<K,V> (comparer);
        }
        public ThreadSafeDictionary (int capacity, IEqualityComparer<K> comparer)
        {
            items = new Dictionary<K,V> (capacity, comparer);
        }

        public void Add(K key, V value)
		{
            lock (oLock)
                items.Add(key,value);
		}

        public void Add(KeyValuePair<K, V> item)
        {
            lock (oLock)
                ((IDictionary<K,V>)items).Add(item);
        }

        public void Clear(){

            lock (oLock) 
                items.Clear();
		}

        public bool Contains(KeyValuePair<K, V> item)
        {
            lock (oLock)
                return items.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            lock (oLock)
                ((IDictionary<K,V>)items).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            lock (oLock)
                return ((IDictionary<K, V>) items).Remove(item);
        }

        public int Count {
			get {
                lock (oLock)
                    return items.Count;
            }
		}

        public bool IsReadOnly
        {
            get
            {
                lock (oLock)
                    return ((IDictionary<K,V>)items).IsReadOnly;
            }
        }

        public bool Remove(K key){
            lock (oLock)
                return items.Remove(key);
		}

        public bool TryGetValue(K key, out V value)
        {
            lock (oLock)
                return ((IDictionary<K, V>) items).TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<K,V>> GetEnumerator()
		{
            lock (oLock)
                return items.GetEnumerator();
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (oLock)
                return items.GetEnumerator();
        }
        
        public bool ContainsKey(K key)
		{
            lock (oLock)
                return items.ContainsKey(key);
		}

        public ICollection<K> Keys
		{
            get
            {
                lock (oLock)
                    return items.Keys;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                lock (oLock)
                    return items.Values;
            }
        }
        
        public V this[K key] {
			get {
                lock (oLock)
                    return items[key];
            }
			set {
                lock (oLock)
                    items[key] = value;
            }
		}
        
    }
}

