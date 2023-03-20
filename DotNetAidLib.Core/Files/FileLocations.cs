using System;
using System.IO;
using System.Reflection;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Introspection;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Files
{
    /// <summary>
    /// User and system File locations
    /// </summary>
    public enum FileLocation
    {
        /// <summary>
        /// Entry assemply folder
        /// </summary>
        ExecutableFolder,

        /// <summary>
        /// User base folder<br/>
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\$USER$ ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\Users\$USER$ ]]><br/>
        /// <![CDATA[Posix: ~ ]]>
        /// </summary>
        UserBaseFolder,

        /// <summary>
        /// User application data folder<br/>
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\$USER$\Local\<AppFolder> ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\Users\$USER$\AppData\Local\<AppFolder> ]]><br/>
        /// <![CDATA[Posix: ~/.local/<AppFolder> ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        UserApplicationDataFolder,

        /// <summary>
        /// Common users application data folder<br/>
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\All Users\<AppFolder> ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\ProgramData\<AppFolder> ]]><br/>
        /// <![CDATA[Posix: /usr/share/<AppFolder> ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        CommonApplicationDataFolder,

        /// <summary>
        /// User application data folder<br/>
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\$USER$\Local\<AppFolder>\log ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\Users\$USER$\AppData\Local\<AppFolder>\log ]]><br/>
        /// <![CDATA[Posix: ~/.local/<AppFolder>/log ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        UserLogDataFolder,

        /// <summary>
        /// User application data folder
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\$USER$\Local\<AppFolder>\conf ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\Users\$USER$\AppData\Local\<AppFolder>\conf ]]><br/>
        /// <![CDATA[Posix: ~/.config/<AppFolder> ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        UserConfigurationDataFolder,

        /// <summary>
        /// Common users application data folder<br/>
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\All Users\<AppFolder>\log ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\ProgramData\<AppFolder>\log ]]><br/>
        /// <![CDATA[Posix: /var/log/<AppFolder> ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        CommonLogDataFolder,

        /// <summary>
        /// Common users application data folder<br/>
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\All Users\<AppFolder>\config ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\ProgramData\<AppFolder>\config ]]><br/>
        /// <![CDATA[Posix: /etc/<AppFolder> ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        CommonConfigurationDataFolder,

        /// <summary>
        /// Base store data folder
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\All Users ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\ProgramData ]]><br/>
        /// <![CDATA[Posix: /var/lib ]]><br/>
        /// </summary>
        BaseStoreDataFolder,

        /// <summary>
        /// Common store data folder
        /// <![CDATA[Windows <= XP: %SystemDrive%\Documents and Settings\All Users\<AppFolder> ]]><br/>
        /// <![CDATA[Windows >= Vista: %SystemDrive%\ProgramData\<AppFolder> ]]><br/>
        /// <![CDATA[Posix: /var/lib/<AppFolder> ]]><br/>
        /// <![CDATA[ <AppFolder>: '(<AssemblyCompanyName>|DotNetApplication_<AssemblyGUID>)/<AssemblyName>' ]]>
        /// </summary>
        CommonStoreDataFolder,
    }

    public class FileLocations
    {
        public static DirectoryInfo GetLocation(FileLocation location, String user = null)
        {
            if (Helper.IsWindowsSO())
                return GetLocationWin(location, user);
            else
                return GetLocationPosix(location, user);
        }
        
        private static DirectoryInfo GetLocationPosix(FileLocation location, String user=null)
        {
            Assembly entryAssembly = Helper.GetEntryAssembly();

            String aPath = null;
            String bPath = null;
            String cPath = null;

            // Ruta final resultante: <aPath> + <bPath> + <cPath>
            if ((location.Equals(FileLocation.ExecutableFolder)))
            {
                aPath = new FileInfo(entryAssembly.Location).Directory.FullName;
                bPath = null;
                cPath = null;
            }
            else if (location.Equals(FileLocation.UserBaseFolder))
            {
                //(Posix): /home/<user>
                aPath = (user != null ? GetUserDirectoryPosix(user) : Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                bPath = null;
                cPath = null;
            }
            else if ((location.Equals(FileLocation.UserApplicationDataFolder)))
            {
                //(Posix): /home/<user>
                aPath = (user!=null?GetUserDirectoryPosix(user):Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                                   + Path.DirectorySeparatorChar + ".local"
                                   + Path.DirectorySeparatorChar + "share";
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.UserLogDataFolder)))
            {
                //(Posix): /home/<user>
                aPath = (user != null ? GetUserDirectoryPosix(user) : Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                                   + Path.DirectorySeparatorChar + ".log";
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.UserConfigurationDataFolder)))
            {
                //(Posix): /home/<user>
                aPath = (user != null ? GetUserDirectoryPosix(user) : Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                                   + Path.DirectorySeparatorChar + ".config";
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.CommonApplicationDataFolder)))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                // (Posix): /usr/share
                aPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.CommonLogDataFolder)))
            {
                aPath = "/var/log";
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.CommonConfigurationDataFolder)))
            {
                aPath = "/etc";
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.CommonStoreDataFolder)))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                // (Posix): /var/lib
                aPath = "/var/lib";
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if ((location.Equals(FileLocation.BaseStoreDataFolder)))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                // (Posix): /var/lib
                aPath = "/var/lib";
                bPath = null;
                cPath = null;
            }

            return new DirectoryInfo(
                aPath
                + (bPath == null ? "" : Path.DirectorySeparatorChar + bPath)
                + (cPath == null ? "" : Path.DirectorySeparatorChar + cPath));
        }
        
        private static String GetUserDirectoryPosix(String user) {
            FileInfo getentFI = new FileInfo(".").FromPathEnvironmentVariable("getent");
            String ret = null;
            try
            {
                if(getentFI==null)
                    throw new FileLoadException("getent program is missing.");

                ret = getentFI
                    .CmdExecuteSync("passwd " + user);
                String[] aret = ret.Split(':');
                if (ret.Length > 5)
                    ret = aret[5];
                else
                    ret = "/home/" + user;

                return ret;
            }
            catch (Exception ex) {
                throw new FileLoadException("Error retrieving user directory.", ex);
            }
        }

        private static String GetApplicationIDPath()
        {
            Assembly entryAssembly = Helper.GetEntryAssembly();

            // Nombre de la compañía (si no hubiese ninguna en el ensamblado, sería 'DotNetApplication_<GUID>')
            string companyName = entryAssembly.GetCompanyAttribute();
            if (companyName == null)
                companyName = "DotNetApplication_" + entryAssembly.GetType().GUID.ToString();

            companyName = companyName.NormalizeForFilename();

            // RelativePath
            return companyName.NormalizeForFilename() +
                   Path.DirectorySeparatorChar +
                   entryAssembly.GetName().Name;

        }
        
        private static  String GetUserDirectoryWin(String user)
        {
            String ret = null;
            try
            {
                ret = Environment.GetEnvironmentVariable("%HOMEDRIVE%")
                    + Environment.GetEnvironmentVariable("%HOMEPATH%");

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user directory.", ex);
            }
        }

        private static  DirectoryInfo GetLocationWin(FileLocation location, String user=null)
        {
            Assembly entryAssembly = Helper.GetEntryAssembly();

            String aPath = null;
            String bPath = null;
            String cPath = null;

            // Ruta final resultante: <aPath> + <bPath> + <cPath>
            if (location.Equals(FileLocation.ExecutableFolder))
            {
                aPath = new FileInfo(entryAssembly.Location).Directory.FullName;
                bPath = null;
                cPath = null;
            }
            else if (location.Equals(FileLocation.UserBaseFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\$USER$\Local
                // (Windows >= Vista): %SystemDrive%\Users\$USER$\AppData\Local
                aPath = (user != null ? GetUserDirectoryWin(user): Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace(@"\Local","").Replace(@"\AppData",""));
                bPath = null;
                cPath = null;
            }
            else if (location.Equals(FileLocation.UserApplicationDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\$USER$\Local
                // (Windows >= Vista): %SystemDrive%\Users\$USER$\AppData\Local
                aPath = (user != null ? GetUserDirectoryWin(user) + (Environment.OSVersion.Version.Minor < 6 ? @"\Local" : @"\AppData\Local") : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if (location.Equals(FileLocation.UserLogDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\$USER$\Local
                // (Windows >= Vista): %SystemDrive%\Users\$USER$\AppData\Local
                aPath = (user != null ? GetUserDirectoryWin(user) + (Environment.OSVersion.Version.Minor < 6 ? @"\Local" : @"\AppData\Local") : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                bPath = GetApplicationIDPath();
                cPath = "log";
            }
            else if (location.Equals(FileLocation.UserConfigurationDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\$USER$\Local
                // (Windows >= Vista): %SystemDrive%\Users\$USER$\AppData\Local
                aPath = (user != null ? GetUserDirectoryWin(user) + (Environment.OSVersion.Version.Minor < 6 ? @"\Local" : @"\AppData\Local") : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                bPath = GetApplicationIDPath();
                cPath = "conf";
            }
            else if (location.Equals(FileLocation.CommonApplicationDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                aPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if (location.Equals(FileLocation.CommonLogDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                aPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                bPath = GetApplicationIDPath();
                cPath = "log";
            }
            else if (location.Equals(FileLocation.CommonConfigurationDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                aPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                bPath = GetApplicationIDPath();
                cPath = "conf";
            }
            else if (location.Equals(FileLocation.CommonStoreDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                aPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                bPath = GetApplicationIDPath();
                cPath = null;
            }
            else if (location.Equals(FileLocation.BaseStoreDataFolder))
            {
                // (Windows <= XP): %SystemDrive%\Documents and Settings\All Users
                // (Windows >= Vista): %SystemDrive%\ProgramData
                aPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                bPath = null;
                cPath = null;
            }


            return new DirectoryInfo(
                aPath
                + (bPath == null ? "" : Path.DirectorySeparatorChar + bPath)
                + (cPath == null ? "" : Path.DirectorySeparatorChar + cPath));
        }
    }
}

