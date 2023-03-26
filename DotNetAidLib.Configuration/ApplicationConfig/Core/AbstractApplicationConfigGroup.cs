using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core
{
    public abstract class AbstractApplicationConfigGroup : IApplicationConfigGroup
    {
        protected AbstractApplicationConfigGroup(IApplicationConfigGroup parent)
        {
            Parent = parent;
        }

        public object this[string key]
        {
            get => GetConfiguration<object>(key).Value;
            set => SetConfiguration<object>(key, value);
        }

        public IApplicationConfigGroup Parent { get; }

        public IApplicationConfig Root
        {
            get
            {
                if (Parent == null)
                    return (IApplicationConfig) this;
                return Parent.Root;
            }
        }

        public abstract string GroupName { get; }
        public abstract string GroupInfo { get; set; }
        public abstract DateTime DateOfCreation { get; }
        public abstract DateTime DateOfModification { get; }

        public IApplicationConfigGroup AddGroup(string groupName)
        {
            return AddGroup(groupName, false);
        }

        public abstract IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists);

        public abstract IEnumerable<IApplicationConfigGroup> Groups { get; }
        public abstract IEnumerable<IConfig<object>> Configurations { get; }
        public abstract bool GroupExist(string groupName);


        public abstract IApplicationConfigGroup GetGroup(string groupName);

        public void RemoveGroup(string groupName)
        {
            RemoveGroup(groupName, false);
        }

        public abstract void RemoveGroup(string groupName, bool ifExist);
        public abstract void RemoveAllGroups();

        public abstract Dictionary<string, Type> ConfigurationKeys { get; }
        public abstract bool ConfigurationExist(string key);

        public IConfig<object> AddConfiguration(string key, object value)
        {
            return AddConfiguration(key, value, false);
        }

        public IConfig<object> AddConfiguration(string key, object value, bool modifyIfExist)
        {
            return AddConfiguration<object>(key, value, modifyIfExist);
        }

        public IConfig<T> AddConfiguration<T>(string key, T Value)
        {
            return AddConfiguration(key, Value, false);
        }

        public abstract IConfig<T> AddConfiguration<T>(string key, T value, bool modifyIfExist);

        public IConfig<object> GetConfiguration(string key)
        {
            return GetConfiguration<object>(key);
        }

        public IConfig<object> GetConfiguration(string key, object defaultValue)
        {
            return GetConfiguration<object>(key, defaultValue);
        }

        public abstract IConfig<T> GetConfiguration<T>(string key);

        public IConfig<T> GetConfiguration<T>(string key, T defaultValue)
        {
            if (!ConfigurationExist(key))
                return new DinamicConfig<T>(key, defaultValue);
            return GetConfiguration<T>(key);
        }

        public IConfig<T> GetConfiguration<T>(string key, T defaultValue, bool createIfNotExist)
        {
            if (createIfNotExist && !ConfigurationExist(key))
                AddConfiguration(key, defaultValue);

            return GetConfiguration(key, defaultValue);
        }

        public abstract void RemoveConfiguration(string key, bool ifExist);
        public abstract void RemoveAllConfigurations();

        public IConfig<object> SetConfiguration(string key, object value, bool createIfNotExists)
        {
            return SetConfiguration<object>(key, value, createIfNotExists);
        }

        public IConfig<object> SetConfiguration(string key, object value)
        {
            return SetConfiguration<object>(key, value, false);
        }

        public abstract IConfig<T> SetConfiguration<T>(string key, T value, bool createIfNotExists);

        public IDictionary<string, object> ToProperties()
        {
            return ConfigurationKeys.Select(kv => kv.Key)
                .ToDictionary(k => k, k => GetConfiguration(k).Value);
        }

        public void RemoveConfiguration(string key)
        {
            RemoveConfiguration(key, false);
        }

        public IConfig<T> SetConfiguration<T>(string key, T value)
        {
            return SetConfiguration(key, value, false);
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int nivel)
        {
            var ret = "";

            ret = ret + new string(' ', nivel * 2) + "[" + GroupName + "]\r\n";

            foreach (var cg in Groups) ret = ret + ((AbstractApplicationConfigGroup) cg).ToString(nivel + 1);

            foreach (var config in ConfigurationKeys)
            {
                ret = ret + new string(' ', nivel * 2 + 2) + "" + config.Key + " (" + config.Value.FullName + ") = ";
                var value = GetConfiguration(config.Key).Value;
                if (value == null)
                    ret = ret + "NULL" + "\r\n";
                else
                    ret = ret + value + "\r\n";
            }

            return ret;
        }
    }
}