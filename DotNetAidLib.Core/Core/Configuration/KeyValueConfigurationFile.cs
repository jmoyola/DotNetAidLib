using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
    public class KeyValueConfigurationFile : KeyValueConfiguration
    {
        private static readonly Dictionary<string, KeyValueConfigurationFile> _Instances =
            new Dictionary<string, KeyValueConfigurationFile>();

        private readonly object oBlock = new object();

        private KeyValueConfigurationFile(FileInfo file, char[] commentChars, char[] assignationChars)
        {
            Assert.NotNull(file, nameof(file));

            CommentChars = commentChars;
            AssignationChar = assignationChars;

            File = file;
        }

        public FileInfo File { get; set; }

        public static KeyValueConfigurationFile Instance(FileInfo file)
        {
            return Instance(file, DEFAULT_COMMENTCHARS, DEFAULT_ASSIGNATIONCHARS);
        }

        public static KeyValueConfigurationFile Instance(FileInfo file, char[] commentChars, char[] assignationChars)
        {
            KeyValueConfigurationFile ret = null;

            if (!_Instances.ContainsKey(file.FullName))
                _Instances.Add(file.FullName, new KeyValueConfigurationFile(file, commentChars, assignationChars));

            return ret = _Instances[file.FullName];
        }

        public void Load()
        {
            lock (oBlock)
            {
                try
                {
                    if (File == null)
                        throw new Exception("File is not set.");

                    File.Refresh();

                    if (!File.Exists)
                        return;

                    using (var sr = File.OpenText())
                    {
                        Load(sr);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error loading configuration from '" + File.FullName + "'.", ex);
                }
            }
        }


        public void Save(bool includeHeader = true)
        {
            lock (oBlock)
            {
                try
                {
                    using (var sw = File.CreateText())
                    {
                        Save(sw, includeHeader);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving configuration to '" + File.FullName + "'.", ex);
                }
            }
        }
    }
}