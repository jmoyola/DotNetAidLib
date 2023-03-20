using System;
using System.Linq;

namespace DotNetAidLib.Core.Collections 
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	public class FluentList<K> : IEnumerable<K>
	{
		private List<K> _Items = new List<K>();
        public FluentList<K> Add(K v)
		{
			_Items.Add(v);
            return this;
		}

        public FluentList<K> Clear(){
			_Items.Clear();
            return this;
		}

		public int Count {
			get { return _Items.Count; }
		}

        public FluentList<K> Remove(K v){
			_Items.Remove(v);
            return this;
		}

		public System.Collections.Generic.IEnumerator<K> GetEnumerator()
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

		public K this[int index] {
			get { return _Items[index]; }
			set { _Items[index] = value; }
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

