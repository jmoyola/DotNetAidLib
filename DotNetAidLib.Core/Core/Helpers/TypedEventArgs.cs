using System;

namespace DotNetAidLib.Core.Helpers
{
    public delegate void TypedEventHandler<T>(object sender, TypedEventArgs<T> args);

    public class TypedEventArgs<T> : EventArgs
    {
        public TypedEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}