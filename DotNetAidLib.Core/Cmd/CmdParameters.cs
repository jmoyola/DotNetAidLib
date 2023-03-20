using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace DotNetAidLib.Core.Cmd{
	public class CmdParameters : Dictionary<string, string>
	{
		private List<KeyValuePair<string, string>> _UnNamedParameters;
		public CmdParameters(FileInfo cfgFile) : this(readConfigFromFile(cfgFile))
		{
		}

		public CmdParameters(string args) : this(ArgumentsStringsSplit(args, ' '))
		{
		}

		private static Regex keyValueRegex = new Regex(@"^-?[-/]([^=:]+)([=:](.+))?$");
		public CmdParameters(string[] args)
		{
			for (int i = 0; i <= args.Length - 1; i++) {
				string arg = args[i];
				Match m = keyValueRegex.Match(arg);

				if (String.IsNullOrEmpty(m.Groups[2].Value)) {
					this.Add(i.ToString(), m.Groups[1].Value);
				} else {
					this.Add(m.Groups[1].Value, m.Groups[3].Value);
				}
			}
			int auxInt;
			this._UnNamedParameters = this.Where((KeyValuePair<string, string> kv) => int.TryParse(kv.Key,out auxInt)).ToList();
		}

		public List<KeyValuePair<string, string>> UnNamedParameters {
			get { return this._UnNamedParameters; }
		}

		public static string[] ArgumentsStringsSplit(string args, char separatorChar)
		{

			List<string> ret = new List<string>();

			Regex reg = null;
			reg = new Regex("(([^" + Regex.Escape("" + separatorChar) + "]+\\=[^" + Regex.Escape("" + separatorChar) + "\\\"]+)|([^" + Regex.Escape("" + separatorChar) + "]+\\=\\\"[^\\\"]+\\\"))|((\\\"([^\\\"\\=]+)\\\")|([^" + Regex.Escape("" + separatorChar) + "\\\"\\=]+))");
			MatchCollection mc = reg.Matches(args);
			foreach (Match m in mc) {
				String aux = m.Value.Replace("\\\"", "^");
				aux = aux.Replace("\"", "");
				aux.Replace("^", "\"");
				ret.Add(aux);
			}

			return ret.ToArray();
		}

		public static string readConfigFromFile(FileInfo cfgFile)
		{
			StreamReader sr = null;
			string ret = "";

			try {
				if ((cfgFile.Exists)) {
					sr = cfgFile.OpenText();
					// Leemos linea a linea y pasamos las que sean comentarios
					ret = sr.ReadLine();
					while (((ret != null) && ret.StartsWith("#"))) {
						ret = sr.ReadLine();
					}

					if (((ret != null))) {
						ret = ret.Trim();
					} else {
						ret = "";
					}
				}

			} catch  {
				ret = "";
			} finally {
				if (((sr != null))) {
					sr.Close();
				}
			}

			return ret;
		}

        public static String[] FromString(String sParams, char separator=' ', char escapeChar='\\') {
            IList<String> ret = new List<String>();
            String param = null;
            bool inQuotes = false;

            sParams = sParams.Trim();

            if (String.IsNullOrEmpty(sParams))
                return new string[] { };

            int i = 0;
            char c = '\0';
            char oldc='\0';

            while (i < sParams.Length){
                oldc = c;
                c= sParams[i];
                i++;
                if (c == '\'' || c == '\"'){
                    if (oldc == escapeChar)
                        param = param.Substring(0, param.Length - 1);
                    else
                    {
                        inQuotes = !inQuotes;
                        continue;
                    }

                }
                else if (c==separator){
                    if (!inQuotes){
                        if (oldc != escapeChar)
                        {
                            if (!String.IsNullOrEmpty(param))
                                ret.Add(param);
                            param = null;
                            continue;
                        }
                        else
                            param = param.Substring(0, param.Length - 1);
                    }
                }
                param += c;
            }

            if (!String.IsNullOrEmpty(param))
                ret.Add(param);

            return ret.ToArray();
        }
    }
}