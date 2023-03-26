using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Files;
using Mono.Unix.Native;

namespace DotNetAidLib.Core.Helpers
{
    public class EnvironmentHelper
    {
        public enum RuntimeType
        {
            Mono,
            NetFramework
        }

        public static RuntimeType RuntimeInUse =>
            Type.GetType("Mono.Runtime") != null ? RuntimeType.Mono : RuntimeType.NetFramework;

        public static string PathExecute(string fileName,
            string defaultValueIfNotFound, string arguments = null)
        {
            var f = SearchInPath(fileName);
            if (f == null)
                return defaultValueIfNotFound;

            if (string.IsNullOrEmpty(arguments))
                return f.CmdExecuteSync();
            return f.CmdExecuteSync(arguments);
        }

        public static string SearchInPathSt(string fileName,
            bool errorIfNotFound = false)
        {
            var ret = SearchInPath(fileName, errorIfNotFound);
            if (ret == null)
                return null;
            return ret.FullName;
        }

        public static FileInfo GetFileFromPath(string fileName)
        {
            return SearchInPathGeneric(fileName, false);
        }

        public static FileInfo SearchInPath(string fileName,
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


        private static FileInfo SearchInPathGeneric(string fileName, bool errorIfNotFound)
        {
            FileInfo ret = null;
            var pathVariable = Environment.GetEnvironmentVariable("PATH");
            foreach (var pathFolder in pathVariable.Split(Path.PathSeparator)
                         .Select(d => new DirectoryInfo(d)))
                if (pathFolder.Exists)
                {
                    ret = pathFolder.GetFiles(fileName).FirstOrDefault();
                    if (ret != null)
                        break;
                }

            if (errorIfNotFound && ret == null)
                throw new FileNotFoundException("Can't find '" + fileName + "' from Path environment.");

            return ret;
        }

        public static bool IsUserInteractive()
        {
            bool ret;

            if (RuntimeInUse == RuntimeType.Mono)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    ret = Environment.UserInteractive;
                else
                    ret = Syscall.isatty(0);
            }
            else
            {
                ret = Environment.UserInteractive;
            }

            return ret;
        }

        public static bool IsCompiledInDebugMode()
        {
            var ret = false;

            if (IsMonoRuntime())
            {
#if (DEBUG)
                ret = true;
#endif
            }
            else
            {
                ret = IsCompiledInDebugMode(Assembly.GetEntryAssembly());
            }

            return ret;

            //Assembly entryAssembly = Helpers.Helper.GetEntryAssembly();
            //return IsCompiledInDebugMode (entryAssembly);
        }

        public static bool IsCompiledInDebugMode(Assembly assembly)
        {
            var ret = false;

            var debugAtt = assembly.GetCustomAttribute<DebuggableAttribute>();
            if (debugAtt != null)
                ret = debugAtt.IsJITOptimizerDisabled;

            return ret;
        }

        public static bool IsWindowsSO()
        {
            return Environment.OSVersion.ToString().IndexOf("win", StringComparison.InvariantCultureIgnoreCase) > -1;
        }


        public static bool IsMonoRuntime()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}