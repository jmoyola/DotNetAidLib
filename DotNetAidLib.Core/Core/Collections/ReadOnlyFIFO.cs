using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class ReadOnlyFIFO<T> : IEnumerable<T>
    {
        protected Queue<T> fifo = new Queue<T>();
        protected object oLock = new object();

        public ReadOnlyFIFO()
        {
            fifo = new Queue<T>();
        }

        public ReadOnlyFIFO(IEnumerable<T> values)
        {
            fifo = new Queue<T>(values);
        }

        public int Count
        {
            get
            {
                lock (oLock)
                {
                    return fifo.Count;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (oLock)
            {
                return fifo.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (oLock)
            {
                return fifo.ToArray().GetEnumerator();
            }
        }

        public T Pool()
        {
            lock (oLock)
            {
                return fifo.Dequeue();
            }
        }

        public T Peek()
        {
            lock (oLock)
            {
                return fifo.Peek();
            }
        }

        public bool Contains(T frame)
        {
            lock (oLock)
            {
                return fifo.Contains(frame);
            }
        }

        public void Clear()
        {
            lock (oLock)
            {
                fifo.Clear();
            }
        }
    }
}