using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Plugins;

namespace DotNetAidLib.Database.Export
{
    public static class DataExport
    {
        private static IList<DataExportDriver> plugins;
        
        
        static DataExport() {
            plugins=Plugins.GetPluginsInstances<DataExportDriver>(new Object[0]);
        }

        public static void Export(Stream output, IDictionary<String, Object> parameters, DataExportCommand dataExportCommand)
        {
            Assert.NotNull( output, nameof(output));
            Assert.NotNull( parameters, nameof(parameters));
            Assert.NotNull( dataExportCommand, nameof(dataExportCommand));
            
            try
            {
                
                DBProviderConnector dbProviderConnector = new DBProviderConnector(dataExportCommand.DbProvider, dataExportCommand.ConnectionString);

                Export(output, dataExportCommand.Format, parameters, dbProviderConnector, dataExportCommand.SQLCommand);
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error exporting to " + dataExportCommand.Format + ".", ex);
            }
        }
        public static void Export(Stream output, String outputFormat, IDictionary<String, Object> parameters, DBProviderConnector dbProviderConnector, String sqlCommand)
        {
            Assert.NotNull( output, nameof(output));
            Assert.NotNullOrEmpty( outputFormat, nameof(outputFormat));
            Assert.NotNull( parameters, nameof(parameters));
            Assert.NotNull( dbProviderConnector, nameof(dbProviderConnector));
            Assert.NotNullOrEmpty( sqlCommand, nameof(sqlCommand));
            
            try
            {
                DataExportDriver dataExportDriver = plugins.FirstOrDefault(v =>
                    v.Format.Equals(outputFormat, StringComparison.InvariantCultureIgnoreCase));

                if (dataExportDriver == null)
                    throw new DataExportException("Cant find any driver for format '" + outputFormat + "'."
                                                  + " Available formats: " +
                                                  plugins.Select(v => v.Format).ToStringJoin(", "));
                
                dataExportDriver.DbProviderConnector = dbProviderConnector;

                parameters.Where(v=>dataExportDriver.Properties.ContainsKey(v.Key))
                    .ToList().ForEach(v => dataExportDriver.Properties[v.Key] = v.Value);
                    
                dataExportDriver.Export(sqlCommand, output);
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error exporting to " + outputFormat + ".", ex);
            }
        }
    }
}