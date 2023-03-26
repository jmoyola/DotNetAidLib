using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginInfoAttribute : Attribute
    {
        public PluginInfoAttribute(string name)
            : this(name, null, 0)
        {
        }

        public PluginInfoAttribute(string name, string description)
            : this(name, description, 0)
        {
        }

        public PluginInfoAttribute(string name, string description, int preferentOrder)
        {
            Assert.NotNullOrEmpty(name, nameof(name));

            Name = name;
            Description = description;
            PreferentOrder = preferentOrder;
        }


        public string Name { get; }

        public string Description { get; }

        public int PreferentOrder { get; }
    }
}