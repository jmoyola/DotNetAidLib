using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
	public class IniExConfigurationFile : IniExConfiguration
	{
        private static Dictionary<String, IniExConfigurationFile> _Instances = new Dictionary<String, IniExConfigurationFile>();

        private FileInfo file;

		private IniExConfigurationFile(FileInfo file)
		{
            Assert.NotNull( file, nameof(file));
            this.file = file;
        }

		public FileInfo File {
			get { return this.file; }
		}

        public static IniExConfigurationFile Instance(FileInfo file)
        {
            if (!_Instances.ContainsKey(file.FullName))
                _Instances.Add(file.FullName, new IniExConfigurationFile(file));

            return _Instances[file.FullName];
        }

        public void Load()
		{
            StreamReader sr = null;

			try {
				if (file==null)
					throw new Exception ("File is not set.");
				
				if (!file.Exists)
					throw new Exception ("File not exists.");

				this.Clear();

				sr = file.OpenText();
                this.Load(sr);
			
			} catch (Exception ex) {
				throw new Exception("Error loading configuration from '" + file.FullName + "' " + ex.Message, ex);
			} finally {
				try {
                    if(sr!=null)
					    sr.Close();
				} catch {}
			}
		}


		public void Save()
		{
			StreamWriter sw = null;
			try {
				sw = file.CreateText();

                this.Save(sw);
				sw.Flush();
			} catch (Exception ex) {
				throw new Exception("Error saving configuration to '" + file.FullName + "': " + ex.Message, ex);
			} finally {
				try {
                    if(sw!=null)
					    sw.Close();
				} catch {}
			}
		}
    }
}