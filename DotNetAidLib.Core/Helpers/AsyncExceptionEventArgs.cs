using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncExceptionEventHandler(Object sender, AsyncExceptionEventArgs args);
    public class AsyncExceptionEventArgs:AsyncMessageEventArgs
    {
        private Exception exception;
        public AsyncExceptionEventArgs(Exception exception)
            :base(exception.Message)
        {
            Assert.NotNull(exception, nameof(exception));
            this.exception = exception;
        }

        public Exception Exception
        {
            get
            {
                return exception;
            }
        }
    }
}
