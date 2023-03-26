using System;
using System.Collections;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class TypedValue
    {
        private object value;

        public TypedValue(object value)
        {
            Value = value;
        }

        public Type Type { get; private set; }

        public object Value
        {
            get => value;
            set
            {
                Assert.NotNull(value, nameof(value));
                Type = value.GetType();
                this.value = value;
            }
        }

        public T Get<T>()
        {
            return (T) Convert.ChangeType(value, typeof(T));
        }

        public bool Is<T>()
        {
            return Type.Equals(nameof(T));
        }
    }

    public class NameTypedDictionary : IEnumerable<KeyValuePair<string, TypedValue>>
    {
        private readonly IDictionary<string, TypedValue> content;

        public NameTypedDictionary()
        {
            content = new Dictionary<string, TypedValue>();
        }

        public object this[string key]
        {
            get => Get<object>(key, null);
            set => Add(key, value, true);
        }

        public int Count => content.Count;

        public IEnumerator<KeyValuePair<string, TypedValue>> GetEnumerator()
        {
            return content.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return content.GetEnumerator();
        }

        public void Add<T>(string key, T o, bool updateIfExists = false)
        {
            if (ContainsKey<T>(key))
                content[key] = new TypedValue(o);
            else
                content.Add(key, new TypedValue(o));
        }

        public T Get<T>(string key)
        {
            return (T) content[key].Value;
        }

        public T Get<T>(string key, T valueIfNotExists)
        {
            if (!ContainsKey<T>(key))
                return valueIfNotExists;
            return (T) content[key].Value;
        }

        public void Remove(string key)
        {
            content.Remove(key);
        }

        public Type Typeof(string key)
        {
            return content[key].Type;
        }

        public bool ContainsKey<T>(string key)
        {
            return content.ContainsKey(key) && content[key].Type.Equals(typeof(T));
        }

        public bool ContainsKey(string key)
        {
            return content.ContainsKey(key);
        }
    }
}