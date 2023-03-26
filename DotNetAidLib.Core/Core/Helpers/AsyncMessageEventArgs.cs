using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncMessageEventHandler(object sender, AsyncMessageEventArgs args);

    public class AsyncMessageEventArgs : EventArgs
    {
        public AsyncMessageEventArgs(string message)
        {
            Assert.NotNull(message, nameof(message));
            Message = message;
        }

        public string Message { get; }
    }
}