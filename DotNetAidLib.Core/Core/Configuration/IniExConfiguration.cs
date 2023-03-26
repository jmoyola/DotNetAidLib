using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
    public class IniExConfiguration : CaptionDictionaryList<string, CaptionDictionaryList<string, string>>
    {
        public IList<char> CommentChars { get; } = new List<char> {'#', '\\', ';', '\''};

        public void Load(StreamReader sr)
        {
            var commentPattern =
                new Regex(@"^[" + CommentChars.Select(v => Regex.Escape("" + v)).ToStringJoin() + "](.*)$");
            var groupPattern = new Regex(@"^\s*\[\s*([^\]\s]+)\s*\]");
            var kvPattern = new Regex(@"^\s*([^" + CommentChars.Select(v => Regex.Escape("" + v)).ToStringJoin() +
                                      @"\[\n][^=\n\s]+)(\s*=\s*([^\s]*)\s*)?$");

            CaptionDictionaryList<string, string> currentGroup = null;
            try
            {
                Assert.NotNullOrEmpty(CommentChars, "CommentChars");
                Assert.NotNull(sr, nameof(sr));
                string comment = null;

                // root group (noname)
                currentGroup = new CaptionDictionaryList<string, string>();
                Add("", currentGroup);

                var li = sr.ReadLine();

                while (li != null)
                {
                    if (commentPattern.IsMatch(li))
                    {
                        // Comentario
                        comment += (string.IsNullOrEmpty(comment) ? "" : "\n")
                                   + commentPattern.Match(li).Groups[1].Value;
                    }
                    else if (groupPattern.IsMatch(li))
                    {
                        // Grupo
                        currentGroup = new CaptionDictionaryList<string, string>();
                        Add(groupPattern.Match(li).Groups[1].Value, currentGroup);
                        currentGroup.Caption = comment;
                        comment = null;
                    }
                    else if (kvPattern.IsMatch(li))
                    {
                        var kvMatch = kvPattern.Match(li);
                        currentGroup.Add(new CaptionKeyValue<string, string>(kvMatch.Groups[1].Value,
                            string.IsNullOrEmpty(kvMatch.Groups[2].Value) ? null : kvMatch.Groups[3].Value, comment));
                        comment = null;
                    }

                    li = sr.ReadLine();
                }

                if (this[""].Count == 0) // Si el grupo sin nombre está vacío, se elimina
                    Remove("");
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
                Assert.NotNullOrEmpty(CommentChars, "CommentChars");
                Assert.NotNull(sw, nameof(sw));

                foreach (var kvGroup in this)
                {
                    if (!string.IsNullOrEmpty(kvGroup.Value.Caption))
                        kvGroup.Value.Caption
                            .Split('\n')
                            .Select(v => CommentChars[0] + v)
                            .ToList().ForEach(v => sw.WriteLine(v));

                    if (!string.IsNullOrEmpty(kvGroup.Key))
                        sw.WriteLine("[" + kvGroup.Key + "]");
                    foreach (var ckv in this[kvGroup.Key])
                    {
                        if (!string.IsNullOrEmpty(ckv.Caption))
                            ckv.Caption
                                .Split('\n')
                                .Select(v => CommentChars[0] + v)
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

        public CaptionDictionaryList<string, string> AddGroup(string groupName)
        {
            var ret = new CaptionDictionaryList<string, string>();
            Add(groupName, ret);
            return ret;
        }

        public CaptionDictionaryList<string, string> GetGroup(string groupName)
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

                Save(sw);
                sw.Flush();

                ms.Seek(0, SeekOrigin.Begin);

                sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
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