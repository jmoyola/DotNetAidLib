using System;
using System.Globalization;
using System.IO;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.Export
{
    public abstract class DataExportDriver
    {
        protected TypedProperties properties;
        private DBProviderConnector dbProviderConnector;

        public DataExportDriver()
        {
            this.properties = new TypedProperties();

        }

        public ReadOnlyTypedProperties Properties => this.properties;
        
        public abstract String Format { get; }
        public DBProviderConnector DbProviderConnector
        {
            get => dbProviderConnector;
            set
            {
                Assert.NotNull( value, nameof(value));
                dbProviderConnector = value;
            }
        }

        public abstract void Export(String command, Stream output);
    }
}