using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Plugins
{
    public class Plugins
    {
        public static IList<Type> GetPluginsTypes<T>(Type[] constructorParameters)
        {
            return GetPluginsTypes<T>("*.dll", constructorParameters);
        }

        public static IList<Type> GetPluginsTypes<T>()
        {
            return GetPluginsTypes<T>("*.dll");
        }

        public static IList<Type> GetPluginsTypes<T>(string pluginsFilePattern, Type[] constructorParameters)
        {
            return GetPluginsTypesInAllAssemblies<T>(new FileInfo(Helper.GetEntryAssembly().Location).Directory,
                pluginsFilePattern, constructorParameters);
        }

        public static IList<Type> GetPluginsTypes<T>(string pluginsFilePattern)
        {
            return GetPluginsTypesInAllAssemblies<T>(new FileInfo(Helper.GetEntryAssembly().Location).Directory,
                pluginsFilePattern);
        }

        public static IList<T> GetPluginsInstances<T>(object[] constructorParameters)
        {
            return GetPluginsInstances<T>("*.dll", constructorParameters);
        }

        public static IList<T> GetPluginsInstances<T>(string pluginsFilePattern, object[] constructorParameters)
        {
            DirectoryInfo baseDirectory = null;
            //baseDirectory = new FileInfo(Helpers.Helper.GetEntryAssembly().Location).Directory;
            baseDirectory = Helper.BinBaseDirectory();
            return GetPluginsInstancesInAllAssemblies<T>(baseDirectory, pluginsFilePattern, constructorParameters);
        }

        public static IList<T> GetPluginsInstancesInAllAssemblies<T>(DirectoryInfo baseFolder,
            string pluginsFilePattern, object[] constructorParameters)
        {
            var ret = new List<T>();
            var assemblies = LoadAssemblies(baseFolder, pluginsFilePattern);

            foreach (var ass in assemblies) ret.AddRange(GetPluginsInstancesInAssembly<T>(ass, constructorParameters));

            return ret;
        }

        public static IList<Type> GetPluginsTypesInAllAssemblies<T>(DirectoryInfo baseFolder,
            string assemblyFilePattern, Type[] constructorParameters)
        {
            var ret = new List<Type>();

            var assemblies = LoadAssemblies(baseFolder, assemblyFilePattern);
            foreach (var ass in assemblies) ret.AddRange(GetPluginsTypesInAssembly<T>(ass, constructorParameters));

            return ret;
        }

        public static IList<Type> GetPluginsTypesInAllAssemblies<T>(DirectoryInfo baseFolder,
            string assemblyFilePattern)
        {
            var ret = new List<Type>();

            var assemblies = LoadAssemblies(baseFolder, assemblyFilePattern);
            foreach (var ass in assemblies) ret.AddRange(GetPluginsTypesInAssembly<T>(ass));

            return ret;
        }

        public static IList<Assembly> LoadAssemblies(DirectoryInfo baseFolder, string pluginsFilePattern)
        {
            var ret = new List<Assembly>();

            foreach (var assFile in baseFolder.GetFiles(pluginsFilePattern))
                try
                {
                    var assembly = Assembly.LoadFile(assFile.FullName);
                    ret.Add(assembly);
                }
                catch
                {
                }

            return ret;
        }

        public static IList<Type> GetPluginsTypesInFolder<T>(DirectoryInfo folder)
        {
            var ret = new List<Type>();

            foreach (var f in folder.GetFiles("*.dll"))
                try
                {
                    var assembly = Assembly.LoadFile(f.FullName);
                    ret.AddRange(GetPluginsTypesInAssembly<T>(assembly));
                }
                catch
                {
                }

            return ret;
        }

        public static IList<T> GetPluginsInstancesInFolder<T>(DirectoryInfo folder, object[] constructorParameters)
        {
            var ret = new List<T>();

            foreach (var f in folder.GetFiles("*.dll"))
                try
                {
                    var assembly = Assembly.LoadFile(f.FullName);
                    ret.AddRange(GetPluginsInstancesInAssembly<T>(assembly, constructorParameters));
                }
                catch
                {
                }

            return ret;
        }

        public static IList<Type> GetPluginsTypesInFile<T>(FileInfo file)
        {
            try
            {
                var assembly = Assembly.LoadFile(file.FullName);
                return GetPluginsTypesInAssembly<T>(assembly);
            }
            catch
            {
                return new List<Type>();
            }
        }

        public static IList<T> GetPluginsInstancesInFile<T>(FileInfo file, object[] constructorParameters)
        {
            try
            {
                var assembly = Assembly.LoadFile(file.FullName);
                return GetPluginsInstancesInAssembly<T>(assembly, constructorParameters);
            }
            catch
            {
                return new List<T>();
            }
        }

        public static IList<Type> GetPluginsTypesInAssembly<T>(Assembly assembly)
        {
            var ret = new List<Type>();
            var asm = assembly;
            var tType = typeof(T);
            foreach (var tt in asm.GetTypes())
                // Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...
                if (tType.IsAssignableFrom(tt) && !tt.Name.Equals(tType.Name)
                                               && !tt.IsInterface && !tt.IsAbstract && tt.IsPublic)
                    ret.Add(tt);

            return ret;
        }

        public static IList<Type> GetPluginsTypesInAssembly<T>(Assembly assembly, Type[] constructorParameters)
        {
            var ret = new List<Type>();
            var asm = assembly;
            var tType = typeof(T);
            foreach (var tt in asm.GetTypes())
                // Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...
                try
                {
                    if (tType.IsAssignableFrom(tt) && !tt.Name.Equals(tType.Name)
                                                   && !tt.IsInterface && !tt.IsAbstract && tt.IsPublic
                                                   && tt.GetConstructor(Type.GetTypeArray(constructorParameters)) !=
                                                   null)
                        ret.Add(tt);
                }
                catch
                {
                }

            return ret;
        }

        public static IList<T> GetPluginsInstancesInAssembly<T>(Assembly assembly, object[] constructorParameters)
        {
            var ret = new List<T>();
            var asm = assembly;
            var tType = typeof(T);

            foreach (var tt in asm.GetTypes())
                // Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...
                try
                {
                    if (tType.IsAssignableFrom(tt) && !tt.Name.Equals(tType.Name)
                                                   && !tt.IsInterface && !tt.IsAbstract && tt.IsPublic
                                                   && tt.GetConstructor(Type.GetTypeArray(constructorParameters)) !=
                                                   null)
                        ret.Add((T) Activator.CreateInstance(tt, constructorParameters));
                }
                catch
                {
                }

            return ret;
        }

        public static T GetInstance<T>(object[] constructorParameters)
        {
            return (T) Activator.CreateInstance(typeof(T), constructorParameters);
        }

        public static string Info(Type pluginType)
        {
            var ret = new StringBuilder();

            ret.AppendLine(pluginType.Name + " Plugin info: ");
            var inf = (PluginInfoAttribute) pluginType.GetCustomAttributes(typeof(PluginInfoAttribute), true)
                .FirstOrDefault();
            if (inf != null)
                ret.AppendLine(inf.Description);
            else
                ret.AppendLine(" No info.");

            ret.AppendLine(" Constructor Parameters: ");
            foreach (var ci in pluginType.GetConstructors())
            {
                var sp = "";
                foreach (var p in ci.GetParameters()) sp += p.ParameterType.Name + " " + p.Name + ", ";
                if (sp.Length > 0) sp = sp.Substring(0, sp.Length - 2);
                ret.AppendLine(pluginType.Name + "(" + sp + ")");
            }

            return ret.ToString();
        }
    }
}