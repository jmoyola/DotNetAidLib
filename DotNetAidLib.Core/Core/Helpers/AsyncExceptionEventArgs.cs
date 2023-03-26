using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncExceptionEventHandler(object sender, AsyncExceptionEventArgs args);

    public class AsyncExceptionEventArgs : AsyncMessageEventArgs
    {
        public AsyncExceptionEventArgs(Exception exception)
            : base(exception.Message)
        {
            Assert.NotNull(exception, nameof(exception));
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}