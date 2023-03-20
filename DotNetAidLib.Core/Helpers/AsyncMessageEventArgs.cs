using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncMessageEventHandler(Object sender, AsyncMessageEventArgs args);
    public class AsyncMessageEventArgs : EventArgs
    {
        private String message;
        public AsyncMessageEventArgs(String message)
        {
            Assert.NotNull(message, nameof(message));
            this.message = message;
        }

        public String Message
        {
            get
            {
                return message;
            }
        }
    }
}
