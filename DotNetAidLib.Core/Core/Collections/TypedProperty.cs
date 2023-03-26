using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class TypePropertyException : Exception
    {
        public TypePropertyException(string message) : base(message)
        {
        }

        public TypePropertyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class TypedProperty
    {
        private readonly IList<IAssertCondition> assertConditions;

        public TypedProperty(string name, Type type, IList<IAssertCondition> assertConditions = null)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNull(type, nameof(type));

            Name = name;
            Type = type;
            this.assertConditions = assertConditions;
        }

        public string Name { get; }

        public Type Type { get; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && typeof(TypedProperty).IsAssignableFrom(obj.GetType())
                               && ((TypedProperty) obj).Name.Equals(Name);
        }

        public override string ToString()
        {
            return Name + "(" + Type.Name + ")";
        }

        public object Cast(string value)
        {
            return Convert.ChangeType(value, Type);
        }

        public void Validate(object value)
        {
            if (value != null && !Type.IsAssignableFrom(value.GetType()))
                throw new TypePropertyException("Value type '" + value.GetType().Name +
                                                "' is not valid type for property '" + Name + "' (" + Type.Name + ").");

            if (assertConditions != null)
                foreach (var assertCondition in assertConditions)
                    if (!assertCondition.IsValid(value))
                        throw new TypePropertyException("Value '" +
                                                        (value == null ? "[NULL]" : value.ToString()) +
                                                        "' is not valid for property '" + Name + "' (" + Type.Name +
                                                        "): Assert '" + assertCondition.GetType().Name +
                                                        "' not passed.");
        }
    }
}