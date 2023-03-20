using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncValueExceptionEventHandler<T>(Object sender, AsyncValueExceptionEventArgs<T> args);
    public class AsyncValueExceptionEventArgs<T>: AsyncExceptionEventArgs
    {
        private T value=default(T);
        public AsyncValueExceptionEventArgs(Exception exception)
            : base(exception){}

        public AsyncValueExceptionEventArgs(T value, Exception exception)
            :base(exception)
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
