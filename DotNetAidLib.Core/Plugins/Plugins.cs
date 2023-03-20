using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Text;

namespace DotNetAidLib.Core.Plugins{

	public class Plugins
	{
		public static IList<Type> GetPluginsTypes<T>(Type[] constructorParameters){
			return GetPluginsTypes<T> ("*.dll", constructorParameters);
		}

		public static IList<Type> GetPluginsTypes<T>(){
			return GetPluginsTypes<T> ("*.dll");
		}

		public static IList<Type> GetPluginsTypes<T>(string pluginsFilePattern, Type[] constructorParameters)
		{
			return GetPluginsTypesInAllAssemblies<T>(new FileInfo(Helpers.Helper.GetEntryAssembly().Location).Directory, pluginsFilePattern,constructorParameters);
		}

		public static IList<Type> GetPluginsTypes<T>(string pluginsFilePattern)
		{
			return GetPluginsTypesInAllAssemblies<T>(new FileInfo(Helpers.Helper.GetEntryAssembly().Location).Directory, pluginsFilePattern);
		}

		public static IList<T> GetPluginsInstances<T>(object[] constructorParameters){
			return GetPluginsInstances<T> ("*.dll", constructorParameters); 
		}

		public static IList<T> GetPluginsInstances<T>(string pluginsFilePattern, object[] constructorParameters)
		{
			DirectoryInfo baseDirectory = null;
			//baseDirectory = new FileInfo(Helpers.Helper.GetEntryAssembly().Location).Directory;
			baseDirectory=Helpers.Helper.BinBaseDirectory();
			return GetPluginsInstancesInAllAssemblies<T>(baseDirectory, pluginsFilePattern, constructorParameters);
		}

		public static IList<T> GetPluginsInstancesInAllAssemblies<T>(DirectoryInfo baseFolder, string pluginsFilePattern, object[] constructorParameters)
		{
			List<T> ret = new List<T>();
			IList<Assembly> assemblies = LoadAssemblies(baseFolder, pluginsFilePattern);

			foreach (Assembly ass in assemblies) {
				ret.AddRange(GetPluginsInstancesInAssembly<T>(ass, constructorParameters));
			}

			return ret;
		}

		public static IList<Type> GetPluginsTypesInAllAssemblies<T>(DirectoryInfo baseFolder, string assemblyFilePattern, Type[] constructorParameters)
		{
			List<Type> ret = new List<Type>();

			IList<Assembly> assemblies = LoadAssemblies(baseFolder, assemblyFilePattern);
			foreach (Assembly ass in assemblies) {
				ret.AddRange(GetPluginsTypesInAssembly<T>(ass, constructorParameters));
			}

			return ret;
		}

		public static IList<Type> GetPluginsTypesInAllAssemblies<T>(DirectoryInfo baseFolder, string assemblyFilePattern)
		{
			List<Type> ret = new List<Type>();

			IList<Assembly> assemblies = LoadAssemblies(baseFolder, assemblyFilePattern);
			foreach (Assembly ass in assemblies) {
				ret.AddRange(GetPluginsTypesInAssembly<T>(ass));
			}

			return ret;
		}
			
		public static IList<Assembly> LoadAssemblies(DirectoryInfo baseFolder, string pluginsFilePattern)
		{
			List<Assembly> ret = new List<Assembly>();

			foreach (FileInfo assFile in baseFolder.GetFiles(pluginsFilePattern)) {
				try
				{
					Assembly assembly = Assembly.LoadFile(assFile.FullName);
					ret.Add(assembly);
				}
				catch { }
			}

			return ret;
		}

		public static IList<Type> GetPluginsTypesInFolder<T>(DirectoryInfo folder)
		{
			List<Type> ret = new List<Type>();

			foreach (FileInfo f in folder.GetFiles("*.dll")) {
				try
				{
					Assembly assembly = Assembly.LoadFile(f.FullName);
					ret.AddRange(GetPluginsTypesInAssembly<T>(assembly));
				}
				catch { }
			}

			return ret;
		}

