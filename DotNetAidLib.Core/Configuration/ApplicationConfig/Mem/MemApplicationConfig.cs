using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using System.IO;
using System.Globalization;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Mem{
    public class MemApplicationConfig : MemApplicationConfigGroup, IApplicationConfig
    {
        private List<Type> knownTypes = new List<Type>();
        private DateTime? lastSavedTime;
        public MemApplicationConfig()
            : base(null)
        {
            this.InitSettings();
        }

        public DateTime? LastSavedTime
        {
            get {
                return this.lastSavedTime;
            }
        }

        public List<Type> KnownTypes => this.knownTypes;

        public void Load()
        {
        }

        public void Save()
        {
            this.lastSavedTime = DateTime.Now;
        }

        private void InitSettings()
        {
        }

    }
}