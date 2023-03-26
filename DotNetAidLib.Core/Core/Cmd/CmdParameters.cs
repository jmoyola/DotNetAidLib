using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Core.Cmd
{
    public class CmdParameters : Dictionary<string, string>
    {
        private static readonly Regex keyValueRegex = new Regex(@"^-?[-/]([^=:]+)([=:](.+))?$");

        public CmdParameters(FileInfo cfgFile) : this(readConfigFromFile(cfgFile))
        {
        }

        public CmdParameters(string args) : this(ArgumentsStringsSplit(args, ' '))
        {
        }

        public CmdParameters(string[] args)
        {
            for (var i = 0; i <= args.Length - 1; i++)
            {
                var arg = args[i];
                var m = keyValueRegex.Match(arg);

                if (string.IsNullOrEmpty(m.Groups[2].Value))
                    Add(i.ToString(), m.Groups[1].Value);
                else
                    Add(m.Groups[1].Value, m.Groups[3].Value);
            }

            int auxInt;
            UnNamedParameters = this.Where(kv => int.TryParse(kv.Key, out auxInt)).ToList();
        }

        public List<KeyValuePair<string, string>> UnNamedParameters { get; }

        public static string[] ArgumentsStringsSplit(string args, char separatorChar)
        {
            var ret = new List<string>();

            Regex reg = null;
            reg = new Regex("(([^" + Regex.Escape("" + separatorChar) + "]+\\=[^" + Regex.Escape("" + separatorChar) +
                            "\\\"]+)|([^" + Regex.Escape("" + separatorChar) +
                            "]+\\=\\\"[^\\\"]+\\\"))|((\\\"([^\\\"\\=]+)\\\")|([^" + Regex.Escape("" + separatorChar) +
                            "\\\"\\=]+))");
            var mc = reg.Matches(args);
            foreach (Match m in mc)
            {
                var aux = m.Value.Replace("\\\"", "^");
                aux = aux.Replace("\"", "");
                aux.Replace("^", "\"");
                ret.Add(aux);
            }

            return ret.ToArray();
        }

        public static string readConfigFromFile(FileInfo cfgFile)
        {
            StreamReader sr = null;
            var ret = "";

            try
            {
                if (cfgFile.Exists)
                {
                    sr = cfgFile.OpenText();
                    // Leemos linea a linea y pasamos las que sean comentarios
                    ret = sr.ReadLine();
                    while (ret != null && ret.StartsWith("#")) ret = sr.ReadLine();

                    if (ret != null)
                        ret = ret.Trim();
                    else
                        ret = "";
                }
            }
            catch
            {
                ret = "";
            }
            finally
            {
                if (sr != null) sr.Close();
            }

            return ret;
        }

        public static string[] FromString(string sParams, char separator = ' ', char escapeChar = '\\')
        {
            IList<string> ret = new List<string>();
            string param = null;
            var inQuotes = false;

            sParams = sParams.Trim();

            if (string.IsNullOrEmpty(sParams))
                return new string[] { };

            var i = 0;
            var c = '\0';
            var oldc = '\0';

            while (i < sParams.Length)
            {
                oldc = c;
                c = sParams[i];
                i++;
                if (c == '\'' || c == '\"')
                {
                    if (oldc == escapeChar)
                    {
                        param = param.Substring(0, param.Length - 1);
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                        continue;
                    }
                }
                else if (c == separator)
                {
                    if (!inQuotes)
                    {
                        if (oldc != escapeChar)
                        {
                            if (!string.IsNullOrEmpty(param))
                                ret.Add(param);
                            param = null;
                            continue;
                        }

                        param = param.Substring(0, param.Length - 1);
                    }
                }

                param += c;
            }

            if (!string.IsNullOrEmpty(param))
                ret.Add(param);

            return ret.ToArray();
        }
    }
}