namespace DotNetAidLib.Database.DAO.Navigation
{
    public class DaoObject<T>
    {
        private T _Value;

        private DaoObject()
        {
        }

        public T Value
        {
            get => _Value;
            set => _Value = value;
        }

        public T Retrieve()
        {
            return _Value;
        }

        public override string ToString()
        {
            return _Value.ToString();
        }

        public override int GetHashCode()
        {
            return _Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _Value.Equals(obj);
        }

        public static implicit operator T(DaoObject<T> v)
        {
            return v.Value;
        }
    }
}