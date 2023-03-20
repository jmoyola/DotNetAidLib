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
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Configuration
{
	public class IniXConfigurationFile : Dictionary<String, DictionaryList<string, string>>
	{

		private static Dictionary<String, IniXConfigurationFile> _Instances = new Dictionary<String, IniXConfigurationFile>();

		private FileInfo _File;
		private String _CommentChars;

		private IniXConfigurationFile(FileInfo file)
		:this(file, @"#\;")
		{
		}

		private IniXConfigurationFile(FileInfo file, String commentChars)
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

		public static IniXConfigurationFile Instance(FileInfo file)
		{
			IniXConfigurationFile ret = null;

			if ((_Instances.ContainsKey(file.FullName))) {
				ret = _Instances[file.FullName];
			} else {
				ret = new IniXConfigurationFile(file);
				_Instances.Add(file.FullName, ret);
			}

			return ret;
		}

		public void Load()
		{
			DictionaryList<string, string> ret = null;
			StreamReader sr = null;
			try {
				if (_File==null)
					throw new Exception ("File is not set.");
				
				if (!_File.Exists)
					throw new Exception ("File not exists.");

				this.Clear();
				ret = new DictionaryList<string, string>();
				this.Add("", ret);

				sr = _File.OpenText();

				string li = sr.ReadLine();

				while (li != null) {
					if (li.Trim().Length > 0 && !li.RegexIsMatch(@"^\s*[" + _CommentChars + "].*$")) {
						string[] aLi = li.Split('=');
						if (aLi.Length == 1){
							if (aLi[0].RegexIsMatch(@"^\s*\[(.+)\]$")) {
								ret = new DictionaryList<string, string>();
								this.Add(aLi[0].RegexGroupsMatches(@"^\s*\[(.+)\]$")[1], ret);
							}
							else
								ret.Add(aLi[0].Trim(), null);
						}
						else if (aLi.Length > 1)
							ret.Add(aLi[0].Trim(), aLi[1].Trim());
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
					foreach (KeyValue<string, string> kv in this[key])
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

		public DictionaryList<String, String> AddGroup(String groupName){
			DictionaryList<String, String> ret = new DictionaryList<String, String> ();
			this.Add (groupName, ret);
			return ret;
		}

		public DictionaryList<String, String> GetGroup(String groupName){
			return this[groupName];
		}

	}
}