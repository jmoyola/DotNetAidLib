using System;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	public class CaptionDictionaryList<K, V> : IEnumerable<CaptionKeyValue<K, V>>
	{
        protected bool changed = false;
        private String caption = null;

        public String Caption
        {
            get { return caption; }
            set { caption = value; }
        }

        private List<CaptionKeyValue<K, V>> _Items = new List<CaptionKeyValue<K, V>>();
		public void Add(K key, V value) { 
			this.Add(new CaptionKeyValue<K, V>(key, value));
        }

		public void Add(K key, V value, String comment){
			this.Add(new CaptionKeyValue<K, V>(key, value, comment));
        }

		public void Add(CaptionKeyValue<K, V> keyValue)
		{
			_Items.Add(keyValue);
            this.changed = true;
        }

        public void AddOrUpdate(K key, V value)
        {
            int i = this.IndexOf(key);
            if (i > 0)
            {
                this[i].Value = value;
                this.changed = true;
            }
            else
                this.Add(key, value);

        }

        public void AddOrUpdate(K key, V value, String comment) {
            int i = this.IndexOf(key);
            if (i > 0)
            {
                this[i].Value = value;
                this[i].Caption = comment;
                this.changed = true;
            }
            else
                this.Add(key, value, comment);
        }

        public void AddOrUpdate(CaptionKeyValue<K, V> keyValue)
        {
            int i = this.IndexOf(keyValue.Key);
            if (i > 0)
            {
                this[i].Value = keyValue.Value;
                this[i].Caption = keyValue.Caption;
                this.changed = true;
            }
            else
                this.Add(keyValue);
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

        public bool Changed
        {
            get{
                return this.changed;
            }
            set
            {
                this.changed=value;
            }
        }

        public void Remove(K key)
		{
			IEnumerable<CaptionKeyValue<K, V>> lstToDelete = _Items.Where(v => v.Key.Equals(key));
            for (int i = 0; i < lstToDelete.Count(); i++){
                CaptionKeyValue<K, V> item= lstToDelete.ToList()[i];
                _Items.Remove(item);
            }



            this.changed = lstToDelete.Count()>0;
        }

		public System.Collections.Generic.IEnumerator<CaptionKeyValue<K, V>> GetEnumerator()
		{
			return _Items.GetEnumerator();
		}

		public System.Collections.IEnumerator GetEnumerator1()
		{
			return _Items.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator1();
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
			_Items.Insert(index, new CaptionKeyValue<K, V>(key, value));
            this.changed = true;
        }

		public void Insert(int index, K key, V value, String comment)
		{
			_Items.Insert(index, new CaptionKeyValue<K, V>(key, value, comment));
            this.changed = true;
        }

		public void Insert(int index, CaptionKeyValue<K, V> keyValue)
		{
			_Items.Insert(index, keyValue);
            this.changed = true;
        }

		public void InsertRange(int index, IEnumerable<CaptionKeyValue<K, V>> collection)
		{
			_Items.InsertRange(index, collection);
            this.changed = collection.Count() > 0;
        }

		public CaptionKeyValue<K, V> this[int index] {
			get { return _Items[index]; }
			set {
                _Items[index] = value;
                this.changed = true;
            }
		}

		public V this[K key] {
			get {
				CaptionKeyValue<K, V> i = _Items.FirstOrDefault(v => v.Key.Equals(key));

				if ((i == null)) {
					throw new Exception("key don't exists.");
				}

				return i.Value;
			}
			set {
				CaptionKeyValue<K, V> i = _Items.FirstOrDefault(v => v.Key.Equals(key));

				if ((i == null)) {
					throw new Exception("key don't exists.");
				}

				i.Value = value;
                this.changed = true;
            }
		}

		public IEnumerable<V> ItemGroup(K key) {
			IEnumerable<CaptionKeyValue<K, V>> i = _Items.Where(v => v.Key.Equals(key));

			if ((i.Count() == 0)) {
				throw new Exception("key don't exists.");
			}

			return i.Select(v => v.Value);
		}

		public void AddRange(IEnumerable<CaptionKeyValue<K, V>> collection)
		{
			_Items.AddRange(collection);
            this.changed = collection.Count()>0;
        }

		public void RemoveAt(int index)
		{
			_Items.RemoveAt(index);
            this.changed = true;
        }
	}
}

