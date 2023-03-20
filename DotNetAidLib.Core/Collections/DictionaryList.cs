using System;
using System.Linq;

namespace DotNetAidLib.Core.Collections 
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	public class DictionaryList<K, V> : IList<KeyValue<K, V>>
    {
		private List<KeyValue<K, V>> _Items = new List<KeyValue<K, V>>();
		public void Add(K key, V value)
		{
			_Items.Add(new KeyValue<K, V>(key, value));
		}

		public void Add(KeyValue<K, V> keyValue)
		{
			_Items.Add(keyValue);
		}

		public void Clear()
		{
			_Items.Clear();
		}

		public bool ContainsKey(K key)
		{
			KeyValue<K, V> lst = _Items.FirstOrDefault(v => v.Key.Equals(key));
			return (lst != null);
		}

		public int Count {
			get { return _Items.Count; }
		}

        public ICollection<K> Keys {
            get {
                return _Items.Select(v=>v.Key).ToList();
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return _Items.Select(v => v.Value).ToList();
            }
        }

        public bool IsReadOnly { get { return false; } }

        public void Remove(K key)
		{
			IEnumerable<KeyValue<K, V>> lstToDelete = _Items.Where(v => v.Key.Equals(key));
			foreach (KeyValue<K, V> Item in lstToDelete) {
				_Items.Remove(Item);
			}
		}

		public System.Collections.Generic.IEnumerator<KeyValue<K, V>> GetEnumerator()
		{
			return _Items.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _Items.GetEnumerator();
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

		public void Insert(int index, KeyValue<K, V> keyValue)
		{
			_Items.Insert(index, keyValue);
		}

		public void InsertRange(int index, IEnumerable<KeyValue<K, V>> collection)
		{
			_Items.InsertRange(index, collection);
		}

		public KeyValue<K, V> this[int index] {
			get { return _Items[index]; }
			set { _Items[index] = value; }
		}

		public V this[K key] {
			get {
				KeyValue<K, V> i = _Items.FirstOrDefault(v => v.Key.Equals(key));

				if ((i == null)) {
					throw new Exception("key don't exists.");
				}

				return i.Value;
			}
			set {
				KeyValue<K, V> i = _Items.FirstOrDefault(v => v.Key.Equals(key));

				if ((i == null)) {
					throw new Exception("key don't exists.");
				}

				i.Value = value;
			}
		}

		public IEnumerable<V> ItemGroup(K key) {
			IEnumerable<KeyValue<K, V>> i = _Items.Where(v => v.Key.Equals(key));

			if ((i.Count() == 0)) {
				throw new Exception("key don't exists.");
			}

			return i.Select(v => v.Value);
		}

		public void AddRange(IEnumerable<KeyValue<K, V>> collection)
		{
			_Items.AddRange(collection);
		}

		public void RemoveAt(int index)
		{
			_Items.RemoveAt(index);
		}

        public V Get(K key, V ifNotExists) {
            if (!this.ContainsKey(key))
                return ifNotExists;
            else
                return this[key];
        }

        public void Set(K key, V value, bool addIfNotExists)
        {
            if (!this.ContainsKey(key) && addIfNotExists)
                this.Add(key, default(V));

            this[key]=value;
        }

        public int IndexOf (KeyValue<K, V> item)
        {
            return _Items.IndexOf (item);
        }

        public bool Contains (KeyValue<K, V> item)
        {
            return _Items.Contains (item);
        }

        public void CopyTo (KeyValue<K, V> [] array, int arrayIndex)
        {
            _Items.CopyTo (array, arrayIndex);
        }

        public bool Remove (KeyValue<K, V> item)
        {
            return _Items.Remove (item);
        }
    }
}

