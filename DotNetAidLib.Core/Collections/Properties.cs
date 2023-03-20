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
        public PropertiesException ()
        {
        }

        public PropertiesException (string message) : base (message)
        {
        }

        public PropertiesException (string message, Exception innerException) : base (message, innerException)
        {
        }

        protected PropertiesException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }

    public class Properties:IDictionary<String, Object>
    {
        private bool readOnly = false;
        private IDictionary<String, Type> assertTypes=new Dictionary<String, Type>();
        private IDictionary<String, Object> properties = new Dictionary<String, Object> ();

        public Properties()
            : this(new Dictionary<String, Type>(), new Dictionary <String, Object>(), false) { }

        public Properties(IDictionary<String, Object> defaults, bool readOnly)
            :this(new Dictionary<String, Type>(), defaults, readOnly) {}

        public Properties (IDictionary<String, Type> assertTypes, IDictionary<String, Object> defaults, bool readOnly)
        {
            Assert.NotNull( assertTypes, nameof(assertTypes));
            Assert.NotNull( defaults, nameof(defaults));

            this.assertTypes = assertTypes;

            foreach (KeyValuePair<String, Object> kv in defaults){
                this.Add(kv.Key, kv.Value);
            }

            // Importante que sea después de establecer los valores defaults
            this.readOnly = readOnly;
        }

        private void AssertValue(String name, Object value)
        {
            if (value != null && this.assertTypes != null && this.assertTypes.ContainsKey(name)
                && !this.assertTypes[name].IsAssignableFrom(value.GetType()))
                throw new PropertiesException("Value type '" + value.GetType().Name + "' is not valid value type for property name '" + name + "'. Only type '" + this.assertTypes[name].Name + "' is allowed.");
        }

        public bool ContainsKey(String key)
        {
            Assert.NotNullOrEmpty( key, nameof(key));

            return this.properties.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return properties.ToDictionary(v => v.Key, v => v.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(string key, object value)
        {
            if(this.readOnly)
                throw new PropertiesException("Properties is read only.");

            Assert.NotNullOrEmpty( key, nameof(key));
            AssertValue(key, value);

            this.properties.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Remove(string key)
        {
            Assert.NotNullOrEmpty( key, nameof(key));

            if (this.readOnly)
                throw new PropertiesException("Properties is read only.");
            return this.properties.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            Assert.NotNullOrEmpty( item.Key, nameof(item));

            if (this.readOnly)
                throw new PropertiesException("Properties is read only.");
            return this.properties.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;

            if (this.properties.ContainsKey(key))
            {
                value = this.properties[key];
                return true;
            }
            else
                return false;
        }


        public void Clear()
        {
            if (this.readOnly)
                throw new PropertiesException("Properties is read only.");
            this.properties.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            Assert.NotNullOrEmpty( item.Key, nameof(item));

            return this.properties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this.properties.CopyTo(array, arrayIndex);
        }

        public IDictionary<String, Type> AssertTypes
        {
            get
            {
                return this.assertTypes.ToDictionary(v => v.Key, v => v.Value);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return this.properties.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return this.properties.Values;
            }
        }

        public int Count
        {
            get
            {
                return this.properties.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }

        public Object Get(String key, Object defaultValue)
        {
            Assert.NotNullOrEmpty( key, nameof(key));
            AssertValue(key, defaultValue);

            if (!this.properties.ContainsKey(key))
                return defaultValue;
            else
                return this.properties[key];
        }

        public object this[string key]
        {
            get {
                Assert.NotNullOrEmpty( key, nameof(key));

                if (!this.properties.ContainsKey(key))
                    return null;
                else
                    return this.properties[key];
            }
            set
            {
                Assert.NotNullOrEmpty( key, nameof(key));
                AssertValue(key, value);

                if (!this.properties.ContainsKey(key))
                    this.Add(key, value);
                else
                    this.properties[key] = value;
            }
        }
    }
}
