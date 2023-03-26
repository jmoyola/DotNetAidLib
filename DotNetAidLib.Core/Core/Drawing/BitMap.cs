using System;
using System.Numerics;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Drawing
{
    public class BitMap<T> : IDisposable where T : struct
    {
        private bool _disposed;
        private int _height;

        private int _width;

        public BitMap(int width, int height)
        {
            Assert.GreaterThan(width, 0, nameof(width));
            Assert.GreaterThan(height, 0, nameof(height));

            _width = width;
            _height = height;

            Points = new Vector<T>[width, height];
        }

        public Vector<T>[,] Points { get; }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposed)
                    return;


                _disposed = true;
            }
        }
    }
}