using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public class TryParse<T>
    {
        private readonly Func<object, T> castFunction;
        private T value;

        private TryParse(object value, Func<object, T> castFunction)
            : this(castFunction)
        {
            Parse(value);
        }

        public TryParse(Func<object, T> castFunction)
        {
            Assert.NotNull(castFunction, nameof(castFunction));
            this.castFunction = castFunction;
        }

        public T Value
        {
            get => value;

            set
            {
                this.value = value;
                IsValid = true;
            }
        }

        public bool IsValid { get; private set; }

        public void Parse(object value)
        {
            try
            {
                this.value = castFunction.Invoke(value);
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }

        public static TryParse<T> Parse(object value, Func<object, T> castFunction)
        {
            return new TryParse<T>(value, castFunction);
        }
    }
}