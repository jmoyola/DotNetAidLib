using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.Export
{
    public class DataExportDriverCsv:DataExportDriver
    {
        public DataExportDriverCsv()
            : base()
        {
            this.properties
                .Add<CultureInfo>("culture", CultureInfo.CurrentCulture, AssertCondition.IsNotNull)
                .Add<Encoding>("encoding", Encoding.Default, AssertCondition.IsNotNull)
                .Add<bool>("includeHeaders", true)
                .Add<char>("fieldSeparator", ',')
                .Add<String>("recordSeparator",Environment.NewLine);
        }
        
        public override String Format=>"csv";

        public override void Export(String command, Stream output)
        {
            Assert.NotNullOrEmpty( command, nameof(command));
            Assert.NotNull( output, nameof(output));

            IDbConnection cnx=null;

            StreamWriter sw = new StreamWriter(output, this.properties.Get<Encoding>("encoding"));
            try
            {
                cnx=this.DbProviderConnector.CreateConnection();
                cnx.Open();
                IDbCommand cmd = cnx.CreateCommand();
                cmd.CommandText = command;
                IDataReader dr = cmd.ExecuteReader();
                dr.ToCSV(sw,
                    this.properties.Get<CultureInfo>("culture"),
                    this.properties.Get<bool>("includeHeaders"),
                    this.properties.Get<char>("fieldSeparator"),
                    this.properties.Get<String>("recordSeparator")
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
                if(cnx!=null && cnx.State!= ConnectionState.Closed)
                    cnx.Close();
            }
        }

    }
}