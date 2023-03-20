using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public abstract class ParameterInfo
    {
        private String description;
        private Func<String, Object> castFunction = null;
        private bool optional = false;
        private String defaultValue = null;

        public ParameterInfo(String description, bool optional, String defaultValue)
            :this(description, optional, defaultValue, v=>v)
         { }

        public ParameterInfo (String description, bool optional, String defaultValue, Func<String, Object> castFunction)
        {
            Assert.NotNullOrEmpty (description, nameof (description));
            Assert.NotNull(castFunction, nameof(castFunction));

            this.description = description;
            this.optional = optional;
            this.defaultValue=defaultValue;
            this.castFunction = castFunction;
        }

        public abstract string Name { get; }
        public string Description { get => description;}
        public bool Optional { get => optional;}
        public String DefaultValue { get => defaultValue; }

        internal Object Cast(String value) {
            return this.castFunction.Invoke(value);
        }

        public override string ToString ()
        {
            return this.description;
        }
    }
}
