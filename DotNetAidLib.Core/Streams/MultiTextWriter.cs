using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public class MultiTextWriter: TextWriter, IDisposable
    {
        private IList<TextWriter> writers;

        public MultiTextWriter(IList<TextWriter> writers)
        {
            Assert.NotNullOrEmpty( writers, nameof(writers));
            this.writers = writers;
        }

        public override Encoding Encoding => writers[0].Encoding;
        
        public override void Close()
        {
            writers.ToList().ForEach(v=>v.Close());
        }

        public override void Flush()
        {
            writers.ToList().ForEach(v=>v.Flush());
        }
        
        public override void Write(long value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        
        public override void Write(float value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        public override void Write(Object value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        
        public override void Write(ulong value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        public override void Write(int value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        public override void Write(uint value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }

        public override void Write(double value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        public override void Write(decimal value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }

        public override void Write(bool value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        public override void Write(char value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        public override void Write(char[] value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            writers.ToList().ForEach(v=>v.Write(buffer, index, count));
        }
        
        public override void Write(string value)
        {
            writers.ToList().ForEach(v=>v.Write(value));
        }
        
        public override void Write(string format, params object[] arg)
        {
            writers.ToList().ForEach(v=>v.Write(format, arg));
        }

        public override void Write(string format, object arg0)
        {
            writers.ToList().ForEach(v=>v.Write(format, arg0));
        }

        public override void Write(string format, object arg0, object arg1)
        {
            writers.ToList().ForEach(v=>v.Write(format, arg0, arg1));
        }

        public override void Write(string format, Object arg0, Object arg1, Object arg2)
        {
            writers.ToList().ForEach(v=>v.Write(format, arg0, arg1, arg2));
        }

        public override void WriteLine()
        {
            writers.ToList().ForEach(v=>v.WriteLine());
        }

        public override void WriteLine(bool value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(char value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(char[] buffer)
        {
            writers.ToList().ForEach(v=>v.WriteLine(buffer));
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            writers.ToList().ForEach(v=>v.WriteLine(buffer, index, count));
        }

        public override void WriteLine(decimal value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(double value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(int value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(long value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(object value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(float value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(string value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(string format, object arg0)
        {
            writers.ToList().ForEach(v=>v.WriteLine(format, arg0));
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            writers.ToList().ForEach(v=>v.WriteLine(format, arg0, arg1));
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            writers.ToList().ForEach(v=>v.WriteLine(format, arg0, arg1, arg2));
        }

        public override void WriteLine(string format, params object[] arg)
        {
            writers.ToList().ForEach(v=>v.WriteLine(format, arg));
        }

        public override void WriteLine(uint value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override void WriteLine(ulong value)
        {
            writers.ToList().ForEach(v=>v.WriteLine(value));
        }

        public override Task FlushAsync()
        {
            return Task.WhenAll(writers.ToList().Select(v => v.FlushAsync()));
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteLineAsync(buffer, index, count)));
        }

        public override Task WriteLineAsync(string value)
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteLineAsync(value)));
        }
        
        public override Task WriteAsync(char value)
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteAsync(value)));
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteAsync(buffer, index, count)));
        }

        public override Task WriteAsync(string value)
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteAsync(value)));
        }
        
        public override Task WriteLineAsync()
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteLineAsync()));
        }

        public override Task WriteLineAsync(char value)
        {
            return Task.WhenAll(writers.ToList().Select(v => v.WriteLineAsync(value)));
        }
        
        public override string NewLine {
            get=>writers[0].NewLine;
            set=>writers.ToList().ForEach(v=>v.NewLine=value);
        }

        public override IFormatProvider FormatProvider
        {
            get => writers[0].FormatProvider;
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                writers.ToList().ForEach(v => v.Dispose());
                this.disposed = true;
            }
        }
        
        public new void Dispose()
        {
            this.Dispose(true);
        }
    }
}
