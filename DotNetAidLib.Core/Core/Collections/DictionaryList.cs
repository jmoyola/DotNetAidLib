using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class DictionaryList<K, V> : IList<KeyValue<K, V>>
    {
        private readonly List<KeyValue<K, V>> _Items = new List<KeyValue<K, V>>();

        public ICollection<K> Keys
        {
            get { return _Items.Select(v => v.Key).ToList(); }
        }

        public ICollection<V> Values
        {
            get { return _Items.Select(v => v.Value).ToList(); }
        }

        public V this[K key]
        {
            get
            {
                var i = _Items.FirstOrDefault(v => v.Key.Equals(key));

                if (i == null) throw new Exception("key don't exists.");

                return i.Value;
            }
            set
            {
                var i = _Items.FirstOrDefault(v => v.Key.Equals(key));

                if (i == null) throw new Exception("key don't exists.");

                i.Value = value;
            }
        }

        public void Add(KeyValue<K, V> keyValue)
        {
            _Items.Add(keyValue);
        }

        public void Clear()
        {
            _Items.Clear();
        }

        public int Count => _Items.Count;

        public bool IsReadOnly => false;

        public IEnumerator<KeyValue<K, V>> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        public void Insert(int index, KeyValue<K, V> keyValue)
        {
            _Items.Insert(index, keyValue);
        }

        public KeyValue<K, V> this[int index]
        {
            get => _Items[index];
            set => _Items[index] = value;
        }

        public void RemoveAt(int index)
        {
            _Items.RemoveAt(index);
        }

        public int IndexOf(KeyValue<K, V> item)
        {
            return _Items.IndexOf(item);
        }

        public bool Contains(KeyValue<K, V> item)
        {
            return _Items.Contains(item);
        }

        public void CopyTo(KeyValue<K, V>[] array, int arrayIndex)
        {
            _Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValue<K, V> item)
        {
            return _Items.Remove(item);
        }

        public void Add(K key, V value)
        {
            _Items.Add(new KeyValue<K, V>(key, value));
        }

        public bool ContainsKey(K key)
        {
            var lst = _Items.FirstOrDefault(v => v.Key.Equals(key));
            return lst != null;
        }

        public void Remove(K key)
        {
            var lstToDelete = _Items.Where(v => v.Key.Equals(key));
            foreach (var Item in lstToDelete) _Items.Remove(Item);
        }

        public int IndexOf(K key)
        {
            return _Items.FindIndex(v => v.Key.Equals(key));
        }

        public int IndexOf(int startIndex, K key)
        {
            return _Items.FindIndex(startIndex, v => v.Key.Equals(key));
        }

        public void Insert(int index, K key, V value)
        {
            _Items.Insert(index, new KeyValue<K, V>(key, value));
        }

        public void InsertRange(int index, IEnumerable<KeyValue<K, V>> collection)
        {
            _Items.InsertRange(index, collection);
        }

        public IEnumerable<V> ItemGroup(K key)
        {
            var i = _Items.Where(v => v.Key.Equals(key));

            if (i.Count() == 0) throw new Exception("key don't exists.");

            return i.Select(v => v.Value);
        }

        public void AddRange(IEnumerable<KeyValue<K, V>> collection)
        {
            _Items.AddRange(collection);
        }

        public V Get(K key, V ifNotExists)
        {
            if (!ContainsKey(key))
                return ifNotExists;
            return this[key];
        }

        public void Set(K key, V value, bool addIfNotExists)
        {
            if (!ContainsKey(key) && addIfNotExists)
                Add(key, default);

            this[key] = value;
        }
    }
}