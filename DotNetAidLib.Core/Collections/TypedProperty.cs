using System;
using System.Collections.Generic;
using System.Linq;
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
        private String name;
        private Type type;
        private IList<IAssertCondition> assertConditions= null;

        public TypedProperty(String name, Type type, IList<IAssertCondition> assertConditions=null)
        {
            Assert.NotNullOrEmpty( name, nameof(name));
            Assert.NotNull( type, nameof(type));

            this.name = name;
            this.type = type;
            this.assertConditions = assertConditions;
        }
        
        public String Name=>name;
        public Type Type=>type;

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj!=null && typeof(TypedProperty).IsAssignableFrom(obj.GetType())
                && ((TypedProperty)obj).name.Equals(this.name);
        }

        public override string ToString()
        {
            return this.name + "(" + this.type.Name + ")";
        }

        public Object Cast(String value)
        {
            return Convert.ChangeType(value, this.type);
        }

        public void Validate(Object value)
        {
            if(value != null && !this.type.IsAssignableFrom(value.GetType()))
                    throw new TypePropertyException("Value type '" + value.GetType().Name + "' is not valid type for property '" + this.name + "' (" + this.type.Name + ").");

            if (this.assertConditions != null)
            {
                foreach (IAssertCondition assertCondition in this.assertConditions)
                {
                    if(!assertCondition.IsValid(value))
                        throw new TypePropertyException("Value '" +
                                                    (value == null ? "[NULL]" : value.ToString()) + "' is not valid for property '" + this.name + "' (" + this.type.Name + "): Assert '" + assertCondition.GetType().Name + "' not passed.");    
                }
            }
        }
    }
}