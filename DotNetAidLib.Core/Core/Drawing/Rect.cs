namespace DotNetAidLib.Core.Drawing
{
    public struct Rect
    {
        private double _x1;
        private double _y1;

        private double _x2;
        private double _y2;

        public Rect(double x, double y)
        :this(x,y,x,y) { }

        public Rect(double x1, double y1, double x2, double y2)
        {
            this._x1 = x1;
            this._y1 = y1;
            this._x2 = x2;
            this._y2 = y2;
        }

        public double X1
        {
            get => _x1;
            set => _x1 = value;
        }

        public double Y1
        {
            get => _y1;
            set => _y1 = value;
        }

        public double X2
        {
            get => _x2;
            set => _x2 = value;
        }

        public double Y2
        {
            get => _y2;
            set => _y2 = value;
        }

        public Rect Empty => new Rect(0, 0);

        public Rect Add(Rect v)
        {
            return new Rect(X1 + v.X1, Y1 + v.Y1, X2 + v.X2, Y2 + v.Y2);
        }

        public Rect Substract(Rect v)
        {
            return new Rect(X1 - v.X1, Y1 - v.Y1, X2 - v.X2, Y2 - v.Y2);
        }

        public static Rect operator +(Rect v, Rect b)
        {
            return v.Add(b);
        }

        public static Rect operator -(Rect v, Rect b)
        {
            return v.Substract(b);
        }

        public static bool operator ==(Rect v, Rect b)
        {
            return v.Equals(b);
        }

        public static bool operator !=(Rect v, Rect b)
        {
            return !v.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj != null
                   && obj.GetType().Equals(typeof(Rect))
                   && ((Rect)obj)._x1.Equals(_x1)
                   && ((Rect)obj)._y1.Equals(_y1)
                   && ((Rect)obj)._x2.Equals(_x2)
                   && ((Rect)obj)._y2.Equals(_y2);
        }

        public override int GetHashCode()
        {
            return _x1.GetHashCode() & _y1.GetHashCode()
                & _x2.GetHashCode() & _y2.GetHashCode();
        }

        public override string ToString()
        {
            return "(" + _x1 + ", " + _y1 + ") - (" + _x2 + ", " + _y2 + ")";
        }
    }
}