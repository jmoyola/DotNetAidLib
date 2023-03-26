using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Configuration
{
    public class IniXConfigurationFile : Dictionary<string, DictionaryList<string, string>>
    {
        private static readonly Dictionary<string, IniXConfigurationFile> _Instances =
            new Dictionary<string, IniXConfigurationFile>();

        private string _CommentChars;

        private IniXConfigurationFile(FileInfo file)
            : this(file, @"#\;")
        {
        }

        private IniXConfigurationFile(FileInfo file, string commentChars)
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

        public static IniXConfigurationFile Instance(FileInfo file)
        {
            IniXConfigurationFile ret = null;

            if (_Instances.ContainsKey(file.FullName))
            {
                ret = _Instances[file.FullName];
            }
            else
            {
                ret = new IniXConfigurationFile(file);
                _Instances.Add(file.FullName, ret);
            }

            return ret;
        }

        public void Load()
        {
            DictionaryList<string, string> ret = null;
            StreamReader sr = null;
            try
            {
                if (File == null)
                    throw new Exception("File is not set.");

                if (!File.Exists)
                    throw new Exception("File not exists.");

                Clear();
                ret = new DictionaryList<string, string>();
                Add("", ret);

                sr = File.OpenText();

                var li = sr.ReadLine();

                while (li != null)
                {
                    if (li.Trim().Length > 0 && !li.RegexIsMatch(@"^\s*[" + _CommentChars + "].*$"))
                    {
                        var aLi = li.Split('=');
                        if (aLi.Length == 1)
                        {
                            if (aLi[0].RegexIsMatch(@"^\s*\[(.+)\]$"))
                            {
                                ret = new DictionaryList<string, string>();
                                Add(aLi[0].RegexGroupsMatches(@"^\s*\[(.+)\]$")[1], ret);
                            }
                            else
                            {
                                ret.Add(aLi[0].Trim(), null);
                            }
                        }
                        else if (aLi.Length > 1)
                        {
                            ret.Add(aLi[0].Trim(), aLi[1].Trim());
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

        public DictionaryList<string, string> AddGroup(string groupName)
        {
            var ret = new DictionaryList<string, string>();
            Add(groupName, ret);
            return ret;
        }

        public DictionaryList<string, string> GetGroup(string groupName)
        {
            return this[groupName];
        }
    }
}