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
	public class IniConfigurationFile : Dictionary<String, Dictionary<string, string>>
	{

		private static Dictionary<String, IniConfigurationFile> _Instances = new Dictionary<String, IniConfigurationFile>();

		private FileInfo _File;
		private String _CommentChars;

		private IniConfigurationFile(FileInfo file)
		:this(file, @"#\;")
		{
		}

		private IniConfigurationFile(FileInfo file, String commentChars)
		{
			this.CommentChars = commentChars;
			_File = file;
		}

		public FileInfo File {
			get { return _File; }
			set { _File=value; }
		}

		public String CommentChars{
			get{
				return Regex.Unescape(_CommentChars);
			}
			set{
				if (String.IsNullOrEmpty (value))
					throw new Exception ("CommentChars can't be null or empty.");
				_CommentChars=Regex.Escape(value);
			}
		}

		public static IniConfigurationFile Instance(FileInfo file)
		{
            Assert.NotNull( file, nameof(file));

			IniConfigurationFile ret = null;

			if ((_Instances.ContainsKey(file.FullName))) {
				ret = _Instances[file.FullName];
			} else {
				ret = new IniConfigurationFile(file);
				_Instances.Add(file.FullName, ret);
			}

			return ret;
		}

        private static Regex keyValueRegex = new Regex(@"^\s*([^=\s]+)\s*(=\s*(.*))?$");
        private static Regex iniGroupRegex = new Regex(@"^\s*\[\s*([^\s]+)\s*\]\s*$");

        public void Load()
		{


            Dictionary<string, string> ret = null;
			StreamReader sr = null;
			try {
				if (_File==null)
					throw new Exception ("File is not set.");
				
				if (!_File.Exists)
					throw new Exception ("File not exists.");

				this.Clear();
				ret = new Dictionary<string, string>();
				this.Add("", ret);

				sr = _File.OpenText();

				string li = sr.ReadLine();

				while (li != null) {
					if (li.Trim().Length > 0 && !li.RegexIsMatch(@"^\s*[" + _CommentChars + "].*$")) {

                        if (iniGroupRegex.IsMatch(li)) // Si es un grupo
                        {
                            ret = new Dictionary<string, string>();
                            this.Add(iniGroupRegex.Match(li).Groups[1].Value, ret);
                        }
                        else if(keyValueRegex.IsMatch(li)) // Si es un key value
                        {
                            Match keyValueMatch = keyValueRegex.Match(li);
                            if (String.IsNullOrEmpty(keyValueMatch.Groups[2].Value)) // si no hay value
                                ret.Add(keyValueMatch.Groups[1].Value.Trim(), null);
                            else
                                ret.Add(keyValueMatch.Groups[1].Value.Trim(), keyValueMatch.Groups[3].Value.Trim());
						}
						
					}							
					li = sr.ReadLine();
				}

				if(this[""].Keys.Count==0) // Si el grupo sin nombre está vacío, se elimina
					this.Remove("");
			
			} catch (Exception ex) {
				throw new Exception("Error loading configuration from '" + _File.FullName + "' " + ex.Message, ex);
			} finally {
				try {
					sr.Close();
				} catch {
				}
			}
		}


		public void Save()
		{
			StreamWriter sw = null;
			try {
				sw = _File.CreateText();

				foreach(String key in this.Keys){
					if(!string.IsNullOrEmpty(key))
						sw.WriteLine("[" + key + "]");
					foreach (KeyValuePair<string, string> kv in this[key])
						if(kv.Value==null)
							sw.WriteLine(kv.Key);
						else
							sw.WriteLine(kv.Key + "=" + kv.Value);
				}
				sw.Flush();
			} catch (Exception ex) {
				throw new Exception("Error saving configuration to '" + _File.FullName + "': " + ex.Message, ex);
			} finally {
				try {
					sw.Close();
				} catch {
				}
			}
		}

		public Dictionary<String, String> AddGroup(String groupName){
			Dictionary<String, String> ret = new Dictionary<String, String> ();
			this.Add (groupName, ret);
			return ret;
		}

		public Dictionary<String, String> GetGroup(String groupName){
			return this[groupName];
		}

        public bool ContainsGroup(String groupName)
        {
            return this.ContainsKey(groupName);
        }

    }
}