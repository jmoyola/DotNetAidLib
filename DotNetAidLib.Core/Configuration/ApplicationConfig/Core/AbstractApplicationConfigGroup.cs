using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Configuration;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Core{
	public abstract class AbstractApplicationConfigGroup : IApplicationConfigGroup
	{
        private IApplicationConfigGroup parent;

        protected AbstractApplicationConfigGroup(IApplicationConfigGroup parent) {
            this.parent = parent;
        }

        public IApplicationConfigGroup Parent
        {
            get { return this.parent; }
        }

        public IApplicationConfig Root {
            get {
                if (this.parent == null)
                    return (IApplicationConfig)this;
                else
                    return this.parent.Root;
            }
        }

        public abstract string GroupName { get; }
        public abstract string GroupInfo { get; set; }
        public abstract System.DateTime DateOfCreation { get; }
		public abstract System.DateTime DateOfModification { get; }

		public IApplicationConfigGroup AddGroup(string groupName)
		{
			return this.AddGroup(groupName, false);
		}

        public abstract IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists);

        public abstract System.Collections.Generic.IEnumerable<IApplicationConfigGroup> Groups { get; }
        public abstract System.Collections.Generic.IEnumerable<IConfig<Object>> Configurations { get; }
        public abstract bool GroupExist(string groupName);


		public abstract IApplicationConfigGroup GetGroup(string groupName);
		public void RemoveGroup(string groupName)
		{
			this.RemoveGroup(groupName, false);
		}
		public abstract void RemoveGroup(string groupName, bool ifExist);
		public abstract void RemoveAllGroups();

		public abstract System.Collections.Generic.Dictionary<string, System.Type> ConfigurationKeys { get; }
		public abstract bool ConfigurationExist(string key);
		public IConfig<object> AddConfiguration(string key, object value)
		{
			return this.AddConfiguration(key, value, false);
		}
		public IConfig<object> AddConfiguration(string key, object value, bool modifyIfExist)
		{
			return this.AddConfiguration<object>(key, value, modifyIfExist);
		}
        public IConfig<T> AddConfiguration<T>(string key, T Value)
        {
            return this.AddConfiguration<T>(key, Value, false);
        }
        public abstract IConfig<T> AddConfiguration<T>(string key, T value, bool modifyIfExist);
		public IConfig<object> GetConfiguration(string key)
		{
			return this.GetConfiguration<object>(key);
		}
		public IConfig<object> GetConfiguration(string key, object defaultValue)
		{
			return this.GetConfiguration<object>(key, defaultValue);
		}

		public abstract IConfig<T> GetConfiguration<T>(string key);

        public IConfig<T> GetConfiguration<T>(string key, T defaultValue)
        {
            if (!this.ConfigurationExist(key))
                return new DinamicConfig<T>(key, defaultValue);
            else
                return this.GetConfiguration<T>(key);
        }

        public IConfig<T> GetConfiguration<T>(string key, T defaultValue, bool createIfNotExist)
        {
            if (createIfNotExist && !this.ConfigurationExist(key))
                this.AddConfiguration<T>(key, defaultValue);

            return this.GetConfiguration<T>(key, defaultValue);
        }

        public object this[string key] {
			get { return this.GetConfiguration<object>(key).Value; }
			set { this.SetConfiguration<object>(key, value); }
		}
		public void RemoveConfiguration(string key)
		{
			this.RemoveConfiguration(key, false);
		}
		public abstract void RemoveConfiguration(string key, bool ifExist);
		public abstract void RemoveAllConfigurations();

		public IConfig<object> SetConfiguration(string key, object value, bool createIfNotExists)
		{
			return this.SetConfiguration<object>(key, value, createIfNotExists);
		}
		public IConfig<object> SetConfiguration(string key, object value)
		{
			return this.SetConfiguration<object>(key, value, false);
		}

        public abstract IConfig<T> SetConfiguration<T>(string key, T value, bool createIfNotExists);
		public IConfig<T> SetConfiguration<T>(string key, T value)
		{
			return this.SetConfiguration<T>(key, value, false);
		}

		public override string ToString()
		{
			return this.ToString(0);
		}

		public string ToString(int nivel)
		{
			string ret = "";

			ret = ret + new string(' ', nivel * 2) + "[" + this.GroupName + "]\r\n";

			foreach (IApplicationConfigGroup cg in this.Groups) {
				ret = ret + ((AbstractApplicationConfigGroup)cg).ToString(nivel + 1);
			}

			foreach (KeyValuePair<string, Type> config in this.ConfigurationKeys) {
				ret = ret + new string(' ', (nivel * 2) + 2) + "" + config.Key + " (" + config.Value.FullName + ") = ";
				object value = this.GetConfiguration(config.Key).Value;
				if ((value == null)) {
					ret = ret + ("NULL") + "\r\n";
				} else {
					ret = ret + value.ToString() + "\r\n";
				}
			}

			return ret;
		}

        public IDictionary<String, Object> ToProperties() {
            return this.ConfigurationKeys.Select(kv => kv.Key)
                .ToDictionary(k=>k,k=>this.GetConfiguration(k).Value);
        }
    }
}