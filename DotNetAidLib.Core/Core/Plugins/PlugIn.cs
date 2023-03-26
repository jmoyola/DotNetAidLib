using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Plugins
{
    public class PlugIn<T>
    {
        private readonly PluginInfoAttribute defaultPluginInfo;

        public PlugIn(Type plugInType)
        {
            Assert.NotNull(plugInType, nameof(plugInType));

            if (!typeof(T).IsAssignableFrom(plugInType))
                throw new ArgumentException("'" + plugInType.Name + "' is not plugin of '" + typeof(T).Name + "'",
                    nameof(plugInType));

            PlugInType = plugInType;
            defaultPluginInfo = new PluginInfoAttribute(PlugInType.FullName);
        }

        public Type PlugInType { get; }

        public PluginInfoAttribute Info
        {
            get
            {
                PluginInfoAttribute ret = null;
                ret = (PluginInfoAttribute) PlugInType.GetCustomAttributes(typeof(PluginInfoAttribute), true)
                    .FirstOrDefault();
                if (ret == null)
                    ret = defaultPluginInfo;
                return ret;
            }
        }

        public IEnumerable<PluginPropertyAttribute> Properties => PlugInType
            .GetCustomAttributes(typeof(PluginPropertyAttribute), true).Cast<PluginPropertyAttribute>();

        public IDictionary<string, object> PropertiesKeyValues
        {
            get { return Properties.ToDictionary(k => k.Key, v => v.Value); }
        }

        public IEnumerable<ParameterInfo[]> Constructors
        {
            get
            {
                var ret = new List<ParameterInfo[]>();
                foreach (var ci in PlugInType.GetConstructors())
                    ret.Add(ci.GetParameters());

                return ret;
            }
        }

        public T Instance()
        {
            return Instance(new object[] { });
        }

        public T Instance(object[] constructorParameters)
        {
            var constructorTypes = Type.GetTypeArray(constructorParameters);
            if (PlugInType.GetConstructor(constructorTypes) == null)
                throw new PluginException("Type '" + PlugInType.Name
                                                   + "' don't have a constructor with ("
                                                   + constructorTypes.Select(v => v.Name).ToStringJoin(", ") +
                                                   ") arguments.");
            return (T) Activator.CreateInstance(PlugInType, constructorParameters);
        }
    }
}