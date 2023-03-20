using System;
namespace DotNetAidLib.Core.Helpers
{
    public delegate void TypedCancellableEventHandler<T>(Object sender, TypedCancellableEventArgs<T> args);
    public class TypedCancellableEventArgs<T> : TypedEventArgs<T>
    {
        private bool cancel = false;

        public TypedCancellableEventArgs(T value)
            : base(value) { }

        public bool Cancel
        {
            get
            {
                return cancel;
            }

            set
            {
                cancel = value;
            }
        }
    }
}
