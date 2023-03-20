using System;
using System.IO;
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Text
{
    public class StringStream:MemoryStream
    {
        private StreamReader sr=null;
        private StreamWriter sw=null;
        public StringStream()
            : this(System.Text.Encoding.UTF8) { }

        public StringStream(Encoding encoding) {
            Assert.NotNull(encoding, nameof(encoding));
            this.sr=new StreamReader(this, encoding);
            this.sw=new StreamWriter(this, encoding);
        }

        public StringStream(String value)
            :this(value, Encoding.UTF8) { }

        public StringStream(String value, Encoding encoding)
        :this(encoding){
            this.Write(value);
            this.Flush();
        }
        
        public String ReadLine() => sr.ReadLine();
        public int Read() => sr.Read();

        public void Reset()
        {
            this.Seek(0, SeekOrigin.Begin);
        }

        public void WriteLine(String value = null)
        {
            sw.WriteLine(value);
        }

        public void Write(String value)
        {
            sw.Write(value);
        }

        public override void Flush()
        {
            this.sw.Flush();
            base.Flush();
        }

        public override string ToString()
        {
            this.Flush();
            this.Seek(0, SeekOrigin.Begin);
            return this.sr.ReadToEnd();
        }
    }
}
