using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Helpers;
using System.Text.RegularExpressions;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class DictionaryDBException : Exception
    {
        public DictionaryDBException(){}
        public DictionaryDBException(string message) : base(message){}
        public DictionaryDBException(string message, Exception innerException) : base(message, innerException){}
        protected DictionaryDBException(SerializationInfo info, StreamingContext context) : base(info, context){}
    }

    public class DictionaryDB:Dictionary<String, String>
    {
        private static Regex kvPattern = new Regex(@"^\s*([a-zA-Z0-9@_\.^=]+)\s*=\s*([^\n=]*)\s*$", RegexOptions.Multiline);
        private static IDictionary<String, DictionaryDB> instances = new Dictionary<String, DictionaryDB>();
        private Object oLock = new object();

        private FileInfo file;
        private DictionaryDB(FileInfo file)
        {
            Assert.NotNull( file, nameof(file));
            
            this.file = file;
            if (this.file.Exists)
                this.Load();
            else
                this.file.Create().Close();
        }

        public String Get(String key){
            return this.Get(key, null);
        }

        public String Get(String key, String defaultValueIfNotExists) {
            if (!this.ContainsKey(key))
                return defaultValueIfNotExists;
            else
                return this[key];
        }

        public void Set(String key, String value)
        {
            if (!this.ContainsKey(key))
                this.Add(key, value);
            else
                this[key]=value;
        }

        public void Save() {
            lock (oLock)
            {
                try
                {
                    using (StreamWriter sw = this.file.CreateText()){
                        foreach (KeyValuePair<String, String> kv in this)
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
                    this.Clear();
                    /*String content = this.file.OpenText().ReadToEnd(true);
                    kvPattern.Matches(content)
                    .Cast<Match>().ToList()
                    .ForEach(m => this.Add(m.Groups[1].Value, m.Groups[2].Value));
                    */
                    using(StreamReader sr=this.file.OpenText()){
                        String line = sr.ReadLine();
                        while (line != null) {
                            if (!String.IsNullOrWhiteSpace(line)) {
                                String key = line;
                                String value = null;
                                int n = line.IndexOf("=", StringComparison.InvariantCulture);
                                if (n > -1){
                                    key = line.Substring(0, n);
                                    value = line.Substring(n+1);
                                }
                                this.Add(key, value);
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

        public static DictionaryDB Instance(FileInfo file, String instanceKey="__DEFAULT__")
        {
            Assert.NotNull( file, nameof(file));
            Assert.NotNullOrEmpty( instanceKey, nameof(instanceKey));

            if (instances.ContainsKey(instanceKey))
                throw new DictionaryDBException("Instance '" + instanceKey + "' is already initialized.");

            instances.Add(instanceKey, new DictionaryDB(file));

            return instances[instanceKey];
        }

        public static DictionaryDB Instance(String instanceKey = "__DEFAULT__")
        {
            Assert.NotNullOrEmpty( instanceKey, nameof(instanceKey));

            if (!instances.ContainsKey(instanceKey))
                throw new DictionaryDBException("Instance '" + instanceKey + "' is not initialized.");
                
            return instances[instanceKey];
        }

        public static String GetFullPath(String keyFullPath, String defaultValueIfNotExists=null, String keyFullPathPattern = @"^/([^\s/]*)/(\S+)$")
        {
            Regex keyFullPathRegex = new Regex(keyFullPathPattern);

            if(keyFullPathRegex.GetGroupNumbers().Length<3)
                throw new DictionaryDBException("keyFullPathRegex must have 3 group: group index 1 for instanceName and group index 2 for key name.");

            DictionaryDB instance;

            Match m = keyFullPathRegex.Match(keyFullPath);
            if (m.Success)
            {
                String sInstance = m.Groups[1].Value;
                String key = m.Groups[2].Value;
                if (String.IsNullOrEmpty(sInstance))
                    instance = DictionaryDB.Instance();
                else
                    instance = DictionaryDB.Instance(sInstance);

                return instance.Get(key, defaultValueIfNotExists);
            }
            else
                throw new DictionaryDBException("No valid keyFullPath '" + keyFullPath + "' for keyFullPathPattern '" + keyFullPathPattern + "'.");
        }
    }
}
