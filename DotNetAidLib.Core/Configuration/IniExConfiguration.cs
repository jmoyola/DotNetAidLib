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
    public class IniExConfiguration : CaptionDictionaryList<String, CaptionDictionaryList<string, string>>
    {

        private IList<char> _CommentChars = new List<char>() { '#', '\\', ';', '\'' };

        public IniExConfiguration()
        {
        }

        public IList<char> CommentChars
        {
            get
            {
                return _CommentChars;
            }
        }

        public void Load(StreamReader sr)
        {
            Regex commentPattern = new Regex(@"^[" + _CommentChars.Select(v => Regex.Escape("" + v)).ToStringJoin("") + "](.*)$");
            Regex groupPattern = new Regex(@"^\s*\[\s*([^\]\s]+)\s*\]");
            Regex kvPattern = new Regex(@"^\s*([^" + _CommentChars.Select(v => Regex.Escape("" + v)).ToStringJoin("") + @"\[\n][^=\n\s]+)(\s*=\s*([^\s]*)\s*)?$");

            CaptionDictionaryList<string, string> currentGroup = null;
            try
            {
                Assert.NotNullOrEmpty(this.CommentChars, "CommentChars");
                Assert.NotNull( sr, nameof(sr));
                string comment = null;

                // root group (noname)
                currentGroup = new CaptionDictionaryList<string, string>();
                this.Add("", currentGroup);

                string li = sr.ReadLine();

                while (li != null)
                {
                    if (commentPattern.IsMatch(li))
                    { // Comentario
                        comment += (String.IsNullOrEmpty(comment) ? "" : "\n")
                            + commentPattern.Match(li).Groups[1].Value;
                    }
                    else if (groupPattern.IsMatch(li))
                    { // Grupo
                        currentGroup = new CaptionDictionaryList<string, string>();
                        this.Add(groupPattern.Match(li).Groups[1].Value, currentGroup);
                        currentGroup.Caption = comment;
                        comment = null;
                    }
                    else if (kvPattern.IsMatch(li))
                    {
                        Match kvMatch = kvPattern.Match(li);
                        currentGroup.Add(new CaptionKeyValue<String, String>(kvMatch.Groups[1].Value, (String.IsNullOrEmpty(kvMatch.Groups[2].Value)?null:kvMatch.Groups[3].Value), comment));
                        comment = null;
                    }

                    li = sr.ReadLine();
                }

                if (this[""].Count == 0) // Si el grupo sin nombre está vacío, se elimina
                    this.Remove("");

            }
            catch (Exception ex)
            {
                throw new Exception("Error loading configuration from stream.", ex);
            }
        }

        public void Save(StreamWriter sw)
        {
            try
            {
                Assert.NotNullOrEmpty(this.CommentChars, "CommentChars");
                Assert.NotNull( sw, nameof(sw));

                foreach (CaptionKeyValue <String, CaptionDictionaryList<string, string>> kvGroup in this)
                {
                    if (!String.IsNullOrEmpty(kvGroup.Value.Caption))
                        kvGroup.Value.Caption
                            .Split('\n')
                            .Select(v => this.CommentChars[0] + v)
                            .ToList().ForEach(v=>sw.WriteLine(v));

                    if (!string.IsNullOrEmpty(kvGroup.Key))
                        sw.WriteLine("[" + kvGroup.Key + "]");
                    foreach (CaptionKeyValue<string, string> ckv in this[kvGroup.Key])
                    {
                        if (!String.IsNullOrEmpty(ckv.Caption))
                            ckv.Caption
                                .Split('\n')
                                .Select(v => this.CommentChars[0] + v)
                                .ToList().ForEach(v => sw.WriteLine(v));

                        if (ckv.Value == null)
                            sw.WriteLine(ckv.Key);
                        else
                            sw.WriteLine(ckv.Key + "=" + ckv.Value);
                    }
                }
                sw.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving configuration to stream.", ex);
            }
        }

        public CaptionDictionaryList<String, String> AddGroup(String groupName)
        {
            CaptionDictionaryList<String, String> ret = new CaptionDictionaryList<String, String>();
            this.Add(groupName, ret);
            return ret;
        }

        public CaptionDictionaryList<String, String> GetGroup(String groupName)
        {
            return this[groupName];
        }

        public override string ToString()
        {
            MemoryStream ms = null;
            StreamWriter sw = null;
            StreamReader sr = null;
            try
            {
                ms = new MemoryStream();
                sw = new StreamWriter(ms);

                this.Save(sw);
                sw.Flush();

                ms.Seek(0, SeekOrigin.Begin);

                sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
            catch(Exception ex) {
                throw ex;
            }
            finally {
                if (sw != null)
                    sw.Close();
                if (sr != null)
                    sr.Close();
                if (ms != null)
                    ms.Close();
            }
        }
    }
}