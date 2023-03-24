using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Helpers
{
    public class EnvironmentHelper
    {
        public static String PathExecute(String fileName,
            String defaultValueIfNotFound, String arguments = null)
        {
            FileInfo f = SearchInPath(fileName, false);
            if (f == null)
                return defaultValueIfNotFound;

            if (String.IsNullOrEmpty(arguments))
                return f.CmdExecuteSync();
            else
                return f.CmdExecuteSync(arguments);
        }

        public static String SearchInPathSt(String fileName,
            bool errorIfNotFound = false)
        {
            FileInfo ret = SearchInPath(fileName, errorIfNotFound);
            if (ret == null)
                return null;
            else
                return ret.FullName;
        }

        public static FileInfo GetFileFromPath(String fileName)
        {
            return SearchInPathGeneric(fileName, false);
        }
        
        public static FileInfo SearchInPath(String fileName,
            bool errorIfNotFound = false)
        {
            FileInfo ret = null;
            ret = SearchInPathGeneric(fileName, false);

            if (ret == null
                && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && !fileName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                ret = SearchInPathGeneric(fileName + ".exe", errorIfNotFound);


            return ret;
        }



        private static FileInfo SearchInPathGeneric(String fileName, bool errorIfNotFound)
        {
            FileInfo ret = null;
            String pathVariable = Environment.GetEnvironmentVariable("PATH");
            foreach (DirectoryInfo pathFolder in pathVariable.Split(Path.PathSeparator)
                         .Select(d => new DirectoryInfo(d)))
            {
                if (pathFolder.Exists)
                {
                    ret = pathFolder.GetFiles(fileName).FirstOrDefault();
                    if (ret != null)
                        break;
                }
            }

            if (errorIfNotFound && ret == null)
                throw new FileNotFoundException("Can't find '" + fileName + "' from Path environment.");

            return ret;
        }
        
        public enum RuntimeType{Mono, NetFramework}
        public static RuntimeType RuntimeInUse => Type.GetType("Mono.Runtime") != null ? RuntimeType.Mono : RuntimeType.NetFramework;

        public static bool IsUserInteractive(){
            bool ret;

            if (RuntimeInUse==RuntimeType.Mono) {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    ret = System.Environment.UserInteractive;
                else {
                    ret = Mono.Unix.Native.Syscall.isatty (0);
                }
            }else{
                ret = System.Environment.UserInteractive;
            }

            return ret;
        }

        public static bool IsCompiledInDebugMode(){
            bool ret=false;

            if (IsMonoRuntime())
            {

#if (DEBUG)
                ret = true;
#endif
            }
            else
                ret = IsCompiledInDebugMode(Assembly.GetEntryAssembly());
            return ret;

            //Assembly entryAssembly = Helpers.Helper.GetEntryAssembly();
            //return IsCompiledInDebugMode (entryAssembly);
        }

        public static bool IsCompiledInDebugMode(Assembly assembly){
            bool ret = false;

            DebuggableAttribute debugAtt=assembly.GetCustomAttribute<DebuggableAttribute>();
            if (debugAtt != null)
                ret = debugAtt.IsJITOptimizerDisabled;

            return ret;
        }

        public static bool IsWindowsSO(){
            return Environment.OSVersion.ToString ().IndexOf ("win", StringComparison.InvariantCultureIgnoreCase) > -1;
        }
        

        
        public static bool IsMonoRuntime(){
            return Type.GetType ("Mono.Runtime") != null;
        }

    }
}