using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class NamedParameterInfo : ParameterInfo
    {
        private readonly string name;

        public NamedParameterInfo(string name, string description)
            : this(name, description, false, null, v => v)
        {
        }

        public NamedParameterInfo(string name, string description, bool optional, string defaultValue)
            : this(name, description, optional, defaultValue, v => v)
        {
        }

        public NamedParameterInfo(string name, string description, bool optional, string defaultValue,
            Func<string, object> castFunction)
            : base(description, optional, defaultValue, castFunction)
        {
            Assert.NotNull(name, nameof(name));

            this.name = name;
        }

        public override string Name => name;

        public override string ToString()
        {
            return (Optional ? "[" : "<") + name + (Optional ? "]" : ">") + "\t"
                   + base.ToString()
                   + (Optional ? ", default: " + (DefaultValue == null ? "null" : DefaultValue) : "");
        }
    }
}