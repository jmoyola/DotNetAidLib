using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Xml.Serialization;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace DotNetAidLib.Database.Export
{
    public class DataExportCommand
    {
        private string _id;
        private string _description;
        private string _dbProvider;
        private IDictionary<string, String> _dbParams=new Dictionary<string, String>();
        private CData _sqlCommand;
        private string _format = "csv";
        private String _toFile="default";

        public DataExportCommand()
        {
            this.Id = "Example";
            this.Format = "csv";
            this.ToFile = "default";
        }
        public String Id
        {
            get => _id;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                Assert.When(value, v=>v.IndexOfAny(Path.GetInvalidPathChars())==-1, "Id can't contains '" + Path.GetInvalidPathChars()+"' chars.", nameof(value));
                _id = value;
            }
        }

        public String Description
        {
            get => _description;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _description = value;
            }
        }

        public String DbProvider
        {
            get => _dbProvider;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _dbProvider = value;
            }
        }

        public IDictionary<string, String> DbParams
        {
            get => _dbParams;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _dbParams = value;
            }
        }

        
        
        public CData SQLCommand
        {
            get => _sqlCommand;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _sqlCommand = value;
            }
        }

        public string Format
        {
            get => _format;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                _format = value;
            }
        }
        
        public String ToFile
        {
            get => this._toFile;
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                this._toFile = value;
            }
        }

        public String ConnectionString
        {
            get
            {
                try
                {
                    var f = DbProviderFactories.GetFactory(this.DbProvider, true);
                    var csb = f.CreateConnectionStringBuilder();
                    this._dbParams.ToList().ForEach(kv => csb.Add(kv.Key, kv.Value));
                    return csb.ConnectionString;
                }
                catch (Exception ex)
                {
                    throw new Exception("DbProvider or DbParams are not valid: " + ex.Message, ex);
                }
            }
        }
        
        public void ToKeyValueFile(String path)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(path);
                sw.WriteLine("Id:" + this.Id);
                sw.WriteLine("Description:" + this.Description);
                sw.WriteLine("DbProvider:" + this.DbProvider);
                this._dbParams.ToList().ForEach(kv=>sw.WriteLine("DbParam" + kv.Key +":" + kv.Value.ToString()));
                sw.WriteLine("Format:" + this.Format);
                sw.WriteLine("ToFile:" + this.ToFile);
                sw.WriteLine("SQLCommand:" + this.SQLCommand);
                sw.Flush();
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error writing to key value file.", ex);
            }
            finally
            {
                if(sw!=null)
                    sw.Close();
            }
        }
        
        private static Regex kvRegex = new Regex(@"^\s*([^:\s]+)\s*:\s*(.+)?$");
        public static DataExportCommand FromKeyValueFile(String path)
        {
            StreamReader sr = null;
            DataExportCommand ret = new DataExportCommand();
            try
            {
                sr = new StreamReader(path);
                String line = sr.ReadLine();
                while (line != null)
                {
                    if(String.IsNullOrEmpty(line))
                        continue;
                    Match m = kvRegex.Match(line);
                    if (m.Success)
                    {
                        String k = m.Groups[1].Value;
                        String v = m.Groups[2].Value;
                        if (k.Equals("Id"))
                            ret.Id = v;
                        else if (k.Equals("Description"))
                            ret.Description = v;
                        else if (k.Equals("DbProvider"))
                            ret.DbProvider = v;
                        else if (k.StartsWith("DbParam"))
                            ret._dbParams.Add(k.Substring(7), v);
                        else if (k.Equals("Format"))
                            ret.Format = v;
                        else if (k.Equals("ToFile"))
                            ret.ToFile = v;
                        else if (k.Equals("SQLCommand"))
                            ret.SQLCommand = v;
                    }
                    
                    line = sr.ReadLine();
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error reading from key value file.", ex);
            }
            finally
            {
                if(sr!=null)
                    sr.Close();
            }
        }

        public void ToXmlFile(String path)
        {
            XmlSerializer xml = null;
            StreamWriter sw = null;
            try
            {
                xml = new XmlSerializer(typeof(DataExportCommand));
                sw = new StreamWriter(path);
                xml.Serialize(sw, this);
                sw.Flush();
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error writing to xml file.", ex);
            }
            finally
            {
                if(sw!=null)
                    sw.Close();
            }
        }
        
        public static DataExportCommand FromXmlFile(String path)
        {
            XmlSerializer xml = null;
            StreamReader sr = null;
            try
            {
                xml = new XmlSerializer(typeof(DataExportCommand));
                sr = new StreamReader(path);
                return (DataExportCommand)xml.Deserialize(sr);
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error reading from xml file.", ex);
            }
            finally
            {
                if(sr!=null)
                    sr.Close();
            }
        }
    }
}