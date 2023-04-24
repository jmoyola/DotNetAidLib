using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.Export
{
    public class DataExportDriverCsv : DataExportDriver
    {
        private readonly TypedProperties _properties;
        public DataExportDriverCsv()
        {
            _properties=new TypedProperties()
                .Add("culture", CultureInfo.CurrentCulture, "Culture to export", AssertCondition.IsNotNull)
                .Add("encoding", Encoding.Default, "Encoding to export", AssertCondition.IsNotNull)
                .Add("includeHeaders", true, "Include headers in export")
                .Add("fieldSeparator", ',', "field separator in export")
                .Add("recordSeparator", Environment.NewLine, "Record separator in export")
                .ToReadOnly();
        }

        public override TypedProperties Properties => _properties;
        public override string Format => "csv";

        public override void Export(string command, Stream output)
        {
            Assert.NotNullOrEmpty(command, nameof(command));
            Assert.NotNull(output, nameof(output));

            IDbConnection cnx = null;

            var sw = new StreamWriter(output, _properties.Get<Encoding>("encoding"));
            try
            {
                cnx = DbProviderConnector.CreateConnection();
                cnx.Open();
                var cmd = cnx.CreateCommand();
                cmd.CommandText = command;
                var dr = cmd.ExecuteReader();
                dr.ToCSV(sw,
                    _properties.Get<CultureInfo>("culture"),
                    _properties.Get<bool>("includeHeaders"),
                    _properties.Get<char>("fieldSeparator"),
                    _properties.Get<string>("recordSeparator")
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