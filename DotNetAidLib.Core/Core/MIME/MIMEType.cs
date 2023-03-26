using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.MIME
{
    public class MIMEType
    {
        private static MIMEType instance;
        private readonly List<string> extensions = new List<string>();
        private readonly Dictionary<int, int> extToMime = new Dictionary<int, int>();
        private readonly List<string> mimeTypes = new List<string>();

        private MIMEType()
        {
            LoadFromResource(Assembly.GetExecutingAssembly(), "DotNetAidLib.Core.MIME.mime.types");
        }

        private MIMEType(FileInfo mimeTypesFile)
        {
            LoadFromFile(mimeTypesFile);
        }

        public IList<string> Extensions => extensions;

        public IList<string> MimeTypes => mimeTypes;

        public void LoadFromResource(Assembly assembly, string resourcePath)
        {
            Assert.NotNull(assembly, nameof(assembly));
            Assert.NotNullOrEmpty(resourcePath, nameof(resourcePath));

            StreamReader sr = null;
            try
            {
                var stream = assembly.GetManifestResourceStream(resourcePath);
                sr = new StreamReader(stream);
                LoadFromStream(sr);
            }
            catch (Exception ex)
            {
                throw new MIMETypeException(
                    "Error loading mime type type file from resource '" + resourcePath + "' in assembly '" +
                    assembly.GetName().FullName + "'.", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public void LoadFromFile(FileInfo mimeFile)
        {
            Assert.Exists(mimeFile, nameof(mimeFile));

            StreamReader sr = null;
            try
            {
                sr = mimeFile.OpenText();
                LoadFromStream(sr);
            }
            catch (Exception ex)
            {
                throw new MIMETypeException("Error loading mime type file.", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public void LoadFromStream(StreamReader sr)
        {
            Assert.NotNull(sr, nameof(sr));

            var r = new Regex(@"^([^\s]+)\t+(.*)$");

            try
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("#", StringComparison.InvariantCulture))
                    {
                        var m = r.Match(line);
                        if (m.Success)
                        {
                            mimeTypes.Add(m.Groups[1].Value);
                            foreach (var ex in m.Groups[2].Value.Split(' '))
                            {
                                extensions.Add(ex);
                                extToMime.Add(extensions.Count - 1, mimeTypes.Count - 1);
                            }
                        }
                    }

                    line = sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new MIMETypeException("Error loading mime type from stream.", ex);
            }
        }

        public string MimeTypeFromFileName(string path)
        {
            Assert.NotNullOrEmpty(path, nameof(path));

            return MimeTypeFromExtension(new FileInfo(path).Extension);
        }

        public string MimeTypeFromExtension(string extension)
        {
            var ret = "unknow";

            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Trim().Replace(".", "");

                var iExtension = extensions.IndexOf(extension.Trim().ToLower());
                if (iExtension > -1)
                    ret = mimeTypes[extToMime[iExtension]];
            }

            return ret;
        }

        public IEnumerable<string> ExtensionsFromMimeType(string mimeType)
        {
            Assert.NotNullOrEmpty(mimeType, nameof(mimeType));

            IEnumerable<string> ret = new List<string>();
            var iMIMEType = mimeTypes.IndexOf(mimeType.Trim().ToLower());
            if (iMIMEType > -1)
                ret = extToMime.Where(v => v.Value == iMIMEType).Select(v => extensions[v.Key]);

            return ret;
        }

        public static MIMEType Instance(FileInfo mimeTypesFile)
        {
            Assert.Exists(mimeTypesFile, nameof(mimeTypesFile));

            if (instance == null)
                instance = new MIMEType(mimeTypesFile);

            return instance;
        }

        public static MIMEType Instance()
        {
            if (instance == null)
                instance = new MIMEType();

            return instance;
        }
    }
}