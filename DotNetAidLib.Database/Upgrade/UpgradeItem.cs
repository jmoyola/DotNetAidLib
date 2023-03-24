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
        private CDataWrapper _UpgradeCmd = "";
        private CDataWrapper _DowngradeCmd = "";

		public UpgradeItem (String upgradeCmd, String downgradeCmd)
		{
			this.UpgradeCmd = upgradeCmd;
			this.DowngradeCmd = downgradeCmd;
		}

		public UpgradeItem (String upgradeCmd):this(upgradeCmd,null){
		}

		public UpgradeItem (Assembly assembly, String upgradeCmdTextFilename){
			this.UpgradeCmd = ReadAssemblyTextFile (assembly, upgradeCmdTextFilename);
			this.DowngradeCmd = null;
		}

		[DataMember]
		public CDataWrapper UpgradeCmd{
            get
            {
                return _UpgradeCmd;
            }
            set
            {
                _UpgradeCmd = value;
            }
        }

		[DataMember]
		public CDataWrapper DowngradeCmd{
            get
            {
                return _DowngradeCmd;
            }
            set
            {
                _DowngradeCmd = value;
            }
        }

		private String ReadAssemblyTextFile(Assembly assembly, String textFilename){
			try{
				String ret = null;
				FileStream fs=assembly.GetFile (textFilename);
				StreamReader sr = new StreamReader (fs);
				ret=sr.ReadToEnd ();
				fs.Close ();
				return ret;
			}
			catch(Exception ex){
				throw new UpgradeException ("Error reading from text file '" + textFilename +"' in assembly '"+assembly.FullName +"': " + ex.Message, ex);
			}
		}
	}
}

