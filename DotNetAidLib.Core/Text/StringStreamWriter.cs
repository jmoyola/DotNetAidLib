using System;
using System.IO;
using System.Text;

namespace DotNetAidLib.Core.Text
{
    public class StringStreamWriter:StreamWriter
    {
        public StringStreamWriter()
            : this(System.Text.Encoding.Default) { }

        public StringStreamWriter(Encoding encoding)
            :base(new MemoryStream(), encoding)
        {}


        public override string ToString()
        {
            StreamReader ret = null;
            try
            {
                this.Flush();
                this.BaseStream.Flush();
                this.BaseStream.Seek(0, SeekOrigin.Begin);
                ret = new StreamReader(this.BaseStream, this.Encoding);
                return ret.ReadToEnd();
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}
