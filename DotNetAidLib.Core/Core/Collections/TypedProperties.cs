using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Collections
{
    public class TypedProperties : ReadOnlyTypedProperties
    {
        public TypedProperties()
            : base(new Dictionary<TypedProperty, object>())
        {
        }

        public TypedProperties(IDictionary<TypedProperty, object> values)
            : base(values)
        {
        }

        public TypedProperties(IList<TypedProperty> values)
            : base(values)
        {
        }


        public TypedProperties Add<T>(string key, params IAssertCondition[] assertConditions)
        {
            Add(key, default(T), assertConditions);
            return this;
        }

        public TypedProperties Add<T>(string key, T value, params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty(key, nameof(key));

            var ret = new TypedProperty(key, typeof(T), assertConditions);
            ret.Validate(value);

            values.Add(ret, value);
            return this;
        }

        public TypedProperties Add(string key, Type type, params IAssertCondition[] assertConditions)
        {
            Add(key, type, type.GetDefault(), assertConditions);
            return this;
        }

        public TypedProperties Add(string key, Type type, object value, params IAssertCondition[] assertConditions)
        {
            Assert.NotNullOrEmpty(key, nameof(key));
            Assert.NotNull(type, nameof(type));

            var ret = new TypedProperty(key, type, assertConditions);
            ret.Validate(value);

            values.Add(ret, value);
            return this;
        }

        public TypedProperties Remove(string key)
        {
            var tp = values.Keys.FirstOrDefault(v => v.Name.Equals(key));
            if (tp == null)
                throw new Exception("Cant find item with key " + key);
            values.Remove(tp);
            return this;
        }

        public ReadOnlyTypedProperties ToReadOnly()
        {
            return new ReadOnlyTypedProperties(values);
        }
    }
}