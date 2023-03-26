using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Serializer;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Configuration
{
    public class KeyValueConfiguration : CaptionDictionaryList<string, object>
    {
        public static readonly char[] DEFAULT_COMMENTCHARS = {'#', ';'};
        public static readonly char[] DEFAULT_ASSIGNATIONCHARS = {'=', ':'};
        private char[] _AssignationChar;

        private char[] _CommentChars;

        private readonly string _Syntax = @"Key-value configuration file.
Sintax ([optional value]):
 1- Line comments begin with '#' character.
 2- Key value line entry is 'key = value' ";

        private readonly object oBlock = new object();
        private readonly IStringParser stringParser;

        public KeyValueConfiguration(IStringParser stringParser = null)
        {
            CommentChars = DEFAULT_COMMENTCHARS;
            AssignationChar = DEFAULT_ASSIGNATIONCHARS;

            this.stringParser = stringParser == null ? SimpleStringParser.Instance() : stringParser;

            _Syntax += this.stringParser.Syntax;
            Header = _Syntax;
        }

        public char[] CommentChars
        {
            get => _CommentChars;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _CommentChars = value;
            }
        }

        public char[] AssignationChar
        {
            get => _AssignationChar;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                _AssignationChar = value;
            }
        }

        public string Header { get; set; }

        public T GetItem<T>(string key)
        {
            return (T) this[key];
        }

        public void Load(Stream stream)
        {
            lock (oBlock)
            {
                try
                {
                    Assert.NotNull(stream, nameof(stream));

                    using (var sr = new StreamReader(stream))
                    {
                        Load(sr);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error loading configuration from stream.", ex);
                }
            }
        }


        public void Load(string content)
        {
            using (var sr = content.ToReader())
            {
                Load(sr);
            }
        }


        public void Load(StreamReader sr)
        {
            lock (oBlock)
            {
                var commentRegex = new Regex(@"^\s*[" + Regex.Escape(_CommentChars.ToStringJoin()) + @"]+\s*(.+)$");
                var keyValueRegex = new Regex(@"^\s*([^" + Regex.Escape(_CommentChars.ToStringJoin()) +
                                              Regex.Escape(_AssignationChar.ToStringJoin()) + @"\s]+)+\s*([" +
                                              Regex.Escape(_AssignationChar.ToStringJoin()) + @"]\s*(.+))?$");

                try
                {
                    Assert.NotNull(sr, nameof(sr));

                    Clear();

                    var isHeader = true;
                    var line = sr.ReadLine();
                    string comment = null;
                    ;

                    while (line != null)
                    {
                        if (commentRegex.IsMatch(line))
                        {
                            comment += (comment == null ? "" : Environment.NewLine) +
                                       commentRegex.Match(line).Groups[1].Value;
                        }
                        else if (keyValueRegex.IsMatch(line))
                        {
                            var kvm = keyValueRegex.Match(line);
                            Add(new CaptionKeyValue<string, object>(
                                kvm.Groups[1].Value.Trim(),
                                stringParser.Unparse(kvm.Groups[3].Value.Trim()),
                                Caption = comment
                            ));
                            comment = null;
                            isHeader = false;
                        }
                        else
                        {
                            if (isHeader)
                            {
                                Header = comment;
                                isHeader = false;
                            }

                            comment = null;
                        }


                        line = sr.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error loading configuration from stream reader.", ex);
                }
            }
        }

        public void Save(Stream stream, bool includeHeader = false)
        {
            lock (oBlock)
            {
                try
                {
                    Assert.NotNull(stream, nameof(stream));

                    using (var sw = new StreamWriter(stream))
                    {
                        Save(sw, includeHeader);
                        sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving configuration to stream.", ex);
                }
            }
        }

        public void Save(StreamWriter sw, bool includeHeader = false)
        {
            lock (oBlock)
            {
                try
                {
                    Assert.NotNull(sw, nameof(sw));

                    if (includeHeader && !string.IsNullOrEmpty(Header))
                    {
                        foreach (var line in Header.GetLines())
                            sw.WriteLine(_CommentChars[0] + " " + line);
                        sw.WriteLine();
                    }

                    foreach (var kv in this)
                    {
                        if (!string.IsNullOrEmpty(kv.Caption))
                            foreach (var commentLine in kv.Caption.Split('\n'))
                                sw.WriteLine(_CommentChars[0] + " " + commentLine.Replace("\r", ""));
                        sw.WriteLine(kv.Key + _AssignationChar[0] + stringParser.Parse(kv.Value));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving configuration to stream writer.", ex);
                }
            }
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool includeHeader)
        {
            lock (oBlock)
            {
                using (var ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);
                    Save(sw, includeHeader);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    var sr = new StreamReader(ms);
                    return sr.ReadToEnd();
                }
            }
        }
    }
}