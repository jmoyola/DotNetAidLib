using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Collections
{
    public class TypedProperties : IDictionary<TypedProperty, object>
    {
        private readonly IDictionary<TypedProperty, object> _values;
        private bool _readOnly;

        public TypedProperties()
        {
            this._values = new Dictionary<TypedProperty, object>();
        }

        public TypedProperties(IDictionary<TypedProperty, object> values)
        {
            Assert.NotNull(values, nameof(values));

            this._values = values;
        }

        public TypedProperties(IList<TypedProperty> values)
        {
            Assert.NotNull(values, nameof(values));

            this._values = values.ToDictionary(v => v, v => v.Type.GetDefault());
        }

        public object this[TypedProperty key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        public object this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public IEnumerator<KeyValuePair<TypedProperty, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            Assert.NotNullOrEmpty(key, nameof(key));
            
            TypedProperty tp = _values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            return tp != null;
        }
        public bool ContainsKey(TypedProperty key)
        {
            Assert.NotNull(key, nameof(key));
            
            return _values.ContainsKey(key);
        }
        
        public T Get<T>(string key)
        {
            Assert.NotNullOrEmpty(key, nameof(key));
            
            return (T) Get(key);
        }
        public object Get(string key)
        {
            Assert.NotNullOrEmpty(key, nameof(key));
            
            TypedProperty tp = _values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new TypedPropertyException("Cant find item with key " + key);
            return _values[tp];
        }
        public object Get(TypedProperty key)
        {
            Assert.NotNull(key, nameof(key));
            
            return _values[key];
        }

        public void Set(string key, object value)
        {
            Assert.NotNull(key, nameof(key));
            
            TypedProperty tp = _values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new TypedPropertyException("Cant find item with key " + key);
            tp.Validate(value);

            _values[tp] = value;
        }
        public void Set(TypedProperty key, object value)
        {
            Assert.NotNull(key, nameof(key));
            
            key.Validate(value);

            _values[key] = value;
        }

        public void Clear()
        {
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            _values.Clear();
        }

        public bool Contains(KeyValuePair<TypedProperty, object> item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(KeyValuePair<TypedProperty, object>[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool Remove(TypedProperty key)
        {
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            return Remove(key);
        }
        public bool Remove(KeyValuePair<TypedProperty, object> item)
        {
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            return _values.Remove(item);
        }

        public int Count => _values.Count;
        public bool IsReadOnly => _readOnly;

        private void SetReadOnly()
        {
            _readOnly = true;
        }

        public TypedProperties ToReadOnly()
        {
            TypedProperties ret = new TypedProperties(_values);
            ret.SetReadOnly();
            return ret;
        }

        public TypedProperties Add<T>(String name, T value,
            String description = null, params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNull(value, nameof(value));
            
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            
            Add(new TypedProperty(name, typeof(T), description, assertConditions), value);

            return this;
        }
        public TypedProperties Add<T>(String name, String description = null
            , params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty(name, nameof(name));

            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            
            Add(new TypedProperty(name, typeof(T), description, assertConditions), default(T));

            return this;
        }
        
        public TypedProperties Add(String name, Type type, String description=null
            , params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNull(type, nameof(type));
            
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            
            Add(new TypedProperty(name, type, description, assertConditions), type.GetDefault());

            return this;
        }
        public TypedProperties Add(String name, Type type, Object value, String description=null
            , params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNull(type, nameof(type));
            
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            
            Add(new TypedProperty(name, type, description, assertConditions), value);

            return this;
        }
        public void Add(TypedProperty key, object value)
        {
            Assert.NotNull(key, nameof(key));
            
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            
            Add(key, value);
        }
        public TypedProperties Add(TypedProperty key)
        {
            Assert.NotNull(key, nameof(key));
            
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");
            Add(key, key.Type.GetDefault());

            return this;
        }
        public void Add(KeyValuePair<TypedProperty, object> item)
        {
            Assert.NotNull(item, nameof(item));
            
            if(this._readOnly)
                throw new TypedPropertyException("Property collection is readonly");

            _values.Add(item);
        }

        public bool TryGetValue(String key, out object value)
        {
            Assert.NotNullOrEmpty(key, nameof(key));
            
            value = null;
            var tp = _values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            
            return tp!=null && _values.TryGetValue(tp, out value);
        }
        public bool TryGetValue(TypedProperty key, out object value)
        {
            Assert.NotNull(key, nameof(key));
            
            return _values.TryGetValue(key, out value);
        }
        
        public ICollection<TypedProperty> Keys => _values.Keys;
        public ICollection<object> Values => _values.Values;
    }
}