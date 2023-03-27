namespace DotNetAidLib.Core.Drawing
{
    public struct Point
    {
        private double _x;
        private double _y;

        public Point(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        public double X
        {
            get => _x;
            set => _x = value;
        }

        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public Point Empty => new Point(0, 0);

        public Point Add(Point v)
        {
            return new Point(X + v.X, Y + v.Y);
        }

        public Point Substract(Point v)
        {
            return new Point(X - v.X, Y - v.Y);
        }

        public static Point operator +(Point v, Point b)
        {
            return v.Add(b);
        }

        public static Point operator -(Point v, Point b)
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
            return obj != null
                   && obj.GetType().Equals(typeof(Point))
                   && ((Point)obj)._x.Equals(_x)
                   && ((Point)obj)._y.Equals(_y);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() & _y.GetHashCode();
        }

        public override string ToString()
        {
            return "(" + _x + ", " + _y + ")";
        }
    }
}