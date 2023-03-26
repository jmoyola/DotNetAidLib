using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Xml;

namespace DotNetAidLib.Database.Upgrade
{
    [DataContract]
    public class UpgradeItem
    {
        public UpgradeItem(string upgradeCmd, string downgradeCmd)
        {
            UpgradeCmd = upgradeCmd;
            DowngradeCmd = downgradeCmd;
        }

        public UpgradeItem(string upgradeCmd) : this(upgradeCmd, null)
        {
        }

        public UpgradeItem(Assembly assembly, string upgradeCmdTextFilename)
        {
            UpgradeCmd = ReadAssemblyTextFile(assembly, upgradeCmdTextFilename);
            DowngradeCmd = null;
        }

        [DataMember] public CDataWrapper UpgradeCmd { get; set; } = "";

        [DataMember] public CDataWrapper DowngradeCmd { get; set; } = "";

        private string ReadAssemblyTextFile(Assembly assembly, string textFilename)
        {
            try
            {
                string ret = null;
                var fs = assembly.GetFile(textFilename);
                var sr = new StreamReader(fs);
                ret = sr.ReadToEnd();
                fs.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new UpgradeException(
                    "Error reading from text file '" + textFilename + "' in assembly '" + assembly.FullName + "': " +
                    ex.Message, ex);
            }
        }
    }
}