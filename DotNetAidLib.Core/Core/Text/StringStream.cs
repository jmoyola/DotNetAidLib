using System.IO;
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Text
{
    public class StringStream : MemoryStream
    {
        private readonly StreamReader sr;
        private readonly StreamWriter sw;

        public StringStream()
            : this(Encoding.UTF8)
        {
        }

        public StringStream(Encoding encoding)
        {
            Assert.NotNull(encoding, nameof(encoding));
            sr = new StreamReader(this, encoding);
            sw = new StreamWriter(this, encoding);
        }

        public StringStream(string value)
            : this(value, Encoding.UTF8)
        {
        }

        public StringStream(string value, Encoding encoding)
            : this(encoding)
        {
            Write(value);
            Flush();
        }

        public string ReadLine()
        {
            return sr.ReadLine();
        }

        public int Read()
        {
            return sr.Read();
        }

        public void Reset()
        {
            Seek(0, SeekOrigin.Begin);
        }

        public void WriteLine(string value = null)
        {
            sw.WriteLine(value);
        }

        public void Write(string value)
        {
            sw.Write(value);
        }

        public override void Flush()
        {
            sw.Flush();
            base.Flush();
        }

        public override string ToString()
        {
            Flush();
            Seek(0, SeekOrigin.Begin);
            return sr.ReadToEnd();
        }
    }
}