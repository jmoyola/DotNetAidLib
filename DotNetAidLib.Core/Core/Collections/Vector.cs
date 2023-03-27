using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public struct Vector<T> : IEnumerable<T>
    {
        private T[] _content;

        public Vector(params T[] content)
        :this(content.ToList())
        { }

        public Vector(IList<T> content)
        {
            Assert.NotNullOrEmpty(content, nameof(content));
            _content = (T[]) Activator.CreateInstance(typeof(T[]), content.Count);
            content.ToArray().CopyTo(_content, 0);
        }

        public Vector(int length)
        {
            Assert.GreaterThan(length, 0, nameof(length));
            _content = (T[]) Activator.CreateInstance(typeof(T[]), length);
        }

        public int Length => _content.Length;

        public T this[int index]
        {
            get => _content[index];
            set => _content[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _content.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _content.GetEnumerator();
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(_content, item) > -1;
        }

        public void CopyTo(T[] destination, int destinationIndex)
        {
            Array.Copy(_content, 0, destination, destinationIndex, _content.Length);
        }

        public void CopyFrom(T[] source, int sourceIndex)
        {
            Array.Copy(source, sourceIndex, _content, 0, _content.Length);
        }

        public Vector<T> Clone()
        {
            return new Vector<T>(this._content);
        }

        public int IndexOf(byte item)
        {
            return Array.IndexOf(_content, item);
        }

        public static implicit operator Vector<T>(T[] value)
        {
            return new Vector<T>(value.Length);
        }

        public static implicit operator T[](Vector<T> value)
        {
            return value._content;
        }

        public override string ToString()
        {
            return _content.ToStringJoin(", ");
        }
        
    }
}