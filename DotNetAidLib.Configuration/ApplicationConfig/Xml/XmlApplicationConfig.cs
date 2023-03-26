using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Configuration.ApplicationConfig.Xml
{
    public class XmlApplicationConfig : XmlApplicationConfigGroup, IApplicationConfig
    {
        private static readonly string XML_DOCUMENT_ROOT_NODE_NAME = "configurationGroup";

        private static readonly string ROOT_CONFIG_GROUP_NAME = "rootConfigurationGroup";

        private readonly FileInfo
            m_ConfigBackupFile; // Definición de archivo backup (.backup) del archivo de config. en uso

        private readonly object oFileLock = new object();

        public XmlApplicationConfig(FileInfo configFile)
            : base(null, null)
        {
            Assert.NotNull(configFile, nameof(configFile));

            ConfigFile = configFile;
            m_ConfigBackupFile = new FileInfo(ConfigFile.FullName + ".backup");

            try
            {
                Load();
            }
            catch
            {
                InitSettings();
            }
        }

        public FileInfo ConfigFile { get; }

        protected XmlDocument XmlConfig { get; private set; }

        public List<Type> KnownTypes => XmlApplicationConfigFactory.KnownTypes;

        public DateTime? LastSavedTime
        {
            get
            {
                if (m_XmlBaseGroupNode.Attributes["lastSavedTime"] == null
                    || string.IsNullOrEmpty(m_XmlBaseGroupNode.Attributes["lastSavedTime"].Value))
                    return null;
                return DateTime.Parse(m_XmlBaseGroupNode.Attributes["lastSavedTime"].Value,
                    CultureInfo.InvariantCulture.DateTimeFormat);
            }
        }

        public void Load()
        {
            lock (oFileLock)
            {
                try
                {
                    ConfigFile.Refresh();

                    // Intentamos cargar el archivo de configuración principal
                    XmlLoadFrom(ConfigFile);
                }
                catch (Exception ex)
                {
                    // Si ha habido error cargando el principal...
                    if (ConfigFile.Exists)
                        File.Move(ConfigFile.FullName,
                            ConfigFile.FullName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".error");

                    m_ConfigBackupFile.Refresh();
                    if (m_ConfigBackupFile.Exists) // Si hay backup de archivo de configuración
                        try
                        {
                            XmlLoadFrom(m_ConfigBackupFile); // Intentamos cargar desde el archivo de backup
                            // Si todo ok....
                            File.Copy(m_ConfigBackupFile.FullName, ConfigFile.FullName,
                                true); // Reestablecemos el archivo de configuración principal desde el de backup
                        }
                        catch (Exception ex2)
                        {
                            File.Move(m_ConfigBackupFile.FullName,
                                m_ConfigBackupFile.FullName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                                ".error");
                            throw new ApplicationConfigException("Error loading config file from backup.", ex2);
                        }
                    else // Si no, error...
                        throw new ApplicationConfigException("Error loading config file.", ex);
                }

                m_XmlBaseGroupNode = XmlConfig.SelectSingleNode("./" + XML_DOCUMENT_ROOT_NODE_NAME);
            }
        }

        public void Save()
        {
            lock (oFileLock)
            {
                XmlAttribute lastSavedAttribute = null;
                DateTime? lastSavedAux = null;
                FileInfo configTempFile = null; // Definición de archivo temporal (.tmp) del archivo de config. en uso

                try
                {
                    ConfigFile.Refresh();

                    // Si no existe la carpeta base, se crea
                    if (!ConfigFile.Directory.Exists)
                        ConfigFile.Directory.Create();


                    // Si existe el archivo temporal (.tmp), se borra
                    configTempFile = new FileInfo(ConfigFile.FullName + ".tmp");
                    if (configTempFile.Exists)
                        configTempFile.Delete();

                    lastSavedAux = LastSavedTime; // Guardamos la anterior fecha por si la cosa va mal, reestablecerla

                    lastSavedAttribute = m_XmlBaseGroupNode.Attributes["lastSavedTime"];
                    if (lastSavedAttribute == null)
                    {
                        lastSavedAttribute = XmlConfig.CreateAttribute("lastSavedTime");
                        m_XmlBaseGroupNode.Attributes.Append(lastSavedAttribute);
                    }

                    lastSavedAttribute.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);

                    // Guardamos hacia el archivo temporal (.tmp)
                    XmlConfig.Save(configTempFile.FullName);

                    // Si el archivo actual existe
                    if (ConfigFile
                        .Exists) // Copiamos (sobreescribiendo) el archivo actual hacia el archivo de backup (.backup)
                        File.Copy(ConfigFile.FullName, m_ConfigBackupFile.FullName, true);

                    // Copiamos (sobreescribiendo) el archivo temporal (.tmp) hacia el archivo actual
                    File.Copy(configTempFile.FullName, ConfigFile.FullName, true);

                    // Borramos el archivo temporal (.tmp)
                    configTempFile.Delete();

                    // Refrescamos el archivo actual
                    ConfigFile.Refresh();
                }
                catch (Exception ex)
                {
                    // Si algo fué mal, reestablecemos la última fecha de guardado
                    if (lastSavedAux.HasValue && lastSavedAttribute != null)
                        lastSavedAttribute.Value =
                            lastSavedAux.Value.ToString(CultureInfo.InvariantCulture.DateTimeFormat);

                    throw new ApplicationConfigException("Error saving config.", ex);
                }
            }
        }

        private void XmlLoadFrom(FileInfo xmlFile)
        {
            try
            {
                XmlConfig = new XmlDocument(); // Inicializamos el documento xml
                XmlConfig.Load(xmlFile.FullName);
            }
            catch (Exception ex)
            {
                throw new ApplicationConfigException("Error loading config from file '" + xmlFile.FullName + "'.",
                    ex); // Si no, error...
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
            XmlAttribute att = null;

            XmlConfig = new XmlDocument(); // Inicializamos el documento xml

            // Inicializamos el nodo base del documento xml
            XmlNode node = XmlConfig.CreateElement(XML_DOCUMENT_ROOT_NODE_NAME);


            att = XmlConfig.CreateAttribute("key");
            att.Value = ROOT_CONFIG_GROUP_NAME;
            node.Attributes.Append(att);


            // Se especifica la fecha de creación del grupo de configuración
            att = XmlConfig.CreateAttribute("dateOfCreation");
            att.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
            node.Attributes.Append(att);

            // Se especifica la fecha de modificación del grupo de configuración
            att = XmlConfig.CreateAttribute("dateOfModification");
            att.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
            node.Attributes.Append(att);

            XmlConfig.AppendChild(node);
            m_XmlBaseGroupNode = XmlConfig.SelectSingleNode("./" + XML_DOCUMENT_ROOT_NODE_NAME);
        }
    }
}