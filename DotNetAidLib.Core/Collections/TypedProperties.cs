using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Collections
{
    public class TypedProperties:ReadOnlyTypedProperties
    {
        public TypedProperties()
            :base(new Dictionary<TypedProperty, Object>())
        { }
        
        public TypedProperties(IDictionary<TypedProperty, Object> values)
        :base(values)
        { }

        public TypedProperties(IList<TypedProperty> values)
            : base(values) { }


        public TypedProperties Add<T>(String key, params IAssertCondition[] assertConditions)
        {
            this.Add<T>(key, default(T), assertConditions);
            return this;
        }

        public TypedProperties Add<T>(String key, T value, params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty( key, nameof(key));

            TypedProperty ret=new TypedProperty(key, typeof(T), assertConditions);
            ret.Validate(value);
            
            this.values.Add(ret, value);
            return this;
        }

        public TypedProperties Add(String key, Type type, params IAssertCondition[] assertConditions)
        {
            this.Add(key, type, type.GetDefault(), assertConditions);
            return this;
        }

        public TypedProperties Add(String key, Type type, Object value, params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty( key, nameof(key));
            Assert.NotNull( type, nameof(type));
            
            TypedProperty ret=new TypedProperty(key, type, assertConditions);
            ret.Validate(value);
            
            this.values.Add(ret, value);
            return this;
        }

        public TypedProperties Remove(String key)
        {
            TypedProperty tp = this.values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);
            this.values.Remove(tp);
            return this;
        }

        public ReadOnlyTypedProperties ToReadOnly()
        {
            return new ReadOnlyTypedProperties(this.values);
        }
    }
    
}