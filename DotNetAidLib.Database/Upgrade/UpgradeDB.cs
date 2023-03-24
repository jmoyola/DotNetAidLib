using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using DotNetAidLib.Core;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Database.SQL.Core;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Database.Upgrade
{
    public class UpgradeDB
    {
        public event UpgradeEventHandler UpgradeEvent;
        private String _SchemaVersionsName = "SchemaVersions";
        private String _SchemaUpgradesTableName = "SchemaUpgrades";

        private DBProviderConnector _dbProviderConnector;
        private UpgradeGroups _Upgrades = new UpgradeGroups();
        private ISQLParser _SQLParser;

        public UpgradeDB(DBProviderConnector dbProviderConnector)
        {
            if (dbProviderConnector == null)
                throw new UpgradeException("Connection parameter can't be null.");

            _SQLParser = SQLParserFactory.Instance(dbProviderConnector);

            if (_SQLParser == null)
                throw new UpgradeException("SqlParser not implemented..");

            _dbProviderConnector = dbProviderConnector;
        }

        public UpgradeDB(DBProviderConnector dbProviderConnector, Stream xmlInputStream)
            : this(dbProviderConnector)
        {
            _Upgrades = LoadFromXml(xmlInputStream);
        }

        public UpgradeDB(DBProviderConnector dbProviderConnector, FileInfo xmlInputFile)
            : this(dbProviderConnector)
        {
            _Upgrades = LoadFromXml(xmlInputFile);
        }

        public UpgradeDB(DBProviderConnector dbProviderConnector, DirectoryInfo baseFolder, String fileNameFilter)
            : this(dbProviderConnector)
        {
            _Upgrades = LoadFromXml(baseFolder, fileNameFilter);
        }

        public UpgradeDB(DBProviderConnector dbProviderConnector, Assembly assembly, String resourcePath)
            : this(dbProviderConnector)
        {
            _Upgrades = LoadFromXml(assembly, resourcePath);
        }

        public UpgradeDB(DBProviderConnector dbProviderConnector, Assembly assembly, String resourceFolder, String fileNameFilter)
            : this(dbProviderConnector)
        {
            _Upgrades = LoadFromXml(assembly, resourceFolder, fileNameFilter);
        }

        public String SchemaUpgradesTableName
        {
            get
            {
                return _SchemaUpgradesTableName;
            }
            set
            {
                _SchemaUpgradesTableName = value;
            }
        }

        public String SchemaVersionsName
        {
            get
            {
                return _SchemaVersionsName;
            }
            set
            {
                _SchemaVersionsName = value;
            }
        }

        public UpgradeGroups Upgrades
        {
            get
            {
                if (_Upgrades == null)
                    _Upgrades = new UpgradeGroups();
                return _Upgrades;
            }
        }

        protected void OnUpgradeEvent(UpgradeEventArgs args)
        {
            if (UpgradeEvent != null)
                UpgradeEvent(this, args);
        }

        private void Init()
        {
            IDbConnection cnx = null;

            try
            {
                DataSet ds = new DataSet(this.SchemaVersionsName);
                DataTable t = new DataTable(this.SchemaUpgradesTableName);

                DataColumn dc = null;

                // Columna Version
                dc = new DataColumn("SchemaName", typeof(String));
                dc.AllowDBNull = false;
                dc.MaxLength = 30;
                t.Columns.Add(dc);

                // Columna Version
                dc = new DataColumn("Version", typeof(String));
                dc.AllowDBNull = false;
                dc.MaxLength = 30;
                t.Columns.Add(dc);

                // Columna Date
                dc = new DataColumn("Date", typeof(DateTime));
                dc.AllowDBNull = false;
                t.Columns.Add(dc);

                // Columna Description
                dc = new DataColumn("Description", typeof(String));
                dc.AllowDBNull = true;
                dc.MaxLength = -1;
                t.Columns.Add(dc);

                // Columna Description
                dc = new DataColumn("Type", typeof(String));
                dc.AllowDBNull = false;
                dc.MaxLength = -1;
                t.Columns.Add(dc);

                // Columna ClientUpgradeDate
                dc = new DataColumn("ClientUpgradeDate", typeof(DateTime));
                dc.AllowDBNull = false;
                dc.DefaultValue = new DateTime(2000, 1, 1, 0, 0, 0, 0);
                t.Columns.Add(dc);

                // Columna ClientUpgradeVersion
                dc = new DataColumn("clientUpgradeInfo", typeof(String));
                dc.MaxLength = 50;
                dc.AllowDBNull = false;
                t.Columns.Add(dc);

                t.PrimaryKey = new DataColumn[] { t.Columns["SchemaName"], t.Columns["Version"] };

                ds.Tables.Add(t);

                cnx = _dbProviderConnector.CreateConnection();
                cnx.Open();
                foreach (String sentence in this.CreateSchemaSentences(ds))
                    cnx.CreateCommand().ExecuteNonQuery(sentence);
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error initializing database.", ex);
            }
            finally
            {
                if (cnx != null && !cnx.State.Equals(ConnectionState.Closed))
                    cnx.Close();
            }
        }

        private List<String> CreateSchemaSentences(DataSet ds)
        {
            List<String> ret = new List<String>();

            ret.Add(_SQLParser.CreateSchema(ds, true));
            ret.Add(_SQLParser.CreateTable(ds.Tables[this.SchemaUpgradesTableName], true));

            return ret;
        }

        public void UpgradeAsync()
        {
            UpgradeAsync(null);
        }

        public void UpgradeAsync(String clientUpgradeInfo)
        {
            Thread Upgrade_Th = new Thread(new ParameterizedThreadStart(Upgrade_ThreadStart));
            Upgrade_Th.Start(clientUpgradeInfo);
        }

        private void Upgrade_ThreadStart(Object clientUpgradeInfo)
        {
            this.UpgradeAllSchemas((String)clientUpgradeInfo, true);
        }

        public IEnumerable<UpgradeGroup> UpgradePending()
        {
            return this.UpgradeAllSchemas(false);
        }

        public IEnumerable<UpgradeGroup> UpgradeAllSchemas(bool makeChanges)
        {
            return UpgradeAllSchemas(null, makeChanges);
        }

        public IEnumerable<UpgradeGroup> UpgradeAllSchemas(string clientUpgradeInfo, bool makeChanges)
        {
            IDbConnection cnx = null;
            List<UpgradeGroup> ret = null;
            this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.BeginUpgrade));
            try
            {
                ret = new List<UpgradeGroup>();
                this.Init();
                cnx = _dbProviderConnector.CreateConnection();
                cnx.Open();

                cnx.CreateCommand().ExecuteNonQuery("LOCK TABLES `" + this.SchemaVersionsName + "`.`" + this.SchemaUpgradesTableName + "` WRITE");

                foreach (String schema in this.Upgrades.Select(v => v.SchemaName).Distinct())
                {
                    IEnumerable<UpgradeGroup> upgradesApply = pUpgradeSchema(cnx, schema, null, clientUpgradeInfo, makeChanges);
                    ret.AddRange(upgradesApply);
                }

                cnx.CreateCommand().ExecuteNonQuery("UNLOCK TABLES");
                this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.EndUpgrade));

                return ret;
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error upgrading database.", ex);
            }
            finally
            {
                if (cnx != null && !cnx.State.Equals(ConnectionState.Closed))
                    cnx.Close();
            }
        }

        public IEnumerable<UpgradeGroup> UpgradeSchema(String schemaName, bool makeChanges)
        {
            return this.UpgradeSchema(schemaName, null, null, makeChanges);
        }

        public IEnumerable<UpgradeGroup> UpgradeSchema(String schemaName, string clientUpgradeInfo, bool makeChanges)
        {
            return this.UpgradeSchema(schemaName, null, clientUpgradeInfo, makeChanges);
        }

        public IEnumerable<UpgradeGroup> UpgradeSchema(String schemaName, Version upgradeTo, string clientUpgradeInfo, bool makeChanges)
        {
            IDbConnection cnx = null;
            IEnumerable<UpgradeGroup> ret = null;
            this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.BeginUpgrade));
            try
            {
                this.Init();
                cnx = _dbProviderConnector.CreateConnection();
                cnx.Open();

                cnx.CreateCommand().ExecuteNonQuery("LOCK TABLES `" + this.SchemaVersionsName + "`.`" + this.SchemaUpgradesTableName + "` WRITE");

                ret = pUpgradeSchema(cnx, schemaName, upgradeTo, clientUpgradeInfo, makeChanges);

                cnx.CreateCommand().ExecuteNonQuery("UNLOCK TABLES");
                this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.EndUpgrade));

                return ret;
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error upgrading database.", ex);
            }
            finally
            {
                if (cnx != null && !cnx.State.Equals(ConnectionState.Closed))
                    cnx.Close();
            }
        }

        private string GetClientUpgradeInfo(string clientUpgradeInfo)
        {
            String ret = clientUpgradeInfo;

            if (String.IsNullOrEmpty(ret))
            {
                ret = "No Info (0.0.0.0)";
                Assembly entryAssembly = Helper.GetEntryAssembly();
                if (entryAssembly != null && entryAssembly.GetName() != null)
                {
                    ret = entryAssembly.GetName().Name + " (0.0.0.0)";
                    if (entryAssembly.GetName().Version != null)
                        ret = entryAssembly.GetName().Name + "(" + entryAssembly.GetName().Version.ToString() + ")";
                }
            }

            return ret;
        }

        private IEnumerable<UpgradeGroup> pUpgradeSchema(IDbConnection cnx, String schemaName, Version upgradeTo, string clientUpgradeInfo, bool makeChanges)
        {
            UpgradeVersion lastDbVersion = null;

            try
            {
                // Obtenemos la ultima actualización disponible y la ultima actualización realizada en db para el esquema
                UpgradeVersion lastUpgradeVersion = this.Upgrades
                    .Where(v => v.SchemaName.Equals(schemaName))
                    .Select(v => v.Version)
                    .OrderByDescending(v => v)
                    .FirstOrDefault();
                UpgradeVersion lastUpgradeVersionInDB = cnx.CreateCommand()
                    
                    .AddParameter   ("@schemaName", schemaName)
                    .ExecuteScalarColumn<String>("select Version from SchemaVersions.SchemaUpgrades where SchemaName=@schemaName;", 0)
                    .Select(v => UpgradeVersion.Parse(v.Value))
                    .OrderBy(v => v)
                    .LastOrDefault();
                // Si la ultima actualización disponible es menor a la ultima actualización realizada en db para el esquema
                // querrá decir que ya fué actualizada por un binario posterior y podría no ser compatible...
                if (lastUpgradeVersion != null && lastUpgradeVersionInDB != null
                    && lastUpgradeVersion < lastUpgradeVersionInDB)
                {
                    // Lanzamos evento Warning
                    UpgradeEventArgs uea = new UpgradeEventArgs(UpgradeEventType.DBAlreadyUpgradeWithMostRecentUpgraderWarning);
                    this.OnUpgradeEvent(uea);
                    if (uea.Cancel) //  y permitimos cancelar la actualización de ese esquema
                        return new List<UpgradeGroup>();
                }

                List<UpgradeVersion> dbVersions = cnx
                    .CreateCommand()
                    .AddParameter("@SchemaName", schemaName)
                    .ExecuteList<UpgradeVersion>("SELECT Version FROM `" + this.SchemaVersionsName + "`.`" + this.SchemaUpgradesTableName
                        + "` WHERE SchemaName=@SchemaName", v => UpgradeVersion.Parse(v.GetString(v.GetOrdinal("Version"))))
                    .OrderByDescending(v => v).ToList();

                lastDbVersion = dbVersions.FirstOrDefault();
                if (lastDbVersion == null)
                    lastDbVersion = new UpgradeVersion(0, 0);

                if (upgradeTo == null)
                    upgradeTo = lastUpgradeVersion;

                bool debugMode = EnvironmentHelper.IsCompiledInDebugMode();

                IEnumerable<UpgradeGroup> upgradesPending = this.Upgrades
                    .Where(
                        v => v.SchemaName == schemaName &&
                        (v.Version.CompareTo(lastDbVersion) > 0 && v.Version.CompareTo(upgradeTo) <= 0) &&
                        (v.Type == UpgradeType.Release || (v.Type == UpgradeType.Debug && debugMode))
                    ).OrderBy(v => v.Version);

                if (makeChanges)
                {
                    foreach (UpgradeGroup ug in upgradesPending)
                        UpgradeGroup(cnx, ug, clientUpgradeInfo);
                }

                return upgradesPending;
            }
            catch (Exception ex)
            {

                throw new UpgradeException("Error upgrading schema '" + schemaName + "': " + ex.Message, ex);
            }
        }

        private void UpgradeGroup(IDbConnection cnx, UpgradeGroup upgradeGroup, string clientUpgradeInfo)
        {
            IDbTransaction trx = null;
            UpgradeItem ui = null;
            int i = 0;


            try
            {
                this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.BeginUpgradeGroup, upgradeGroup));

                try
                { // Activamos el esquema (si existe, por lo que se mete entre trycatch)
                    cnx.CreateCommand()
                       .AddParameter("@SchemaName", upgradeGroup.SchemaName)
                       .ExecuteNonQuery(_SQLParser.ActiveSchema(upgradeGroup.SchemaName));
                }
                catch { }

                trx = cnx.BeginTransaction();
                for (i = 0; i < upgradeGroup.UpgradeItems.Count; i++)
                {
                    ui = upgradeGroup.UpgradeItems[i];
                    this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.BeginUpgradeItem, upgradeGroup, ui));
                    cnx.CreateCommand().ExecuteNonQuery(ui.UpgradeCmd);
                    this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.EndUpgradeItem, upgradeGroup, ui));
                }

                cnx.CreateCommand()
                    .AddParameter("@SchemaName", upgradeGroup.SchemaName)
                    .AddParameter("@Version", upgradeGroup.Version.ToString())
                    .AddParameter("@Date", upgradeGroup.Date)
                    .AddParameter("@Description", upgradeGroup.Description)
                    .AddParameter("@Type", upgradeGroup.Type.ToString())
                    .AddParameter("@ClientUpgradeDate", DateTime.Now)
                    .AddParameter("@ClientUpgradeInfo", GetClientUpgradeInfo(clientUpgradeInfo))
                    .ExecuteNonQuery("INSERT INTO `" + this.SchemaVersionsName + "`.`" + this.SchemaUpgradesTableName + "` (SchemaName, Version, Date, Description, Type, ClientUpgradeDate, ClientUpgradeInfo) VALUES (@SchemaName, @Version, @Date, @Description, @Type, @ClientUpgradeDate, @ClientUpgradeInfo)");

                trx.Commit();
                this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.EndUpgradeGroup, upgradeGroup));
            }
            catch (Exception ex)
            {
                trx.Rollback();
                this.OnUpgradeEvent(new UpgradeEventArgs(UpgradeEventType.UpgradeError, upgradeGroup, ui));
                throw new UpgradeException("Error upgrading database " + _SchemaVersionsName + " to " + upgradeGroup.Version.ToString() + " (" + upgradeGroup.Date.ToString() + ") " + upgradeGroup.Description + (ui == null ? "" : ", upgradeItem index " + i + ":\r\n" + ui.UpgradeCmd), ex);
            }
        }

        public void Dispose()
        {
            this.Upgrades.Clear();
        }

        public void SaveUpgradesToXml(Stream stream)
        {
            SaveToXml(_Upgrades, stream);
        }

        public void SaveUpgradesToXml(FileInfo xmlOutputFile)
        {
            SaveToXml(_Upgrades, xmlOutputFile);
        }

        private static void SaveToXml(UpgradeGroups upgradeGroups, FileInfo xmlOutputFile)
        {
            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(UpgradeGroups));
                Stream s = xmlOutputFile.Create();
                dcs.WriteObject(s, upgradeGroups);
                s.Close();
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error writing upgrade groups to xml file '" + xmlOutputFile.FullName + "'.", ex);
            }
        }

        public static void WriteXmlSchema(FileInfo xmlSchemaOutputFile)
        {
            try
            {
                List<Type> xsdTypes = new List<Type>() { typeof(UpgradeGroups), typeof(UpgradeGroup), typeof(UpgradeItem) };
                XsdDataContractExporter xsde = new XsdDataContractExporter();
                if (xsde.CanExport(xsdTypes))
                {
                    xsde.Export(xsdTypes);
                    xsde.Schemas.Compile();

                    StreamWriter ssc = xmlSchemaOutputFile.CreateText();
                    foreach (XmlSchema sc in xsde.Schemas.Schemas())
                    {
                        sc.Write(ssc);
                    }
                    ssc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error writing xml schema to xml file '" + xmlSchemaOutputFile.FullName + "'.", ex);
            }
        }

        private static void SaveToXml(UpgradeGroups upgradeGroups, Stream stream)
        {
            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(UpgradeGroups));
                dcs.WriteObject(stream, upgradeGroups);
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error writing upgrade groups to stream.", ex);
            }
        }

        private static UpgradeGroups LoadFromXml(Stream xmlInputStream)
        {
            try
            {
                XmlReaderSettings rs = new XmlReaderSettings();
                rs.IgnoreWhitespace = true;

                XmlReader xr = XmlReader.Create(xmlInputStream, rs);

                return LoadFromXml(xr);
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error reading upgrade groups from xml stream.", ex);
            }
        }

        private static UpgradeGroups LoadFromXml(XmlReader xmlReader)
        {
            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(UpgradeGroups));
                return (UpgradeGroups)dcs.ReadObject(xmlReader);
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error reading upgrade groups from xml reader.", ex);
            }
        }

        private static UpgradeGroups LoadFromXml(FileInfo xmlInputFile)
        {
            UpgradeGroups ret = null;
            try
            {
                Stream s = xmlInputFile.Open(FileMode.Open);
                ret = (UpgradeGroups)LoadFromXml(s);
                s.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error reading upgrade groups from xml file '" + xmlInputFile.FullName + "'.", ex);
            }
        }

        private static UpgradeGroups LoadFromXml(DirectoryInfo baseFolder, String fileNameFilter)
        {
            UpgradeGroups ret = new UpgradeGroups();
            foreach (FileInfo fi in baseFolder.GetFiles(fileNameFilter).OrderBy(v => v.Name))
            {
                ret.AddRange(LoadFromXml(fi));
            }

            return ret;
        }

        private static UpgradeGroups LoadFromXml(Assembly assembly, String resourcePath)
        {
            try
            {
                UpgradeGroups ret = null;
                Stream fs = assembly.GetManifestResourceStream(resourcePath);
                ret = (UpgradeGroups)LoadFromXml(fs);
                fs.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new UpgradeException("Error reading upgrade groups from xml resource file '" + resourcePath + "' from assembly '" + assembly.FullName + "'.", ex);
            }
        }

        private static UpgradeGroups LoadFromXml(Assembly assembly, String resourceFolder, String fileNameFilter)
        {
            if (!resourceFolder.EndsWith(".", StringComparison.InvariantCulture))
                resourceFolder = resourceFolder + ".";

            FileFilter fnf = new FileFilter(resourceFolder + (resourceFolder.EndsWith(".", StringComparison.InvariantCulture) ? "" : ".") + fileNameFilter);

            UpgradeGroups ret = new UpgradeGroups();
            foreach (String resourcePath in assembly.GetManifestResourceNames())
            {//.Where(v=>fileNameFilter.IsMatch(v)).OrderBy(v=>v)) {
                if (fnf.IsMatch(resourcePath))
                    ret.AddRange(LoadFromXml(assembly, resourcePath));
            }

            return ret;
        }


    }
}

