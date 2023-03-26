using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Collections
{
    public class ReadOnlyTypedProperties : IEnumerable<KeyValuePair<string, object>>
    {
        protected IDictionary<TypedProperty, object> values;

        public ReadOnlyTypedProperties(IDictionary<TypedProperty, object> values)
        {
            Assert.NotNull(values, nameof(values));

            this.values = values;
        }

        public ReadOnlyTypedProperties(IList<TypedProperty> values)
        {
            Assert.NotNull(values, nameof(values));

            this.values = values.ToDictionary(v => v, v => v.Type.GetDefault());
        }

        public object this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return values.ToDictionary(v => v.Key.Name, v => v.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Get<T>(string key)
        {
            return (T) Get(key);
        }

        public bool ContainsKey(string key)
        {
            var tp = values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            return tp != null;
        }

        public object Get(string key)
        {
            var tp = values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);
            return values[tp];
        }

        public void Set(string key, object value)
        {
            var tp = values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);

            tp.Validate(value);

            values[tp] = value;
        }

        public object Cast(string key, string value)
        {
            var tp = values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);

            return tp.Cast(value);
        }
    }
}