using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Configuration.ApplicationConfig.Mem
{
    public class MemApplicationConfigGroup : AbstractApplicationConfigGroup
    {
        private readonly Dictionary<string, object> configs = new Dictionary<string, object>();
        private DateTime dateOfCreation;
        private DateTime dateOfModification;
        private string groupName;
        private readonly List<IApplicationConfigGroup> groups = new List<IApplicationConfigGroup>();

        protected MemApplicationConfigGroup(IApplicationConfigGroup parent)
            : base(parent)
        {
        }

        public override DateTime DateOfCreation => dateOfCreation;

        public override DateTime DateOfModification => dateOfModification;

        public override string GroupName => groupName;

        public override string GroupInfo { get; set; }

        public override IEnumerable<IApplicationConfigGroup> Groups => groups;

        public override IEnumerable<IConfig<object>> Configurations
        {
            get
            {
                var ret = new List<MemConfig<object>>();

                return configs.Select(kv => new MemConfig<object>(kv.Key, kv.Value));
            }
        }

        public override Dictionary<string, Type> ConfigurationKeys
        {
            get
            {
                var ret = new Dictionary<string, Type>();
                foreach (var config in configs) ret.Add(config.Key, config.Value.GetType().GetGenericArguments()[0]);
                return ret;
            }
        }

        public override IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists)
        {
            IApplicationConfigGroup g = null;

            if (groups.Any(v => v.GroupName == groupName))
            {
                if (!ifNotExists)
                    throw new ApplicationConfigException("Subgroup with name '" + groupName +
                                                         "' already exists in group '" + GroupName + "'.");
            }
            else
            {
                g = new MemApplicationConfigGroup(this)
                {
                    groupName = groupName,
                    dateOfCreation = DateTime.Now,
                    dateOfModification = DateTime.Now
                };
                groups.Add(g);
            }

            return g;
        }

        public override IApplicationConfigGroup GetGroup(string groupName)
        {
            var ret = groups.FirstOrDefault(v => v.GroupName == groupName);

            // Si no existe, error....
            if (ret == null)
                throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" +
                                                     GroupName + "'.");

            return ret;
        }

        public override void RemoveGroup(string groupName, bool ifExist)
        {
            var ret = groups.FirstOrDefault(v => v.GroupName == groupName);

            // Si no existe, error....
            if (ret == null)
            {
                if (!ifExist)
                    throw new ApplicationConfigException("Subgroup with name '" + groupName +
                                                         "' don't exists in group '" + GroupName + "'.");
            }
            else
            {
                groups.Remove(ret);
            }
        }

        public override void RemoveAllGroups()
        {
            groups.Clear();
        }

        public override bool GroupExist(string groupName)
        {
            var ret = groups.FirstOrDefault(v => v.GroupName == groupName);
            return ret != null;
        }

        public override bool ConfigurationExist(string key)
        {
            return configs.ContainsKey(key);
        }

        public override IConfig<T> GetConfiguration<T>(string key)
        {
            if (!configs.ContainsKey(key))
                throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" +
                                                     GroupName + "'.");

            var t = typeof(T);
            if (t.Equals(typeof(object)))
            {
                var pi = configs[key].GetType().GetProperty("Value");
                return (IConfig<T>) new MemConfig<object>(key, pi.GetValue(configs[key]));
            }

            return (IConfig<T>) configs[key];
        }

        //Public Overrides Function AddConfiguration(ByVal key As String, ByVal Value As Object, ByVal modifyIfExist As Boolean) As IConfig(Of Object)

        //End Function

        public override IConfig<T> AddConfiguration<T>(string key, T value, bool modifyIfExist)
        {
            IConfig<T> ret = null;
            // Si ya existe
            if (configs.ContainsKey(key))
            {
                // En caso de que se desee modificar, se elimina su contenido
                if (modifyIfExist)
                    configs.Remove(key);
                // Error en caso de que no se desee modificar
                else
                    throw new ApplicationConfigException("Configuration with key '" + key +
                                                         "' already exists in group '" + GroupName + "'.");
            }
            else
            {
                ret = new MemConfig<T>(key, value);
                configs.Add(key, ret);
            }

            return ret;
        }

        public override IConfig<T> SetConfiguration<T>(string key, T value, bool createIfNotExists)
        {
            IConfig<T> ret = null;
            // Si no existe la configuracion
            if (!configs.ContainsKey(key))
            {
                // Si no se crea si no existe, error....
                if (!createIfNotExists)
                    throw new ApplicationConfigException("Configuration with key '" + key +
                                                         "' don't exists in group '" + GroupName + "'.");
                // Si se crea si no existe, pues eso....
                ret = new MemConfig<T>(key, value);
                configs.Add(key, ret);
                // Si existe, se borra todo su contenido
            }
            else
            {
                ret = (IConfig<T>) configs[key];
                ret.Value = value;
            }

            return ret;
        }

        public override void RemoveConfiguration(string key, bool ifExist)
        {
            // Si no existe la configuracion
            if (!configs.ContainsKey(key))
            {
                // Si procede, error...
                if (!ifExist)
                    throw new ApplicationConfigException("Configuration with key '" + key +
                                                         "' don't exists in group '" + GroupName + "'.");
            }
            else
            {
                configs.Remove(key);
            }
        }

        public override void RemoveAllConfigurations()
        {
            configs.Clear();
        }
    }
}