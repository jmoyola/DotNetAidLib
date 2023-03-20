using System;
namespace DotNetAidLib.Core.Helpers
{
    public delegate void TypedEventHandler<T>(Object sender, TypedEventArgs<T> args);
    public class TypedEventArgs<T> : EventArgs
    {
        private T value;

        public TypedEventArgs(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get { return this.value; }
        }
    }
}
