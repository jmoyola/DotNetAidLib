using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class CaptionDictionaryList<K, V> : IEnumerable<CaptionKeyValue<K, V>>
    {
        private readonly List<CaptionKeyValue<K, V>> _Items = new List<CaptionKeyValue<K, V>>();
        protected bool changed;

        public string Caption { get; set; } = null;

        public int Count => _Items.Count;

        public bool Changed
        {
            get => changed;
            set => changed = value;
        }

        public CaptionKeyValue<K, V> this[int index]
        {
            get => _Items[index];
            set
            {
                _Items[index] = value;
                changed = true;
            }
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
                changed = true;
            }
        }

        public IEnumerator<CaptionKeyValue<K, V>> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        public void Add(K key, V value)
        {
            Add(new CaptionKeyValue<K, V>(key, value));
        }

        public void Add(K key, V value, string comment)
        {
            Add(new CaptionKeyValue<K, V>(key, value, comment));
        }

        public void Add(CaptionKeyValue<K, V> keyValue)
        {
            _Items.Add(keyValue);
            changed = true;
        }

        public void AddOrUpdate(K key, V value)
        {
            var i = IndexOf(key);
            if (i > 0)
            {
                this[i].Value = value;
                changed = true;
            }
            else
            {
                Add(key, value);
            }
        }

        public void AddOrUpdate(K key, V value, string comment)
        {
            var i = IndexOf(key);
            if (i > 0)
            {
                this[i].Value = value;
                this[i].Caption = comment;
                changed = true;
            }
            else
            {
                Add(key, value, comment);
            }
        }

        public void AddOrUpdate(CaptionKeyValue<K, V> keyValue)
        {
            var i = IndexOf(keyValue.Key);
            if (i > 0)
            {
                this[i].Value = keyValue.Value;
                this[i].Caption = keyValue.Caption;
                changed = true;
            }
            else
            {
                Add(keyValue);
            }
        }

        public void Clear()
        {
            _Items.Clear();
        }

        public bool ContainsKey(K key)
        {
            KeyValue<K, V> lst = _Items.FirstOrDefault(v => v.Key.Equals(key));
            return lst != null;
        }

        public void Remove(K key)
        {
            var lstToDelete = _Items.Where(v => v.Key.Equals(key));
            for (var i = 0; i < lstToDelete.Count(); i++)
            {
                var item = lstToDelete.ToList()[i];
                _Items.Remove(item);
            }


            changed = lstToDelete.Count() > 0;
        }

        public IEnumerator GetEnumerator1()
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
            _Items.Insert(index, new CaptionKeyValue<K, V>(key, value));
            changed = true;
        }

        public void Insert(int index, K key, V value, string comment)
        {
            _Items.Insert(index, new CaptionKeyValue<K, V>(key, value, comment));
            changed = true;
        }

        public void Insert(int index, CaptionKeyValue<K, V> keyValue)
        {
            _Items.Insert(index, keyValue);
            changed = true;
        }

        public void InsertRange(int index, IEnumerable<CaptionKeyValue<K, V>> collection)
        {
            _Items.InsertRange(index, collection);
            changed = collection.Count() > 0;
        }

        public IEnumerable<V> ItemGroup(K key)
        {
            var i = _Items.Where(v => v.Key.Equals(key));

            if (i.Count() == 0) throw new Exception("key don't exists.");

            return i.Select(v => v.Value);
        }

        public void AddRange(IEnumerable<CaptionKeyValue<K, V>> collection)
        {
            _Items.AddRange(collection);
            changed = collection.Count() > 0;
        }

        public void RemoveAt(int index)
        {
            _Items.RemoveAt(index);
            changed = true;
        }
    }
}