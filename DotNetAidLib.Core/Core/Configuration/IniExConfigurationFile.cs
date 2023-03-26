using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Configuration
{
    public class IniExConfigurationFile : IniExConfiguration
    {
        private static readonly Dictionary<string, IniExConfigurationFile> _Instances =
            new Dictionary<string, IniExConfigurationFile>();

        private IniExConfigurationFile(FileInfo file)
        {
            Assert.NotNull(file, nameof(file));
            File = file;
        }

        public FileInfo File { get; }

        public static IniExConfigurationFile Instance(FileInfo file)
        {
            if (!_Instances.ContainsKey(file.FullName))
                _Instances.Add(file.FullName, new IniExConfigurationFile(file));

            return _Instances[file.FullName];
        }

        public void Load()
        {
            StreamReader sr = null;

            try
            {
                if (File == null)
                    throw new Exception("File is not set.");

                if (!File.Exists)
                    throw new Exception("File not exists.");

                Clear();

                sr = File.OpenText();
                Load(sr);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading configuration from '" + File.FullName + "' " + ex.Message, ex);
            }
            finally
            {
                try
                {
                    if (sr != null)
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

                Save(sw);
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
                    if (sw != null)
                        sw.Close();
                }
                catch
                {
                }
            }
        }
    }
}