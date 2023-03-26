using System;
using System.Collections.Generic;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Configuration.ApplicationConfig.Mem
{
    public class MemApplicationConfig : MemApplicationConfigGroup, IApplicationConfig
    {
        public MemApplicationConfig()
            : base(null)
        {
            InitSettings();
        }

        public DateTime? LastSavedTime { get; private set; }

        public List<Type> KnownTypes { get; } = new List<Type>();

        public void Load()
        {
        }

        public void Save()
        {
            LastSavedTime = DateTime.Now;
        }

        private void InitSettings()
        {
        }
    }
}