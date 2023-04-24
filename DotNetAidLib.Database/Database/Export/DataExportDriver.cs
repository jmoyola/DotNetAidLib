using System.IO;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.DbProviders;

namespace DotNetAidLib.Database.Export
{
    public abstract class DataExportDriver
    {
        private DBProviderConnector _dbProviderConnector;

        public abstract TypedProperties Properties { get; }

        public abstract string Format { get; }

        public DBProviderConnector DbProviderConnector
        {
            get => _dbProviderConnector;
            set
            {
                Assert.NotNull(value, nameof(value));
                _dbProviderConnector = value;
            }
        }

        public abstract void Export(string command, Stream output);
    }
}