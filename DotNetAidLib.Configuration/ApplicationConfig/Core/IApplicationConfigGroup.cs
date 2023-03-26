﻿using System;
using System.Collections.Generic;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core
{
    public interface IApplicationConfigGroup
    {
        IApplicationConfigGroup Parent { get; }
        IApplicationConfig Root { get; }

        DateTime DateOfCreation { get; }
        DateTime DateOfModification { get; }

        string GroupName { get; }
        string GroupInfo { get; set; }
        IEnumerable<IApplicationConfigGroup> Groups { get; }
        IEnumerable<IConfig<object>> Configurations { get; }
        Dictionary<string, Type> ConfigurationKeys { get; }
        bool GroupExist(string groupName);
        IApplicationConfigGroup AddGroup(string groupName);
        IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists);
        IApplicationConfigGroup GetGroup(string groupName);
        void RemoveGroup(string groupName, bool ifExist);
        void RemoveGroup(string groupName);

        void RemoveAllGroups();
        bool ConfigurationExist(string key);
        IConfig<T> AddConfiguration<T>(string key, T Value, bool modifyIfExist);
        IConfig<T> AddConfiguration<T>(string key, T Value);
        IConfig<T> GetConfiguration<T>(string key);
        IConfig<T> GetConfiguration<T>(string key, T defaultValue);
        IConfig<T> GetConfiguration<T>(string key, T defaultValue, bool createIfNotExists);
        IConfig<T> SetConfiguration<T>(string key, T Value, bool createIfNotExists);
        IConfig<object> AddConfiguration(string key, object Value, bool modifyIfExist);
        IConfig<object> AddConfiguration(string key, object Value);
        IConfig<object> GetConfiguration(string key);
        IConfig<object> GetConfiguration(string key, object defaultValue);
        IConfig<object> SetConfiguration(string key, object Value, bool createIfNotExists);
        IConfig<object> SetConfiguration(string key, object Value);
        void RemoveConfiguration(string key, bool ifExist);

        void RemoveAllConfigurations();

        IDictionary<string, object> ToProperties();
    }
}