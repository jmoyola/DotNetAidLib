using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Globalization;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Configuration.ApplicationConfig.Xml{
    public class XmlApplicationConfig : XmlApplicationConfigGroup, IApplicationConfig
    {
        private static string XML_DOCUMENT_ROOT_NODE_NAME = "configurationGroup";

        private static string ROOT_CONFIG_GROUP_NAME = "rootConfigurationGroup";

        private FileInfo m_ConfigFile = null;       // Definición de archivo de config. en uso
        private FileInfo m_ConfigBackupFile = null; // Definición de archivo backup (.backup) del archivo de config. en uso

        private Object oFileLock = new object();

        private XmlDocument m_XmlDoc = null;

        public XmlApplicationConfig(FileInfo configFile)
            : base(null, null)
        {
            Assert.NotNull( configFile, nameof(configFile));

            this.m_ConfigFile = configFile;
            this.m_ConfigBackupFile = new FileInfo(m_ConfigFile.FullName + ".backup");

            try
            {
                this.Load();
            }
            catch {
                this.InitSettings();
            }
        }

        public List<Type> KnownTypes{
            get{
                return XmlApplicationConfigFactory.KnownTypes;
            }
        }

        public FileInfo ConfigFile {
            get { return m_ConfigFile; }
        }

        public DateTime? LastSavedTime
        {
            get {
                if (this.m_XmlBaseGroupNode.Attributes["lastSavedTime"]==null
                    || String.IsNullOrEmpty(this.m_XmlBaseGroupNode.Attributes["lastSavedTime"].Value))
                    return null;
                else
                    return DateTime.Parse(this.m_XmlBaseGroupNode.Attributes["lastSavedTime"].Value, CultureInfo.InvariantCulture.DateTimeFormat);
            }
        }

        public void Load()
        {
            lock (oFileLock)
            {
                try
                {
                    this.m_ConfigFile.Refresh();

                    // Intentamos cargar el archivo de configuración principal
                    this.XmlLoadFrom(this.m_ConfigFile);
                }
                catch(Exception ex){// Si ha habido error cargando el principal...
                    if (this.m_ConfigFile.Exists)
                        File.Move(this.m_ConfigFile.FullName, this.m_ConfigFile.FullName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".error");                    

                    this.m_ConfigBackupFile.Refresh();
                    if (m_ConfigBackupFile.Exists){ // Si hay backup de archivo de configuración
                        try
                        {
                            this.XmlLoadFrom(m_ConfigBackupFile); // Intentamos cargar desde el archivo de backup
                            // Si todo ok....
                            File.Copy(m_ConfigBackupFile.FullName, this.m_ConfigFile.FullName, true); // Reestablecemos el archivo de configuración principal desde el de backup
                        }
                        catch (Exception ex2)
                        {
                            File.Move(this.m_ConfigBackupFile.FullName, this.m_ConfigBackupFile.FullName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".error");
                            throw new ApplicationConfigException("Error loading config file from backup.", ex2);
                        }
                    }
                    else // Si no, error...
                    {
                        throw new ApplicationConfigException("Error loading config file.", ex);
                    }
                }
                this.m_XmlBaseGroupNode = this.m_XmlDoc.SelectSingleNode("./" + XML_DOCUMENT_ROOT_NODE_NAME);
            }
        }

        private void XmlLoadFrom(FileInfo xmlFile)
        {
            try
            {
                this.m_XmlDoc = new XmlDocument(); // Inicializamos el documento xml
                this.m_XmlDoc.Load(xmlFile.FullName);
            }
            catch(Exception ex){
                throw new ApplicationConfigException("Error loading config from file '" + xmlFile.FullName + "'.", ex); // Si no, error...
            }
        }

        public void Save()
        {
            lock (oFileLock)
            {
                XmlAttribute lastSavedAttribute = null;
                DateTime? lastSavedAux = null;
                FileInfo configTempFile = null;   // Definición de archivo temporal (.tmp) del archivo de config. en uso

                try
                {

                    m_ConfigFile.Refresh();

                    // Si no existe la carpeta base, se crea
                    if (!this.m_ConfigFile.Directory.Exists)
                        this.m_ConfigFile.Directory.Create();


                    // Si existe el archivo temporal (.tmp), se borra
                    configTempFile = new FileInfo(this.m_ConfigFile.FullName + ".tmp");
                    if (configTempFile.Exists)
                        configTempFile.Delete();

                    lastSavedAux = this.LastSavedTime; // Guardamos la anterior fecha por si la cosa va mal, reestablecerla

                    lastSavedAttribute = this.m_XmlBaseGroupNode.Attributes["lastSavedTime"];
                    if (lastSavedAttribute == null){
                        lastSavedAttribute = this.m_XmlDoc.CreateAttribute("lastSavedTime");
                        this.m_XmlBaseGroupNode.Attributes.Append(lastSavedAttribute);
                    }
                    lastSavedAttribute.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);

                    // Guardamos hacia el archivo temporal (.tmp)
                    m_XmlDoc.Save(configTempFile.FullName);

                    // Si el archivo actual existe
                    if (this.m_ConfigFile.Exists)// Copiamos (sobreescribiendo) el archivo actual hacia el archivo de backup (.backup)
                        File.Copy(this.m_ConfigFile.FullName, this.m_ConfigBackupFile.FullName, true);

                    // Copiamos (sobreescribiendo) el archivo temporal (.tmp) hacia el archivo actual
                    File.Copy(configTempFile.FullName, this.m_ConfigFile.FullName, true);

                    // Borramos el archivo temporal (.tmp)
                    configTempFile.Delete();

                    // Refrescamos el archivo actual
                    this.m_ConfigFile.Refresh();
                }
                catch(Exception ex){
                    // Si algo fué mal, reestablecemos la última fecha de guardado
                    if(lastSavedAux.HasValue && lastSavedAttribute!=null)
                        lastSavedAttribute.Value = lastSavedAux.Value.ToString(CultureInfo.InvariantCulture.DateTimeFormat);

                    throw new ApplicationConfigException("Error saving config.", ex);
                }
            }
        }

        private void XmlSaveTo(XmlDocument xmlDoc, FileInfo configFile)
        {
            try
            {
                // Creamos la carpeta si no existe
                if (!configFile.Directory.Exists)
                    configFile.Directory.Create();

                xmlDoc.Save(configFile.FullName);

                configFile.Refresh();
            }
            catch (Exception ex)
            {
                throw new ApplicationConfigException("Error saving config to file '" + configFile.FullName + "'.", ex);
            }
        }

        private void InitSettings()
        {
            XmlAttribute att=null;

            this.m_XmlDoc = new XmlDocument(); // Inicializamos el documento xml

            // Inicializamos el nodo base del documento xml
            XmlNode node = this.m_XmlDoc.CreateElement(XML_DOCUMENT_ROOT_NODE_NAME);


            att = this.m_XmlDoc.CreateAttribute("key");
            att.Value = ROOT_CONFIG_GROUP_NAME;
            node.Attributes.Append(att);


            // Se especifica la fecha de creación del grupo de configuración
            att = this.m_XmlDoc.CreateAttribute("dateOfCreation");
            att.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
            node.Attributes.Append(att);

            // Se especifica la fecha de modificación del grupo de configuración
            att = this.m_XmlDoc.CreateAttribute("dateOfModification");
            att.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
            node.Attributes.Append(att);

            this.m_XmlDoc.AppendChild(node);
            this.m_XmlBaseGroupNode = this.m_XmlDoc.SelectSingleNode("./" + XML_DOCUMENT_ROOT_NODE_NAME);
        }

        protected XmlDocument XmlConfig {
            get { return m_XmlDoc; }
        }
    }
}