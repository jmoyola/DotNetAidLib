using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class ReadOnlyFIFO<T>: IEnumerable<T>
    {
        protected Object oLock = new object ();

        protected Queue<T> fifo = new Queue<T> ();
        public ReadOnlyFIFO (){
            fifo = new Queue<T> ();
        }

        public ReadOnlyFIFO (IEnumerable<T> values){
            fifo = new Queue<T> (values);
        }

        public T Pool ()
        {
            lock (oLock) {
                return fifo.Dequeue();
            }
        }

        public T Peek ()
        {
            lock (oLock) {
                return fifo.Peek ();
            }
        }

        public bool Contains (T frame)
        {
            lock (oLock) {
                return fifo.Contains (frame);
            }
        }

        public int Count
        {
            get {
                lock (oLock) {
                    return fifo.Count;
                }
            }
        }

        public void Clear ()
        {
            lock (oLock) {
                fifo.Clear ();
            }
        }

        public IEnumerator<T> GetEnumerator (){
            lock (oLock) {
                return this.fifo.ToList ().GetEnumerator ();
            }
        }

        IEnumerator IEnumerable.GetEnumerator (){
            lock (oLock) {
                return this.fifo.ToArray ().GetEnumerator ();
            }
        }
    }
}
