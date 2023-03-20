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
	
	public class PluginFactory<T>
	{
		private static PluginFactory<T> _Instance = null;
		private List<PlugIn<T>> _PlugIns = null;

		private PluginFactory (FileInfo pluginFile, Type [] constructorParameters)
		: this (new Assembly [] { Assembly.LoadFrom (pluginFile.FullName) }, constructorParameters)
		{ }

		private PluginFactory (IEnumerable<Assembly> assemblies, Type [] constructorParameters)
		{
			_PlugIns = new List<PlugIn<T>> ();
			foreach (Assembly ass in assemblies)
				_PlugIns.AddRange (GetPluginsInAssembly (ass, constructorParameters));
		}

		private PluginFactory (DirectoryInfo baseFolder, bool recursive, string pluginsFilePattern, Type [] constructorParameters)
			:this(LoadAssemblies (baseFolder, recursive, pluginsFilePattern), constructorParameters)
		{}

		private PluginFactory (DirectoryInfo baseFolder, bool recursive, Type [] constructorParameters)
			:this(baseFolder, recursive, "*.dll", constructorParameters){}

		private PluginFactory (Type [] constructorParameters)
			: this (new FileInfo (Helpers.Helper.GetEntryAssembly ().Location).Directory, true, "*.dll", constructorParameters) { }

		public IList<PlugIn<T>> PlugIns {
			get { return _PlugIns; }
		}

		public static PluginFactory<T> Instance (DirectoryInfo baseFolder, bool recursive, string pluginsFilePattern, Type [] constructorParameters)
		{
			if (_Instance == null)
				_Instance = new PluginFactory<T> (baseFolder, recursive, pluginsFilePattern, constructorParameters);
			return _Instance;
		}

		public static PluginFactory<T> Instance (DirectoryInfo baseFolder, bool recursive, Type [] constructorParameters)
		{
			if (_Instance == null)
				_Instance = new PluginFactory<T> (baseFolder, recursive, constructorParameters);
			return _Instance;
		}

		public static PluginFactory<T> Instance (Type [] constructorParameters)
		{
			if (_Instance == null)
				_Instance = new PluginFactory<T> (constructorParameters);
			return _Instance;
		}

		public static PluginFactory<T> Instance (FileInfo pluginFile, Type [] constructorParameters)
		{
			if (_Instance == null)
				_Instance = new PluginFactory<T> (pluginFile, constructorParameters);
			return _Instance;
		}

		public static PluginFactory<T> Instance ()
		{
			if (_Instance == null)
				_Instance = new PluginFactory<T>(new Type[] { });
			return _Instance;
		}
			
		private static IList<Assembly> LoadAssemblies(DirectoryInfo baseFolder, bool recursive, string pluginsFilePattern)
		{
			List<Assembly> ret = new List<Assembly>();

			if (recursive)
				foreach (DirectoryInfo dir in baseFolder.GetDirectories ())
					ret.AddRange (LoadAssemblies (dir, recursive, pluginsFilePattern));
			
			foreach (FileInfo assFile in baseFolder.GetFiles(pluginsFilePattern)) {
				try
				{
					ret.Add(Assembly.LoadFile(assFile.FullName));
				}
				catch(Exception ex) {
					Debug.WriteLine("Error loading assembly from '" + assFile.FullName + "': " +  ex.Message);
				}
			}

			// Añadimos entryassembly si no existe ya en la colección (para los ejecutables)
			Assembly entryAssembly = Helpers.Helper.GetEntryAssembly();
			if (entryAssembly != null && !ret.Exists(v=>v.Equals(entryAssembly)))
				ret.Add(entryAssembly);

			return ret;
		}

		private static IList<PlugIn<T>> GetPluginsInAssembly(Assembly assembly, Type[] constructorParameters)
		{
			
			List<PlugIn<T>> ret = null;
			try{
				ret = new List<PlugIn<T>>();
				Assembly asm = assembly;
				Type tType = typeof(T);
				foreach (Type tt in asm.GetTypes())
				{
					// Si tt es herencia de T, no es una interface, no se llama igual a T, No es abstracta, es pública y tiene un constructor con los parámetros deseados...

					if (tType.IsAssignableFrom(tt) && !tt.Name.Equals(tType.Name)
						&& !tt.IsInterface && !tt.IsAbstract && tt.IsPublic
						&& (constructorParameters != null && tt.GetConstructor(constructorParameters) != null))
					{
						ret.Add(new PlugIn<T>(tt));
					}
				}            
			}
			catch(Exception ex){
				Debug.WriteLine("Error loading Types from assembly from '" + assembly.GetName().FullName + "': " + ex.Message);
			}
			return ret;
		}


	}
}