namespace DotNetAidLib.Core.Helpers
{
    public delegate void AsyncValueMessageEventHandler<T>(object sender, AsyncValueMessageEventArgs<T> args);

    public class AsyncValueMessageEventArgs<T> : AsyncMessageEventArgs
    {
        public AsyncValueMessageEventArgs(string message)
            : base(message)
        {
        }

        public AsyncValueMessageEventArgs(T value, string message)
            : base(message)
        {
            Value = value;
        }

        public T Value { get; }
    }
}