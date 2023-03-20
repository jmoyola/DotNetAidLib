using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Collections
{
    public class ReadOnlyTypedProperties:IEnumerable<KeyValuePair<String, Object>>
    {
        protected IDictionary<TypedProperty, Object> values;

        public ReadOnlyTypedProperties(IDictionary<TypedProperty, Object> values)
        {
            Assert.NotNull( values, nameof(values));
            
            this.values = values;
        }

        public ReadOnlyTypedProperties(IList<TypedProperty> values)
        {
            Assert.NotNull( values, nameof(values));
            
            this.values = values.ToDictionary(v=>v, v=>v.Type.GetDefault());
        }

        public T Get<T>(String key)
        {
            return (T) Get(key);
        }
        
        public bool ContainsKey(String key)
        {
            TypedProperty tp = this.values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            return tp!=null;
        }
        
        public Object Get(String key)
        {
            TypedProperty tp = this.values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);
            return this.values[tp];
        }

        public Object this[String key]
        {
            get => this.Get(key);
            set => this.Set(key, value);
        }

        public void Set(String key, Object value)
        {
            TypedProperty tp = this.values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);
            
            tp.Validate(value);
            
            this.values[tp]=value;
        }

        public Object Cast(String key, String value)
        {
            TypedProperty tp = this.values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);

            return tp.Cast(value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.values.ToDictionary(v => v.Key.Name, v => v.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
}