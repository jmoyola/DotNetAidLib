using System;
using System.Linq;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	using System.Globalization;
    using System.IO;
    using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
    using Core.Collections;
    using Core.Develop;
    using Core.Serializer;

    public class KeyValueConfiguration: CaptionDictionaryList<String, Object>
	{
        private string _Syntax = @"Key-value configuration file.
Sintax ([optional value]):
 1- Line comments begin with '#' character.
 2- Key value line entry is 'key = value' ";

        public static readonly char[] DEFAULT_COMMENTCHARS = new char[] { '#', ';' };
        public static readonly char[] DEFAULT_ASSIGNATIONCHARS = new char[] { '=', ':' };

        private char[] _CommentChars;
        private String _Header;
        private char[] _AssignationChar;
        private Object oBlock = new Object();
        private IStringParser stringParser = null;

        public KeyValueConfiguration(IStringParser stringParser=null) {
			this.CommentChars = DEFAULT_COMMENTCHARS;
            this.AssignationChar = DEFAULT_ASSIGNATIONCHARS;
            
            this.stringParser = (stringParser == null?SimpleStringParser.Instance():stringParser);
            
            this._Syntax += this.stringParser.Syntax;
            this.Header = _Syntax;
        }

		public char[] CommentChars
		{
			get{ return _CommentChars; }
			set{
                Assert.NotNullOrEmpty( value, nameof(value));
				_CommentChars = value;
			}
		}

        public char[] AssignationChar{
            get{ return _AssignationChar;}
            set{
                Assert.NotNullOrEmpty( value, nameof(value));
                _AssignationChar = value;
            }
        }

        public String Header{
            get { return _Header; }
            set { _Header = value; }
        }

        public T GetItem<T>(String key)
        {
            return (T)this[key];
        }

        public void Load(Stream stream)
        {
            lock (oBlock)
            {

                try
                {
                    Assert.NotNull( stream, nameof(stream));

                    using (StreamReader sr = new StreamReader(stream)){
                        this.Load(sr);
                    }
                }
                catch (Exception ex){
                    throw new Exception("Error loading configuration from stream.", ex);
                }
            }
        }


        public void Load(String content)
        {
            using (var sr = content.ToReader()){
                this.Load(sr);
            }
        }


        public void Load(StreamReader sr){
            lock (oBlock)
            {
                Regex commentRegex = new Regex(@"^\s*[" + Regex.Escape(_CommentChars.ToStringJoin()) + @"]+\s*(.+)$");
                Regex keyValueRegex = new Regex(@"^\s*([^" + Regex.Escape(_CommentChars.ToStringJoin()) + Regex.Escape(_AssignationChar.ToStringJoin()) + @"\s]+)+\s*([" + Regex.Escape(_AssignationChar.ToStringJoin()) + @"]\s*(.+))?$");

                try
                {
                    Assert.NotNull( sr, nameof(sr));

                    this.Clear();

                    bool isHeader = true;
                    String line = sr.ReadLine();
                    String comment = null; ;

                    while (line!=null)
                    {
                        if (commentRegex.IsMatch(line))
                            comment += (comment == null ? "" : Environment.NewLine) + commentRegex.Match(line).Groups[1].Value;
                        else if (keyValueRegex.IsMatch(line))
                        {
                            Match kvm = keyValueRegex.Match(line);
                            this.Add(new CaptionKeyValue<string, object>(
                                kvm.Groups[1].Value.Trim(),
                                this.stringParser.Unparse(kvm.Groups[3].Value.Trim()),
                                Caption = comment
                            ));
                            comment = null;
                            isHeader = false;
                        }
                        else
                        {
                            if (isHeader){
                                this._Header = comment;
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
            lock (oBlock){
                try
                {
                    Assert.NotNull( stream, nameof(stream));

                    using (StreamWriter sw = new StreamWriter(stream)){
                        this.Save(sw, includeHeader);
                        sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving configuration to stream.", ex);
                }
            }
        }

        public void Save(StreamWriter sw, bool includeHeader=false)
		{
            lock (oBlock)
            {
                try
                {
                    Assert.NotNull( sw, nameof(sw));

                    if (includeHeader && !String.IsNullOrEmpty(this._Header))
                    {
                        foreach (String line in _Header.GetLines())
                            sw.WriteLine(_CommentChars[0] + " " + line);
                        sw.WriteLine();
                    }

                    foreach (CaptionKeyValue<string, object> kv in this)
                    {
                        if (!String.IsNullOrEmpty(kv.Caption))
                            foreach (String commentLine in kv.Caption.Split('\n'))
                                sw.WriteLine(_CommentChars[0] + " " + commentLine.Replace("\r", ""));
                        sw.WriteLine(kv.Key + this._AssignationChar[0] + this.stringParser.Parse(kv.Value));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving configuration to stream writer.", ex);
                }
            }
        }

        public override string ToString(){
            return this.ToString(true);
        }

        public string ToString(bool includeHeader) {
            lock (oBlock)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    StreamWriter sw = new StreamWriter(ms);
                    this.Save(sw, includeHeader);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    StreamReader sr = new StreamReader(ms);
                    return sr.ReadToEnd();
                }
            }
        }
    }
}

