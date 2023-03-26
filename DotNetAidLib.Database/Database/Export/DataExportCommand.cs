using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Xml.Serialization;
using DotNetAidLib.Database.DbProviders;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace DotNetAidLib.Database.Export
{
    public class DataExportCommand
    {
        private static readonly Regex kvRegex = new Regex(@"^\s*([^:\s]+)\s*:\s*(.+)?$");
        private IDictionary<string, string> _dbParams = new Dictionary<string, string>();
        private string _dbProvider;
        private string _description;
        private string _format = "csv";
        private string _id;
        private CData _sqlCommand;
        private string _toFile = "default";

        public DataExportCommand()
        {
            Id = "Example";
            Format = "csv";
            ToFile = "default";
        }

        public string Id
        {
            get => _id;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                Assert.When(value, v => v.IndexOfAny(Path.GetInvalidPathChars()) == -1,
                    "Id can't contains '" + Path.GetInvalidPathChars() + "' chars.", nameof(value));
                _id = value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _description = value;
            }
        }

        public string DbProvider
        {
            get => _dbProvider;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _dbProvider = value;
            }
        }

        public IDictionary<string, string> DbParams
        {
            get => _dbParams;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _dbParams = value;
            }
        }


        public CData SQLCommand
        {
            get => _sqlCommand;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _sqlCommand = value;
            }
        }

        public string Format
        {
            get => _format;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _format = value;
            }
        }

        public string ToFile
        {
            get => _toFile;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _toFile = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                try
                {
                    var f = DbProviderFactories.GetFactory(DbProvider);
                    var csb = f.CreateConnectionStringBuilder();
                    _dbParams.ToList().ForEach(kv => csb.Add(kv.Key, kv.Value));
                    return csb.ConnectionString;
                }
                catch (Exception ex)
                {
                    throw new Exception("DbProvider or DbParams are not valid: " + ex.Message, ex);
                }
            }
        }

        public void ToKeyValueFile(string path)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(path);
                sw.WriteLine("Id:" + Id);
                sw.WriteLine("Description:" + Description);
                sw.WriteLine("DbProvider:" + DbProvider);
                _dbParams.ToList().ForEach(kv => sw.WriteLine("DbParam" + kv.Key + ":" + kv.Value.ToString()));
                sw.WriteLine("Format:" + Format);
                sw.WriteLine("ToFile:" + ToFile);
                sw.WriteLine("SQLCommand:" + SQLCommand);
                sw.Flush();
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error writing to key value file.", ex);
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        public static DataExportCommand FromKeyValueFile(string path)
        {
            StreamReader sr = null;
            var ret = new DataExportCommand();
            try
            {
                sr = new StreamReader(path);
                var line = sr.ReadLine();
                while (line != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    var m = kvRegex.Match(line);
                    if (m.Success)
                    {
                        var k = m.Groups[1].Value;
                        var v = m.Groups[2].Value;
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
                if (sr != null)
                    sr.Close();
            }
        }

        public void ToXmlFile(string path)
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
                if (sw != null)
                    sw.Close();
            }
        }

        public static DataExportCommand FromXmlFile(string path)
        {
            XmlSerializer xml = null;
            StreamReader sr = null;
            try
            {
                xml = new XmlSerializer(typeof(DataExportCommand));
                sr = new StreamReader(path);
                return (DataExportCommand) xml.Deserialize(sr);
            }
            catch (Exception ex)
            {
                throw new DataExportException("Error reading from xml file.", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }
    }
}