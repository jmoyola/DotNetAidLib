using System;

namespace DotNetAidLib.Core.Plugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PluginPropertyAttribute : Attribute
    {
        public PluginPropertyAttribute(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            Key = key;
            Value = value;
        }

        public string Key { get; }

        public object Value { get; }
    }
}