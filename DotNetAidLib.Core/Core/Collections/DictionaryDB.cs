using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class DictionaryDBException : Exception
    {
        public DictionaryDBException()
        {
        }

        public DictionaryDBException(string message) : base(message)
        {
        }

        public DictionaryDBException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DictionaryDBException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DictionaryDB : Dictionary<string, string>
    {
        private static Regex kvPattern =
            new Regex(@"^\s*([a-zA-Z0-9@_\.^=]+)\s*=\s*([^\n=]*)\s*$", RegexOptions.Multiline);

        private static readonly IDictionary<string, DictionaryDB> instances = new Dictionary<string, DictionaryDB>();

        private readonly FileInfo file;
        private readonly object oLock = new object();

        private DictionaryDB(FileInfo file)
        {
            Assert.NotNull(file, nameof(file));

            this.file = file;
            if (this.file.Exists)
                Load();
            else
                this.file.Create().Close();
        }

        public string Get(string key)
        {
            return Get(key, null);
        }

        public string Get(string key, string defaultValueIfNotExists)
        {
            if (!ContainsKey(key))
                return defaultValueIfNotExists;
            return this[key];
        }

        public void Set(string key, string value)
        {
            if (!ContainsKey(key))
                Add(key, value);
            else
                this[key] = value;
        }

        public void Save()
        {
            lock (oLock)
            {
                try
                {
                    using (var sw = file.CreateText())
                    {
                        foreach (var kv in this)
                            sw.WriteLine(kv.Key + "=" + kv.Value);
                        sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    throw new DictionaryDBException("Error saving to file.", ex);
                }
            }
        }

        public void Load()
        {
            lock (oLock)
            {
                try
                {
                    Clear();
                    /*String content = this.file.OpenText().ReadToEnd(true);
                    kvPattern.Matches(content)
                    .Cast<Match>().ToList()
                    .ForEach(m => this.Add(m.Groups[1].Value, m.Groups[2].Value));
                    */
                    using (var sr = file.OpenText())
                    {
                        var line = sr.ReadLine();
                        while (line != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var key = line;
                                string value = null;
                                var n = line.IndexOf("=", StringComparison.InvariantCulture);
                                if (n > -1)
                                {
                                    key = line.Substring(0, n);
                                    value = line.Substring(n + 1);
                                }

                                Add(key, value);
                            }

                            line = sr.ReadLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DictionaryDBException("Error saving to file.", ex);
                }
            }
        }

        public static DictionaryDB Instance(FileInfo file, string instanceKey = "__DEFAULT__")
        {
            Assert.NotNull(file, nameof(file));
            Assert.NotNullOrEmpty(instanceKey, nameof(instanceKey));

            if (instances.ContainsKey(instanceKey))
                throw new DictionaryDBException("Instance '" + instanceKey + "' is already initialized.");

            instances.Add(instanceKey, new DictionaryDB(file));

            return instances[instanceKey];
        }

        public static DictionaryDB Instance(string instanceKey = "__DEFAULT__")
        {
            Assert.NotNullOrEmpty(instanceKey, nameof(instanceKey));

            if (!instances.ContainsKey(instanceKey))
                throw new DictionaryDBException("Instance '" + instanceKey + "' is not initialized.");

            return instances[instanceKey];
        }

        public static string GetFullPath(string keyFullPath, string defaultValueIfNotExists = null,
            string keyFullPathPattern = @"^/([^\s/]*)/(\S+)$")
        {
            var keyFullPathRegex = new Regex(keyFullPathPattern);

            if (keyFullPathRegex.GetGroupNumbers().Length < 3)
                throw new DictionaryDBException(
                    "keyFullPathRegex must have 3 group: group index 1 for instanceName and group index 2 for key name.");

            DictionaryDB instance;

            var m = keyFullPathRegex.Match(keyFullPath);
            if (m.Success)
            {
                var sInstance = m.Groups[1].Value;
                var key = m.Groups[2].Value;
                if (string.IsNullOrEmpty(sInstance))
                    instance = Instance();
                else
                    instance = Instance(sInstance);

                return instance.Get(key, defaultValueIfNotExists);
            }

            throw new DictionaryDBException("No valid keyFullPath '" + keyFullPath + "' for keyFullPathPattern '" +
                                            keyFullPathPattern + "'.");
        }
    }
}