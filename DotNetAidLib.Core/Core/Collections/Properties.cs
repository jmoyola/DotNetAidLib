using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class PropertiesException : Exception
    {
        public PropertiesException()
        {
        }

        public PropertiesException(string message) : base(message)
        {
        }

        public PropertiesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PropertiesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class Properties : IDictionary<string, object>
    {
        private readonly IDictionary<string, Type> assertTypes = new Dictionary<string, Type>();
        private readonly IDictionary<string, object> properties = new Dictionary<string, object>();

        public Properties()
            : this(new Dictionary<string, Type>(), new Dictionary<string, object>(), false)
        {
        }

        public Properties(IDictionary<string, object> defaults, bool readOnly)
            : this(new Dictionary<string, Type>(), defaults, readOnly)
        {
        }

        public Properties(IDictionary<string, Type> assertTypes, IDictionary<string, object> defaults, bool readOnly)
        {
            Assert.NotNull(assertTypes, nameof(assertTypes));
            Assert.NotNull(defaults, nameof(defaults));

            this.assertTypes = assertTypes;

            foreach (var kv in defaults) Add(kv.Key, kv.Value);

            // Importante que sea después de establecer los valores defaults
            IsReadOnly = readOnly;
        }

        public IDictionary<string, Type> AssertTypes
        {
            get { return assertTypes.ToDictionary(v => v.Key, v => v.Value); }
        }

        public bool ContainsKey(string key)
        {
            Assert.NotNullOrEmpty(key, nameof(key));

            return properties.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return properties.ToDictionary(v => v.Key, v => v.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string key, object value)
        {
            if (IsReadOnly)
                throw new PropertiesException("Properties is read only.");

            Assert.NotNullOrEmpty(key, nameof(key));
            AssertValue(key, value);

            properties.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Remove(string key)
        {
            Assert.NotNullOrEmpty(key, nameof(key));

            if (IsReadOnly)
                throw new PropertiesException("Properties is read only.");
            return properties.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            Assert.NotNullOrEmpty(item.Key, nameof(item));

            if (IsReadOnly)
                throw new PropertiesException("Properties is read only.");
            return properties.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;

            if (properties.ContainsKey(key))
            {
                value = properties[key];
                return true;
            }

            return false;
        }


        public void Clear()
        {
            if (IsReadOnly)
                throw new PropertiesException("Properties is read only.");
            properties.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            Assert.NotNullOrEmpty(item.Key, nameof(item));

            return properties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            properties.CopyTo(array, arrayIndex);
        }

        public ICollection<string> Keys => properties.Keys;

        public ICollection<object> Values => properties.Values;

        public int Count => properties.Count;

        public bool IsReadOnly { get; }

        public object this[string key]
        {
            get
            {
                Assert.NotNullOrEmpty(key, nameof(key));

                if (!properties.ContainsKey(key))
                    return null;
                return properties[key];
            }
            set
            {
                Assert.NotNullOrEmpty(key, nameof(key));
                AssertValue(key, value);

                if (!properties.ContainsKey(key))
                    Add(key, value);
                else
                    properties[key] = value;
            }
        }

        private void AssertValue(string name, object value)
        {
            if (value != null && assertTypes != null && assertTypes.ContainsKey(name)
                && !assertTypes[name].IsAssignableFrom(value.GetType()))
                throw new PropertiesException("Value type '" + value.GetType().Name +
                                              "' is not valid value type for property name '" + name +
                                              "'. Only type '" + assertTypes[name].Name + "' is allowed.");
        }

        public object Get(string key, object defaultValue)
        {
            Assert.NotNullOrEmpty(key, nameof(key));
            AssertValue(key, defaultValue);

            if (!properties.ContainsKey(key))
                return defaultValue;
            return properties[key];
        }
    }
}