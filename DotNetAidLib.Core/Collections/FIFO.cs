using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Collections
{
    public class FIFO<T>:ReadOnlyFIFO<T>
    {
        public FIFO ()
            :base(){}
        public FIFO (IEnumerable<T> values)
        :base(values) { }

        public void Add (T frame) {
            lock (oLock) {
                fifo.Enqueue (frame);
            }
        }

        public ReadOnlyFIFO<T> AsReadOnly () {
            return this;
        }
    }
}
