using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
    public class TabConfigurationFile : List<CaptiontItem<List<string>>>
    {
        private static readonly Dictionary<string, TabConfigurationFile> _Instances =
            new Dictionary<string, TabConfigurationFile>();

        private string _CommentChars;

        private string _SeparatorChars;
        private int _SeparatorLenght;

        private TabConfigurationFile(FileInfo file)
            : this(file, @"#\;", " \t", 4)
        {
        }

        private TabConfigurationFile(FileInfo file, string commentChars, string separatorChars, int separatorLenght)
        {
            CommentChars = commentChars;
            SeparatorChars = separatorChars;
            SeparatorLenght = separatorLenght;
            File = file;
        }

        public FileInfo File { get; set; }

        public string CommentChars
        {
            get => Regex.Unescape(_CommentChars);
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _CommentChars = Regex.Escape(value);
            }
        }

        public string SeparatorChars
        {
            get => Regex.Unescape(_SeparatorChars);
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _SeparatorChars = value;
            }
        }

        public int SeparatorLenght
        {
            get => _SeparatorLenght;

            set
            {
                Assert.GreaterThan(value, 0, nameof(value));
                _SeparatorLenght = value;
            }
        }

        public static TabConfigurationFile Instance(FileInfo file)
        {
            TabConfigurationFile ret = null;

            if (!_Instances.ContainsKey(file.FullName))
            {
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
            try
            {
                var regex = new Regex(@"([^" + Regex.Escape(_SeparatorChars) + "]+)([" + Regex.Escape(_SeparatorChars) +
                                      "]+)?");

                if (File == null)
                    throw new Exception("File is not set.");

                if (!File.Exists)
                    throw new Exception("File not exists.");

                Clear();

                sr = File.OpenText();

                var li = sr.ReadLine();
                ret = new CaptiontItem<List<string>>();
                Add(ret);
                while (li != null && li.Trim().Length > 0)
                {
                    if (li.TrimStart().StartsWith("#", StringComparison.InvariantCulture))
                    {
                        ret.Caption += (string.IsNullOrEmpty(ret.Caption) ? "" : Environment.NewLine) +
                                       li.Trim().Substring(1);
                    }
                    else
                    {
                        var mc = regex.Matches(li);
                        if (mc.Count > 0)
                        {
                            ret.Value = new List<string>();
                            foreach (Match m in mc)
                                ret.Value.Add(m.Groups[1].Value);

                            ret = new CaptiontItem<List<string>>();
                            Add(ret);
                        }
                    }

                    li = sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading configuration from '" + File.FullName + "' " + ex.Message, ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }


        public void Save()
        {
            StreamWriter sw = null;
            try
            {
                sw = File.CreateText();
                foreach (var line in this)
                {
                    if (line.Caption != null)
                        foreach (var commentLine in line.Caption.Split('\n'))
                            sw.WriteLine("# " + commentLine);
                    if (line.Value != null)
                        sw.WriteLine(line.Value.ToStringJoin(new string(_SeparatorChars[0], _SeparatorLenght)));
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
    }
}