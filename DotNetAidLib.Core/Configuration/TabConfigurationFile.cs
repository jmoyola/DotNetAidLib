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
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
	public class TabConfigurationFile : List<CaptiontItem<List<String>>>
	{

		private static Dictionary<String, TabConfigurationFile> _Instances = new Dictionary<String, TabConfigurationFile>();

		private FileInfo _File;
		private String _CommentChars;
		private String _SeparatorChars;
		private int _SeparatorLenght;

		private TabConfigurationFile(FileInfo file)
		:this(file, @"#\;",  " \t", 4){
		}

		private TabConfigurationFile(FileInfo file, String commentChars, String separatorChars, int separatorLenght){
			this.CommentChars = commentChars;
			this.SeparatorChars = separatorChars;
			this.SeparatorLenght = separatorLenght;
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
				Assert.NotNullOrEmpty( value, nameof(value));
				_CommentChars=Regex.Escape(value);
			}
		}

		public String SeparatorChars
		{
			get
			{
				return Regex.Unescape(_SeparatorChars);
			}
			set
			{
				Assert.NotNullOrEmpty( value, nameof(value));
				_SeparatorChars = value;
			}
		}

		public int SeparatorLenght
		{
			get{
				return _SeparatorLenght;
			}

			set{
				Assert.GreaterThan(value, 0, nameof(value));
				_SeparatorLenght = value;
			}
		}

		public static TabConfigurationFile Instance(FileInfo file)
		{
			TabConfigurationFile ret = null;

			if (!_Instances.ContainsKey(file.FullName)) {
				ret = new TabConfigurationFile(file);
				_Instances.Add(file.FullName, ret);

			}
			ret = _Instances[file.FullName];

			return ret;
		}

		public void Load()
		{
			CaptiontItem<List<string>> ret = null;
			StreamReader sr = null;
			try {
				Regex regex = new Regex(@"([^" + Regex.Escape(_SeparatorChars) + "]+)([" + Regex.Escape(_SeparatorChars) + "]+)?");

				if (_File==null)
					throw new Exception ("File is not set.");
				
				if (!_File.Exists)
					throw new Exception ("File not exists.");

				this.Clear();

				sr = _File.OpenText();

				string li = sr.ReadLine();
				ret = new CaptiontItem<List<String>>();
				this.Add(ret);
				while (li != null && li.Trim().Length > 0) {
					if (li.TrimStart().StartsWith("#", StringComparison.InvariantCulture))
						ret.Caption +=(String.IsNullOrEmpty(ret.Caption)?"":Environment.NewLine) + li.Trim().Substring(1);
					else {
						MatchCollection mc = regex.Matches(li);
						if (mc.Count > 0){
							ret.Value = new List<String>();
							foreach (Match m in mc)
								ret.Value.Add(m.Groups[1].Value);

							ret = new CaptiontItem<List<String>>();
							this.Add(ret);
						}
					}

					li = sr.ReadLine();
				}
			} catch (Exception ex) {
				throw new Exception("Error loading configuration from '" + _File.FullName + "' " + ex.Message, ex);
			} finally {
				if(sr!=null)
					sr.Close();
			}
		}


		public void Save()
		{
			StreamWriter sw = null;
			try {
				sw = _File.CreateText();
				foreach (CaptiontItem<List<String>> line in this){
					if (line.Caption != null){
						foreach (String commentLine in line.Caption.Split('\n'))
							sw.WriteLine("# " + commentLine);
					}
					if(line.Value!=null)
						sw.WriteLine(line.Value.ToStringJoin(new String(_SeparatorChars[0], _SeparatorLenght)));
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
	}
}