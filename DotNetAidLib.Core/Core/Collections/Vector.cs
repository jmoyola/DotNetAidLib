using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class Vector<T> : IEnumerable<T>
    {
        private readonly T[] _elements;

        public Vector(int length)
        {
            Assert.GreaterThan(length, 0, nameof(length));
            _elements = (T[]) Activator.CreateInstance(typeof(T[]), length);
        }

        public int Length => _elements.Length;

        public T this[int index]
        {
            get => _elements[index];
            set => _elements[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _elements.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(_elements, item) > -1;
        }

        public void CopyTo(T[] destination, int destinationIndex)
        {
            Array.Copy(_elements, 0, destination, destinationIndex, _elements.Length);
        }

        public void CopyFrom(T[] source, int sourceIndex)
        {
            Array.Copy(source, sourceIndex, _elements, 0, _elements.Length);
        }

        public int IndexOf(byte item)
        {
            return Array.IndexOf(_elements, item);
        }

        public static implicit operator Vector<T>(T[] value)
        {
            return new Vector<T>(value.Length);
        }

        public static implicit operator T[](Vector<T> value)
        {
            return value._elements;
        }

        public override string ToString()
        {
            return _elements.ToStringJoin(", ");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj != null
                    && obj.GetType().IsInstanceOfType(GetType())
                    && ((Vector<T>) obj).Length == Length
                    && ((Vector<T>) obj).ToList().All((v, i) => v.Equals(_elements[i])))
                   || (obj.GetType().IsInstanceOfType(typeof(T[]))
                       && ((T[]) obj).Length == Length
                       && ((Vector<T>) obj).ToList().All((v, i) => v.Equals(_elements[i])));
        }
    }
}