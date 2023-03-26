using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.Export
{
    public class DataExportDriverCsv : DataExportDriver
    {
        public DataExportDriverCsv()
        {
            properties
                .Add("culture", CultureInfo.CurrentCulture, AssertCondition.IsNotNull)
                .Add("encoding", Encoding.Default, AssertCondition.IsNotNull)
                .Add("includeHeaders", true)
                .Add("fieldSeparator", ',')
                .Add("recordSeparator", Environment.NewLine);
        }

        public override string Format => "csv";

        public override void Export(string command, Stream output)
        {
            Assert.NotNullOrEmpty(command, nameof(command));
            Assert.NotNull(output, nameof(output));

            IDbConnection cnx = null;

            var sw = new StreamWriter(output, properties.Get<Encoding>("encoding"));
            try
            {
                cnx = DbProviderConnector.CreateConnection();
                cnx.Open();
                var cmd = cnx.CreateCommand();
                cmd.CommandText = command;
                var dr = cmd.ExecuteReader();
                dr.ToCSV(sw,
                    properties.Get<CultureInfo>("culture"),
                    properties.Get<bool>("includeHeaders"),
                    properties.Get<char>("fieldSeparator"),
                    properties.Get<string>("recordSeparator")
                );

                sw.Flush();
                output.Flush();
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error exporting data.", ex);
            }
            finally
            {
                if (cnx != null && cnx.State != ConnectionState.Closed)
                    cnx.Close();
            }
        }
    }
}