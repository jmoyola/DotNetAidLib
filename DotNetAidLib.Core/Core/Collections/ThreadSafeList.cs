﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Collections
{
    public class ThreadSafeList<K> : IList<K>
    {
        private readonly List<K> _Items;

        protected object oLock = new object();

        public ThreadSafeList()
        {
            _Items = new List<K>();
        }

        public ThreadSafeList(int capacity)
        {
            _Items = new List<K>(capacity);
        }

        public ThreadSafeList(IEnumerable<K> collection)
        {
            _Items = new List<K>(collection);
        }

        public object Lock => oLock;

        public void Add(K v)
        {
            lock (oLock)
            {
                _Items.Add(v);
            }
        }

        public void Clear()
        {
            lock (oLock)
            {
                _Items.Clear();
            }
        }

        public bool Remove(K item)
        {
            lock (oLock)
            {
                return _Items.Remove(item);
            }
        }

        public int Count
        {
            get
            {
                lock (oLock)
                {
                    return _Items.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (oLock)
                {
                    return ((IList<K>) _Items).IsReadOnly;
                }
            }
        }

        public IEnumerator<K> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        public int IndexOf(K v)
        {
            lock (oLock)
            {
                return _Items.IndexOf(v);
            }
        }

        public void Insert(int index, K v)
        {
            lock (oLock)
            {
                _Items.Insert(index, v);
            }
        }

        public K this[int index]
        {
            get
            {
                lock (oLock)
                {
                    return _Items[index];
                }
            }
            set
            {
                lock (oLock)
                {
                    _Items[index] = value;
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (oLock)
            {
                _Items.RemoveAt(index);
            }
        }

        public bool Contains(K item)
        {
            lock (oLock)
            {
                return _Items.Contains(item);
            }
        }

        public void CopyTo(K[] array, int arrayIndex)
        {
            lock (oLock)
            {
                _Items.CopyTo(array, arrayIndex);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ForEach(Action<K> action)
        {
            lock (oLock)
            {
                _Items.ForEach(action);
            }
        }

        public int LastIndexOf(K v)
        {
            lock (oLock)
            {
                return _Items.LastIndexOf(v);
            }
        }

        public void InsertRange(int index, IEnumerable<K> collection)
        {
            lock (oLock)
            {
                _Items.InsertRange(index, collection);
            }
        }

        public void AddRange(IEnumerable<K> collection)
        {
            lock (oLock)
            {
                _Items.AddRange(collection);
            }
        }
    }
}