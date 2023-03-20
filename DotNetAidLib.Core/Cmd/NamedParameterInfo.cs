using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class NamedParameterInfo:ParameterInfo
    {
        private String name;

        public NamedParameterInfo(String name, String description)
            :this(name, description, false, null, v=>v){}
        public NamedParameterInfo(String name, String description, bool optional, String defaultValue)
            :this(name, description, optional, defaultValue, v=>v) { }
        public NamedParameterInfo(String name, String description, bool optional, String defaultValue, Func<String, Object> castFunction)
            :base(description, optional, defaultValue, castFunction)
        {
            Assert.NotNull ( name, nameof(name));

            this.name = name;
        }

        public override string Name { get => name;}

        public override string ToString ()
        {
            return (this.Optional?"[":"<") + this.name + (this.Optional ? "]" : ">") + "\t" 
                + base.ToString()
                + (this.Optional ? ", default: " + (this.DefaultValue==null?"null":this.DefaultValue.ToString()) : "");
        }
    }
}
