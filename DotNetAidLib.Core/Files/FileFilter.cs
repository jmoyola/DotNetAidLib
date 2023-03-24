using System;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Core.Files
{
	public class FileFilter
	{
		public FileFilter (String value)
		{
			this.Value = value;
		}

		public string Value{ get; set;}

		public Regex GetRegex(){
			return new Regex (this.Value.Replace(".","\\.").Replace("?",".").Replace("*",".*"));
		}

		public bool IsMatch(String fileName){
			return this.GetRegex ().IsMatch (fileName);
		}

		public static implicit operator FileFilter(String v)
		{
			return new FileFilter(v);
		}
	}
}

