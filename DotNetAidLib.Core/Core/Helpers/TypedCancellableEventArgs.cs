namespace DotNetAidLib.Core.Helpers
{
    public delegate void TypedCancellableEventHandler<T>(object sender, TypedCancellableEventArgs<T> args);

    public class TypedCancellableEventArgs<T> : TypedEventArgs<T>
    {
        public TypedCancellableEventArgs(T value)
            : base(value)
        {
        }

        public bool Cancel { get; set; } = false;
    }
}