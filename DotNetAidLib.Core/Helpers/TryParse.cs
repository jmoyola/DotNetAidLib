using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public class TryParse<T>
    {
        private Func<Object, T> castFunction;
        private T value;
        private bool isValid;

        private TryParse(Object value, Func<Object, T> castFunction)
        :this(castFunction)
        {
            this.Parse(value);
        }

        public TryParse(Func<Object, T> castFunction){
            Assert.NotNull(castFunction, nameof(castFunction));
            this.castFunction = castFunction;
        }

        public void Parse(Object value)
        {
            try
            {
                this.value = this.castFunction.Invoke(value);
                this.isValid = true;
            }
            catch {
                this.isValid = false;
            }
        }

        public T Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
                this.isValid = true;
            }
        }

        public bool IsValid
        {
            get
            {
                return isValid;
            }
        }

        public static TryParse<T> Parse(Object value, Func<Object, T> castFunction){
            return new TryParse<T>(value, castFunction);
        }
    }
}
