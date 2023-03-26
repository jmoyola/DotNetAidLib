using System;
using System.IO;
using System.Text;

namespace DotNetAidLib.Core.Text
{
    public class StringStreamWriter : StreamWriter
    {
        public StringStreamWriter()
            : this(Encoding.Default)
        {
        }

        public StringStreamWriter(Encoding encoding)
            : base(new MemoryStream(), encoding)
        {
        }


        public override string ToString()
        {
            StreamReader ret = null;
            try
            {
                Flush();
                BaseStream.Flush();
                BaseStream.Seek(0, SeekOrigin.Begin);
                ret = new StreamReader(BaseStream, Encoding);
                return ret.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}