using System;
using System.Collections;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Develop
{
    public interface IAssertCondition
    {
        bool IsValid(object value);
    }

    public class IsWhen : AssertCondition
    {
        public IsWhen(Func<object, bool> whenCondition)
            : base(whenCondition)
        {
        }
    }

    public class IsAssignableFrom<T> : AssertCondition
    {
        public IsAssignableFrom()
            : base(v => v != null && v.GetType().IsAssignableFrom(typeof(T)))
        {
        }
    }

    public class IsType<T> : AssertCondition
    {
        public IsType()
            : base(v => v != null && v.GetType().Equals(typeof(T)))
        {
        }
    }

    public class IsGreatestOf<T> : AssertCondition
        where T : IComparable<T>
    {
        public IsGreatestOf(T maxValue)
            : base(
                v => v != null
                     && v.GetType().IsAssignableFrom(typeof(IComparable<T>))
                     && ((IComparable<T>) v).CompareTo(maxValue) > 0)
        {
        }
    }

    public class IsGreatestOrEqualsOf<T> : AssertCondition
        where T : IComparable<T>
    {
        public IsGreatestOrEqualsOf(T maxValue)
            : base(
                v => v != null
                     && v.GetType().IsAssignableFrom(typeof(IComparable<T>))
                     && ((IComparable<T>) v).CompareTo(maxValue) >= 0)
        {
        }
    }

    public class IsLowerOf<T> : AssertCondition
        where T : IComparable<T>
    {
        public IsLowerOf(T maxValue)
            : base(
                v => v != null
                     && v.GetType().IsAssignableFrom(typeof(IComparable<T>))
                     && ((IComparable<T>) v).CompareTo(maxValue) < 0)
        {
        }
    }

    public class IsLowerOfOrEqualsOf<T> : AssertCondition
        where T : IComparable<T>
    {
        public IsLowerOfOrEqualsOf(T maxValue)
            : base(
                v => v != null
                     && v.GetType().IsAssignableFrom(typeof(IComparable<T>))
                     && ((IComparable<T>) v).CompareTo(maxValue) <= 0)
        {
        }
    }

    public class IsBetweenOf<T> : AssertCondition
        where T : IComparable<T>
    {
        public IsBetweenOf(T minValue, T maxValue)
            : base(
                v => v != null
                     && v.GetType().IsAssignableFrom(typeof(IComparable<T>))
                     && ((IComparable<T>) v).CompareTo(minValue) > 0
                     && ((IComparable<T>) v).CompareTo(maxValue) < 0)
        {
        }
    }

    public class IsBetweenOrEqualsOf<T> : AssertCondition
        where T : IComparable<T>
    {
        public IsBetweenOrEqualsOf(T minValue, T maxValue)
            : base(
                v => v != null
                     && v.GetType().IsAssignableFrom(typeof(IComparable<T>))
                     && ((IComparable<T>) v).CompareTo(minValue) >= 0
                     && ((IComparable<T>) v).CompareTo(maxValue) <= 0)
        {
        }
    }

    public class IsRegexMatch : AssertCondition
    {
        public IsRegexMatch(string pattern, RegexOptions regexOptions = RegexOptions.None)
            : base(
                v => v != null && v.ToString().RegexIsMatch(pattern, regexOptions))
        {
        }
    }

    public class AssertCondition : IAssertCondition
    {
        private readonly Func<object, bool> assertFunction;

        public AssertCondition(Func<object, bool> assertFunction)
        {
            Assert.NotNull(assertFunction, nameof(assertFunction));
            this.assertFunction = assertFunction;
        }

        private static AssertCondition _IsNotNull => new AssertCondition(v => v != null);
        private static AssertCondition _IsNumber => new AssertCondition(v => v != null && v.GetType().IsNumber());
        private static AssertCondition _IsInteger => new AssertCondition(v => v != null && v.GetType().IsInteger());
        private static AssertCondition _IsUInteger => new AssertCondition(v => v != null && v.GetType().IsUInteger());
        private static AssertCondition _IsDecimal => new AssertCondition(v => v != null && v.GetType().IsDecimal());
        private static AssertCondition _IsPrimitive => new AssertCondition(v => v != null && v.GetType().IsPrimitive);
        private static AssertCondition _IsClass => new AssertCondition(v => v != null && v.GetType().IsClass);

        private static AssertCondition _IsString =>
            new AssertCondition(v => v == null || v.GetType().Equals(typeof(string)));

        private static AssertCondition _IsEnum => new AssertCondition(v => v != null && v.GetType().IsEnum);

        private static AssertCondition _IsNotEmpty => new AssertCondition(v => v != null
                                                                               && v.GetType()
                                                                                   .IsAssignableFrom(typeof(IList))
                                                                               && ((IList) v).Count > 0);

        private static AssertCondition _IsBool =>
            new AssertCondition(v => v != null && v.GetType().Equals(typeof(bool)));

        private static AssertCondition _IsDateTime =>
            new AssertCondition(v => v != null && v.GetType().Equals(typeof(DateTime)));

        public static AssertCondition IsNotNull => _IsNotNull;
        public static AssertCondition IsNumber => _IsNumber;
        public static AssertCondition IsInteger => _IsInteger;
        public static AssertCondition IsUInteger => _IsUInteger;
        public static AssertCondition IsDecimal => _IsDecimal;
        public static AssertCondition IsBool => _IsBool;
        public static AssertCondition IsDateTime => _IsDateTime;
        public static AssertCondition IsPrimitive => _IsPrimitive;
        public static AssertCondition IsClass => _IsClass;
        public static AssertCondition IsString => _IsString;
        public static AssertCondition IsEnum => _IsEnum;
        public static AssertCondition IsNotEmpty => _IsNotEmpty;

        public bool IsValid(object value)
        {
            return assertFunction(value);
        }
    }
}