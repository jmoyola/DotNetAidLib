using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace DotNetAidLib.Database
{
	[DefaultProperty("Value")]
	public class DBNullable<T>
		:IComparable<T>
	{
		private Object _Value;

		public DBNullable(Object value){
			this.RawValue=value;
		}

		public DBNullable(T value){
			this._Value=value;
		}

		public DBNullable(DBNull value){
			this._Value=value;
		}

		public bool HasValue{
			get{ return !DBNull.Value.Equals (this._Value);}
		}

		public object RawValue{
			get{
				return this._Value;
			}

			set{
				try{
					if (typeof(DBNull).IsInstanceOfType (value))
						this._Value = value;
					else
						this._Value = Convert.ChangeType (value, typeof(T));
				}
				catch(Exception ex){
					throw new Exception ("Only allowed 'DBNull' or '" + typeof(T).Name + "' types value.", ex);
				}
			}
		}

		public T Value{
			get{
				if(typeof(T).IsInstanceOfType (this._Value))
					return (T)this._Value;
				else
					throw new Exception ("Value is 'DBNull'.");
			}
			set{
				this._Value = value;
			}
		}

		public static implicit operator DBNullable<T>(T v)
		{
			return new DBNullable<T>(v);
		}

		public static implicit operator DBNullable<T>(DBNull v)
		{
			return new DBNullable<T>(v);
		}

		public static implicit operator T(DBNullable<T> v)
		{
			return v.Value;
		}

		public override string ToString ()
		{
			if(this.HasValue)
				return "" + this.Value;
			else
				return String.Empty;
		}
 
		public override bool Equals (object obj)
		{
			if (DBNull.Value.Equals(obj))
				return !this.HasValue;
			if (!(obj is DBNullable<T>))
				return false;

			return this.Equals((DBNullable <T>) obj);
		}

		public bool Equals (DBNullable<T> other)
		{
			if (other.HasValue != this.HasValue)
				return false;

			if (!this.HasValue)
				return true;

			return other.Value.Equals (this.Value);
		}

		public int CompareTo (DBNullable<T> n2)
		{
			if (this.HasValue) {
				if (!n2.HasValue)
					return 1;

				return Comparer<T>.Default.Compare (this.Value, n2.Value);
			}

			return n2.HasValue ? -1 : 0;
		}

		public int CompareTo (T n2)
		{
			if (this.HasValue)
				return Comparer<T>.Default.Compare (this.Value, n2);
			else
				return 1;
		}

		public override int GetHashCode ()
		{
			if (this.HasValue)
				return this.Value.GetHashCode ();
			else
				return 0;
		}

		public static int Compare (DBNullable<T> n1, DBNullable<T> n2)
		{
			if (n1.HasValue) {
				if (!n2.HasValue)
					return 1;

				return Comparer<T>.Default.Compare (n1.Value, n2.Value);
			}

			return n2.HasValue ? -1 : 0;
		}
			
		public static bool Equals(DBNullable<T> n1, DBNullable<T> n2)
		{
			if (n1.HasValue != n2.HasValue)
				return false;

			if (!n1.HasValue)
				return true;

			return EqualityComparer<T>.Default.Equals (n1.Value, n2.Value);
		}
	}
}

