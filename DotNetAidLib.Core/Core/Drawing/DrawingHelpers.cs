using System;

namespace DotNetAidLib.Core.Drawing
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public abstract class IPoint<T> where T : IComparable
    {
        private T x;
        private T y;

        public IPoint()
        {
            x = default;
            y = default;
        }

        public IPoint(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public T X
        {
            get => x;
            set => x = value;
        }

        public T Y
        {
            get => y;
            set => y = value;
        }

        public abstract IPoint<T> Empty { get; }
        public abstract IPoint<T> Add(IPoint<T> v);
        public abstract IPoint<T> Substract(IPoint<T> v);

        public override bool Equals(object obj)
        {
            return obj != null
                   && obj.GetType().Equals(GetType())
                   && ((IPoint<T>) obj).x.Equals(x)
                   && ((IPoint<T>) obj).y.Equals(y);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }

    public class Point : IPoint<int>
    {
        public Point()
        {
        }

        public Point(int x, int y)
            : base(x, y)
        {
        }

        public override IPoint<int> Empty => new Point(0, 0);

        public override IPoint<int> Add(IPoint<int> v)
        {
            return new Point(X + v.X, Y + v.Y);
        }

        public override IPoint<int> Substract(IPoint<int> v)
        {
            return new Point(X - v.X, Y - v.Y);
        }

        public static IPoint<int> operator +(Point v, Point b)
        {
            return v.Add(b);
        }

        public static IPoint<int> operator -(Point v, Point b)
        {
            return v.Substract(b);
        }

        public static bool operator ==(Point v, Point b)
        {
            return v.Equals(b);
        }

        public static bool operator !=(Point v, Point b)
        {
            return !v.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class PointF
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class DrawingHelpers
    {
    }
}