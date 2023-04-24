using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    [Serializable]
    public class TypedPropertyException : Exception
    {
        public TypedPropertyException(string message) : base(message)
        {
        }

        public TypedPropertyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TypedPropertyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class TypedProperty
    {
        private readonly string _name;
        private readonly string _description;
        private readonly Type _type;
        private readonly IList<IAssertCondition> _assertConditions;
        

        public TypedProperty(string name, Type type, String description=null, IList<IAssertCondition> assertConditions = null)
        {
            _name = Assert.NotNullOrEmpty(name, nameof(name));
            _type= Assert.NotNull(type, nameof(type));
            _description = description;
            _assertConditions = assertConditions;
        }

        public string Name => _name;
        public Type Type => _type;
        public string Description => _description;
        public IList<IAssertCondition> AssertConditions => _assertConditions;
        
        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is TypedProperty
                    && ((TypedProperty) obj)._name.Equals(_name));
        }

        public override string ToString()
        {
            return _name + " (" + _type.Name + ")" + (_description==null?"":": " + _description);
        }

        public object Cast(string value)
        {
            return Convert.ChangeType(value, Type);
        }

        public void Validate(object value)
        {
            if (!_type.IsInstanceOfType(value))
                throw new TypedPropertyException("Value type '" + value.GetType().Name +
                                                "' is not valid type for property '" + Name + "' (" + Type.Name + ").");

            if (_assertConditions != null)
                foreach (var assertCondition in _assertConditions)
                    if (!assertCondition.IsValid(value))
                        throw new TypedPropertyException("Value '" +
                                                        (value == null ? "[NULL]" : value.ToString()) +
                                                        "' is not valid for property '" + Name + "' (" + Type.Name +
                                                        "): Assert '" + assertCondition.GetType().Name +
                                                        "' not passed.");
        }
    }
}