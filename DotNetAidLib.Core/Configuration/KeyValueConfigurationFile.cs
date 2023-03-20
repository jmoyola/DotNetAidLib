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
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
	public class KeyValueConfigurationFile : KeyValueConfiguration
	{

		private static Dictionary<String, KeyValueConfigurationFile> _Instances = new Dictionary<String, KeyValueConfigurationFile>();

		private FileInfo _File;

		private Object oBlock = new Object();

		private KeyValueConfigurationFile(FileInfo file, char[] commentChars,char[] assignationChars)
		{
            Assert.NotNull( file, nameof(file));

			this.CommentChars = commentChars;
            this.AssignationChar = assignationChars;

			_File = file;
		}

		public FileInfo File {
			get { return _File; }
			set { _File=value; }
		}

        public static KeyValueConfigurationFile Instance(FileInfo file) {
            return Instance(file, KeyValueConfiguration.DEFAULT_COMMENTCHARS, KeyValueConfiguration.DEFAULT_ASSIGNATIONCHARS);
        }

        public static KeyValueConfigurationFile Instance(FileInfo file, char[] commentChars, char[] assignationChars)
		{
			KeyValueConfigurationFile ret = null;

			if (!_Instances.ContainsKey(file.FullName))
				_Instances.Add(file.FullName, new KeyValueConfigurationFile(file, commentChars, assignationChars));
			
			return ret = _Instances[file.FullName];
        }

		public void Load()
		{
			lock (oBlock)
			{
            
				try
				{
					if (_File == null)
						throw new Exception("File is not set.");

                    _File.Refresh();

                    if (!_File.Exists)
						return;

                    using (StreamReader sr = _File.OpenText())
                        this.Load(sr);

				}
				catch (Exception ex)
				{
					throw new Exception("Error loading configuration from '" + _File.FullName + "'.", ex);
				}
			}
		}


        public void Save(bool includeHeader=true)
		{
			lock(oBlock)
			{
				try
				{
                    using (StreamWriter sw = _File.CreateText())
                        this.Save(sw, includeHeader);
				}
				catch (Exception ex)
				{
					throw new Exception("Error saving configuration to '" + _File.FullName + "'.", ex);
				}
			}
		}
    }
}