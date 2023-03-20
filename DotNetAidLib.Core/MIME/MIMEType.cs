using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.MIME
{
    public class MIMEType
    {
        private static MIMEType instance = null;
        private List<String> mimeTypes = new List<string>();
        private List<String> extensions = new List<string>();
        private Dictionary<int, int> extToMime = new Dictionary<int, int>();

        private MIMEType()
        {
            this.LoadFromResource(Assembly.GetExecutingAssembly(), "Library.MIME.mime.types");
        }

        private MIMEType(FileInfo mimeTypesFile){
            this.LoadFromFile(mimeTypesFile);
        }

        public void LoadFromResource(Assembly assembly, String resourcePath) {
            Assert.NotNull( assembly, nameof(assembly));
            Assert.NotNullOrEmpty( resourcePath, nameof(resourcePath));
            
            StreamReader sr = null;
            try
            {
                Stream stream = assembly.GetManifestResourceStream(resourcePath);
                sr = new StreamReader(stream);
                this.LoadFromStream(sr);
            }
            catch (Exception ex)
            {
                throw new MIMETypeException("Error loading mime type type file from resource '" + resourcePath + "' in assembly '" + assembly.GetName().FullName + "'.", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public void LoadFromFile(FileInfo mimeFile) {
            Assert.Exists( mimeFile, nameof(mimeFile));
            
            StreamReader sr = null;
            try
            {
                sr = mimeFile.OpenText();
                this.LoadFromStream(sr);
            }
            catch (Exception ex)
            {
                throw new MIMETypeException("Error loading mime type file.", ex);
            }
            finally {
                if (sr != null)
                    sr.Close();
            }
        }

        public void LoadFromStream(StreamReader sr)
        {
            Assert.NotNull( sr, nameof(sr));
            
            Regex r = new Regex(@"^([^\s]+)\t+(.*)$");

            try
            {
                String line = sr.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("#", StringComparison.InvariantCulture))
                    {
                        Match m = r.Match(line);
                        if (m.Success)
                        {
                            mimeTypes.Add(m.Groups[1].Value);
                            foreach (String ex in m.Groups[2].Value.Split(' '))
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

        public IList<String> Extensions {
            get { return this.extensions; }
        }

        public IList<String> MimeTypes{
            get { return this.mimeTypes; }
        }

        public String MimeTypeFromFileName (String path) {
            Assert.NotNullOrEmpty( path, nameof(path));
            
            return MimeTypeFromExtension (new FileInfo (path).Extension);
        }

        public String MimeTypeFromExtension(String extension)
        {
            String ret="unknow";

            if (!String.IsNullOrEmpty(extension))
            {
                extension = extension.Trim().Replace(".", "");
                
                int iExtension = extensions.IndexOf(extension.Trim().ToLower());
                if (iExtension > -1)
                    ret = mimeTypes[extToMime[iExtension]];
            }

            return ret;
        }

        public IEnumerable<String> ExtensionsFromMimeType(String mimeType)
        {
            Assert.NotNullOrEmpty( mimeType, nameof(mimeType));
            
            IEnumerable<String> ret = new List<String>();
            int iMIMEType = mimeTypes.IndexOf(mimeType.Trim().ToLower());
            if (iMIMEType > -1)
                ret = extToMime.Where(v=>v.Value==iMIMEType).Select(v=>extensions[v.Key]);

            return ret;
        }

        public static MIMEType Instance(FileInfo mimeTypesFile) {
            Assert.Exists( mimeTypesFile, nameof(mimeTypesFile));
            
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
