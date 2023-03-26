using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public abstract class ParameterInfo
    {
        private readonly Func<string, object> castFunction;

        public ParameterInfo(string description, bool optional, string defaultValue)
            : this(description, optional, defaultValue, v => v)
        {
        }

        public ParameterInfo(string description, bool optional, string defaultValue, Func<string, object> castFunction)
        {
            Assert.NotNullOrEmpty(description, nameof(description));
            Assert.NotNull(castFunction, nameof(castFunction));

            Description = description;
            Optional = optional;
            DefaultValue = defaultValue;
            this.castFunction = castFunction;
        }

        public abstract string Name { get; }
        public string Description { get; }

        public bool Optional { get; }

        public string DefaultValue { get; }

        internal object Cast(string value)
        {
            return castFunction.Invoke(value);
        }

        public override string ToString()
        {
            return Description;
        }
    }
}