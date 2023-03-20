using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Globalization;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Mem{
    public class MemApplicationConfigGroup : AbstractApplicationConfigGroup
	{
        private DateTime dateOfCreation;
        private DateTime dateOfModification;
        private string groupName;
        private string groupInfo;
        private List<IApplicationConfigGroup> groups=new List<IApplicationConfigGroup>();
        private Dictionary<String, Object> configs = new Dictionary<String, Object>();

        protected MemApplicationConfigGroup(IApplicationConfigGroup parent)
            :base(parent)
		{
		}

		public override DateTime DateOfCreation {
			get { return this.dateOfCreation; }
		}

		public override DateTime DateOfModification {
			get { return this.dateOfModification; }
		}

		public override string GroupName {
			get { return this.groupName; }
		}

        public override string GroupInfo
        {
            get { return this.groupInfo; }
            set { this.groupInfo = value; }

        }

        public override IEnumerable<IApplicationConfigGroup> Groups {
			get { return this.groups; }
		}

        public override System.Collections.Generic.IEnumerable<IConfig<Object>> Configurations
        {
            get
            {
                List<MemConfig<Object>> ret = new List<MemConfig<Object>>();

                return this.configs.Select(kv=> new MemConfig<Object>(kv.Key, kv.Value));
            }
        }

        public override IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists)
		{
            IApplicationConfigGroup g = null;

            if (this.groups.Any(v => v.GroupName == groupName))
            {
                if (!ifNotExists)
                    throw new ApplicationConfigException("Subgroup with name '" + groupName + "' already exists in group '" + this.GroupName + "'.");
            }
            else
            {
                g = new MemApplicationConfigGroup(this)
                {
                    groupName = groupName,
                    dateOfCreation = DateTime.Now,
                    dateOfModification = DateTime.Now
                };
                this.groups.Add(g);
            }

			return g;
		}

		public override IApplicationConfigGroup GetGroup(string groupName)
		{
            IApplicationConfigGroup ret= this.groups.FirstOrDefault(v=>v.GroupName==groupName);

			// Si no existe, error....
			if (ret == null) {
				throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" + this.GroupName + "'.");
			}

			return ret;
		}

		public override void RemoveGroup(string groupName, bool ifExist)
		{
            IApplicationConfigGroup ret = this.groups.FirstOrDefault(v => v.GroupName == groupName);

			// Si no existe, error....
			if (ret == null) {
				if (!ifExist) {
					throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" + this.GroupName + "'.");
				}
			} else {
				this.groups.Remove(ret);
			}
		}

		public override void RemoveAllGroups()
		{
            this.groups.Clear();
		}

		public override bool GroupExist(string groupName)
		{
            IApplicationConfigGroup ret = this.groups.FirstOrDefault(v => v.GroupName == groupName);
			return (ret != null);
		}

		public override Dictionary<string, Type> ConfigurationKeys {
			get {
				Dictionary<string, Type> ret = new Dictionary<string, Type>();
				foreach (KeyValuePair<String, Object> config in configs) {
					ret.Add(config.Key, config.Value.GetType().GetGenericArguments()[0]);
				}
				return ret;
			}
		}

		public override bool ConfigurationExist(string key)
		{
			return this.configs.ContainsKey(key);
		}

		public override IConfig<T> GetConfiguration<T>(string key)
		{
			if (!this.configs.ContainsKey(key)) {
				throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
			} else {
                Type t = typeof(T);
                if (t.Equals(typeof(object))) {
                    var pi = this.configs[key].GetType().GetProperty("Value");
                    return (IConfig <T>) new MemConfig<Object>(key, pi.GetValue(this.configs[key]));
                }
                else
                    return (IConfig<T>)this.configs[key];
			}
		}

		//Public Overrides Function AddConfiguration(ByVal key As String, ByVal Value As Object, ByVal modifyIfExist As Boolean) As IConfig(Of Object)

		//End Function

		public override IConfig<T> AddConfiguration<T>(string key, T value, bool modifyIfExist)
		{
            IConfig<T> ret = null;
            // Si ya existe
            if (this.configs.ContainsKey(key)) {
				// En caso de que se desee modificar, se elimina su contenido
				if ((modifyIfExist)) {
                    this.configs.Remove(key);
					// Error en caso de que no se desee modificar
				} else {
					throw new ApplicationConfigException("Configuration with key '" + key + "' already exists in group '" + this.GroupName + "'.");
				}
			} else {
                ret = new MemConfig<T>(key, value);
                this.configs.Add(key, ret);
			}

			return ret;
		}

		public override IConfig<T> SetConfiguration<T>(string key, T value, bool createIfNotExists)
		{
            IConfig<T> ret = null;
            // Si no existe la configuracion
            if (!this.configs.ContainsKey(key)) {
				// Si no se crea si no existe, error....
				if ((!createIfNotExists)) {
					throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
					// Si se crea si no existe, pues eso....
				} else {
                    ret = new MemConfig<T>(key, value);
                    this.configs.Add(key, ret);
				}
				// Si existe, se borra todo su contenido
			} else {
                ret = (IConfig <T> )this.configs[key];
                ret.Value = value;
			}

			return ret;
		}

		public override void RemoveConfiguration(string key, bool ifExist)
		{
			// Si no existe la configuracion
			if (!this.configs.ContainsKey(key)) {
				// Si procede, error...
				if ((!ifExist)) {
					throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
				}
			} else {
                this.configs.Remove(key);
			}
		}

		public override void RemoveAllConfigurations()
		{
            this.configs.Clear();
		}
	}
}