using System.IO;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.DbProviders;

namespace DotNetAidLib.Database.Export
{
    public abstract class DataExportDriver
    {
        private DBProviderConnector dbProviderConnector;
        protected TypedProperties properties;

        public DataExportDriver()
        {
            properties = new TypedProperties();
        }

        public ReadOnlyTypedProperties Properties => properties;

        public abstract string Format { get; }

        public DBProviderConnector DbProviderConnector
        {
            get => dbProviderConnector;
            set
            {
                Assert.NotNull(value, nameof(value));
                dbProviderConnector = value;
            }
        }

        public abstract void Export(string command, Stream output);
    }
}