		public static IList<T> GetPluginsInstancesInFolder<T>(DirectoryInfo folder, object[] constructorParameters)
		{
			List<T> ret = new List<T>();

			foreach (FileInfo f in folder.GetFiles("*.dll")) {
				try
				{
					Assembly assembly = Assembly.LoadFile(f.FullName);
					ret.AddRange(GetPluginsInstancesInAssembly<T>(assembly, constructorParameters));
				}
				catch { }
			}

			return ret;
		}

		public static IList<Type> GetPluginsTypesInFile<T>(FileInfo file)
		{
			try
			{
				Assembly assembly = Assembly.LoadFile(file.FullName);
				return GetPluginsTypesInAssembly<T>(assembly);
			} catch{
				return new List<Type>();
			}
		}

		public static IList<T> GetPluginsInstancesInFile<T>(FileInfo file, object[] constructorParameters)
		{
			try {
				Assembly assembly = Assembly.LoadFile(file.FullName);
				return GetPluginsInstancesInAssembly<T>(assembly, constructorParameters);
			} catch{
				return new List<T>();
			}
		}

		public static IList<Type> GetPluginsTypesInAssembly<T>(Assembly assembly)
		{
			List<Type> ret = new List<Type>();
			Assembly asm = assembly;
			Type tType = typeof(T);
			foreach (Type tt in asm.GetTypes()) {
				// Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...

				if (tType.IsAssignableFrom(tt) && !tt.Name.Equals(tType.Name)
					&& !tt.IsInterface && !tt.IsAbstract && tt.IsPublic)
				{
					ret.Add(tt);
				}
			}

			return ret;
		}

		public static IList<Type> GetPluginsTypesInAssembly<T>(Assembly assembly, Type[] constructorParameters)
		{
			List<Type> ret = new List<Type>();
			Assembly asm = assembly;
			Type tType = typeof(T);
			foreach (Type tt in asm.GetTypes()) {
                // Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...
                try {
                    if (tType.IsAssignableFrom (tt) && !tt.Name.Equals (tType.Name)
                        && !tt.IsInterface && !tt.IsAbstract && tt.IsPublic
                        && tt.GetConstructor (Type.GetTypeArray (constructorParameters)) != null) {
                        ret.Add (tt);
                    }
                } catch { }
			}

			return ret;
		}

		public static IList<T> GetPluginsInstancesInAssembly<T>(Assembly assembly, object[] constructorParameters)
		{
			List<T> ret = new List<T>();
			Assembly asm = assembly;
			Type tType = typeof(T);

			foreach (Type tt in asm.GetTypes()) {
                // Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...
                try
                {
	                if (tType.IsAssignableFrom(tt) && !tt.Name.Equals(tType.Name)
	                                               && !tt.IsInterface && !tt.IsAbstract && tt.IsPublic
	                                               && tt.GetConstructor(Type.GetTypeArray(constructorParameters)) !=
	                                               null)
	                {
		                ret.Add((T) Activator.CreateInstance(tt, constructorParameters));
	                }
                }
                catch { }
			}

			return ret;
		}

		public static T GetInstance<T>(object[] constructorParameters){
			return (T)Activator.CreateInstance (typeof (T), constructorParameters);
		}

		public static string Info(Type pluginType)
		{
			StringBuilder ret = new StringBuilder();

			ret.AppendLine(pluginType.Name + " Plugin info: ");
			PluginInfoAttribute inf = (PluginInfoAttribute)pluginType.GetCustomAttributes(typeof(PluginInfoAttribute), true).FirstOrDefault();
			if (((inf != null))) {
				ret.AppendLine(inf.Description);
			} else {
				ret.AppendLine(" No info.");
			}

			ret.AppendLine(" Constructor Parameters: ");
			foreach (ConstructorInfo ci in pluginType.GetConstructors()) {
				string sp = "";
				foreach (ParameterInfo p in ci.GetParameters()) {
					sp += p.ParameterType.Name + " " + p.Name + ", ";
				}
				if ((sp.Length > 0)) {
					sp = sp.Substring(0, sp.Length - 2);
				}
				ret.AppendLine(pluginType.Name + "(" + sp + ")");
			}

			return ret.ToString();
		}
	}
}