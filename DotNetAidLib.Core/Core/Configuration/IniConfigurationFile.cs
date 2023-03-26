using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
    public class IniConfigurationFile : Dictionary<string, Dictionary<string, string>>
    {
        private static readonly Dictionary<string, IniConfigurationFile> _Instances =
            new Dictionary<string, IniConfigurationFile>();

        private static readonly Regex keyValueRegex = new Regex(@"^\s*([^=\s]+)\s*(=\s*(.*))?$");
        private static readonly Regex iniGroupRegex = new Regex(@"^\s*\[\s*([^\s]+)\s*\]\s*$");
        private string _CommentChars;

        private IniConfigurationFile(FileInfo file)
            : this(file, @"#\;")
        {
        }

        private IniConfigurationFile(FileInfo file, string commentChars)
        {
            CommentChars = commentChars;
            File = file;
        }

        public FileInfo File { get; set; }

        public string CommentChars
        {
            get => Regex.Unescape(_CommentChars);
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new Exception("CommentChars can't be null or empty.");
                _CommentChars = Regex.Escape(value);
            }
        }

        public static IniConfigurationFile Instance(FileInfo file)
        {
            Assert.NotNull(file, nameof(file));

            IniConfigurationFile ret = null;

            if (_Instances.ContainsKey(file.FullName))
            {
                ret = _Instances[file.FullName];
            }
            else
            {
                ret = new IniConfigurationFile(file);
                _Instances.Add(file.FullName, ret);
            }

            return ret;
        }

        public void Load()
        {
            Dictionary<string, string> ret = null;
            StreamReader sr = null;
            try
            {
                if (File == null)
                    throw new Exception("File is not set.");

                if (!File.Exists)
                    throw new Exception("File not exists.");

                Clear();
                ret = new Dictionary<string, string>();
                Add("", ret);

                sr = File.OpenText();

                var li = sr.ReadLine();

                while (li != null)
                {
                    if (li.Trim().Length > 0 && !li.RegexIsMatch(@"^\s*[" + _CommentChars + "].*$"))
                    {
                        if (iniGroupRegex.IsMatch(li)) // Si es un grupo
                        {
                            ret = new Dictionary<string, string>();
                            Add(iniGroupRegex.Match(li).Groups[1].Value, ret);
                        }
                        else if (keyValueRegex.IsMatch(li)) // Si es un key value
                        {
                            var keyValueMatch = keyValueRegex.Match(li);
                            if (string.IsNullOrEmpty(keyValueMatch.Groups[2].Value)) // si no hay value
                                ret.Add(keyValueMatch.Groups[1].Value.Trim(), null);
                            else
                                ret.Add(keyValueMatch.Groups[1].Value.Trim(), keyValueMatch.Groups[3].Value.Trim());
                        }
                    }

                    li = sr.ReadLine();
                }

                if (this[""].Keys.Count == 0) // Si el grupo sin nombre está vacío, se elimina
                    Remove("");
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading configuration from '" + File.FullName + "' " + ex.Message, ex);
            }
            finally
            {
                try
                {
                    sr.Close();
                }
                catch
                {
                }
            }
        }


        public void Save()
        {
            StreamWriter sw = null;
            try
            {
                sw = File.CreateText();

                foreach (var key in Keys)
                {
                    if (!string.IsNullOrEmpty(key))
                        sw.WriteLine("[" + key + "]");
                    foreach (var kv in this[key])
                        if (kv.Value == null)
                            sw.WriteLine(kv.Key);
                        else
                            sw.WriteLine(kv.Key + "=" + kv.Value);
                }

                sw.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving configuration to '" + File.FullName + "': " + ex.Message, ex);
            }
            finally
            {
                try
                {
                    sw.Close();
                }
                catch
                {
                }
            }
        }

        public Dictionary<string, string> AddGroup(string groupName)
        {
            var ret = new Dictionary<string, string>();
            Add(groupName, ret);
            return ret;
        }

        public Dictionary<string, string> GetGroup(string groupName)
        {
            return this[groupName];
        }

        public bool ContainsGroup(string groupName)
        {
            return ContainsKey(groupName);
        }
    }
}