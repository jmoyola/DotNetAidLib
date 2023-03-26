using System;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncValueExceptionEventHandler<T>(object sender, AsyncValueExceptionEventArgs<T> args);

    public class AsyncValueExceptionEventArgs<T> : AsyncExceptionEventArgs
    {
        public AsyncValueExceptionEventArgs(Exception exception)
            : base(exception)
        {
        }

        public AsyncValueExceptionEventArgs(T value, Exception exception)
            : base(exception)
        {
            Value = value;
        }

        public T Value { get; }
    }
}