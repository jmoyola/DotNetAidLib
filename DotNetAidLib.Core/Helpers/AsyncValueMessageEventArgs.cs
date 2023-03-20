using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncValueMessageEventHandler<T>(Object sender, AsyncValueMessageEventArgs<T> args);
    public class AsyncValueMessageEventArgs<T> : AsyncMessageEventArgs
    {
        private T value=default(T);
        public AsyncValueMessageEventArgs(String message)
            : base(message){}

        public AsyncValueMessageEventArgs(T value, String message)
            :base(message)
        {
            this.value = value;
        }

        public T Value
        {
            get
            {
                return value;
            }
        }
    }
}
