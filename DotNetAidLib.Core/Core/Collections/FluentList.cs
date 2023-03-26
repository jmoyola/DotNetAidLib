using System.Collections;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Collections
{
    public class FluentList<K> : IEnumerable<K>
    {
        private readonly List<K> _Items = new List<K>();

        public int Count => _Items.Count;

        public K this[int index]
        {
            get => _Items[index];
            set => _Items[index] = value;
        }

        public IEnumerator<K> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        public FluentList<K> Add(K v)
        {
            _Items.Add(v);
            return this;
        }

        public FluentList<K> Clear()
        {
            _Items.Clear();
            return this;
        }

        public FluentList<K> Remove(K v)
        {
            _Items.Remove(v);
            return this;
        }

        public IEnumerator GetEnumerator1()
        {
            return _Items.GetEnumerator();
        }

        public int IndexOf(K v)
        {
            return _Items.IndexOf(v);
        }

        public int LastIndexOf(K v)
        {
            return _Items.LastIndexOf(v);
        }

        public FluentList<K> Insert(int index, K v)
        {
            _Items.Insert(index, v);
            return this;
        }

        public FluentList<K> InsertRange(int index, IEnumerable<K> collection)
        {
            _Items.InsertRange(index, collection);
            return this;
        }

        public FluentList<K> AddRange(IEnumerable<K> collection)
        {
            _Items.AddRange(collection);
            return this;
        }

        public FluentList<K> RemoveAt(int index)
        {
            _Items.RemoveAt(index);
            return this;
        }
    }
}