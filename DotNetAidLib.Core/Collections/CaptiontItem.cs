using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace DotNetAidLib.Core.Collections
{
	public class CaptiontItem<T>
	{
		public CaptiontItem()
			:this(default(T), null){}
		
		public CaptiontItem(T value)
			:this(value, null){ }

		public CaptiontItem(T value, String caption) {
			this.Value = value;
			this.Caption = caption;
		}

		public String Caption { get; set; }
		public T Value { get; set; }

		public static implicit operator T(CaptiontItem<T> commentLine) {
			return commentLine.Value;
		}

		public static implicit operator CaptiontItem<T>(T value)
		{
			return new CaptiontItem<T>(value);
		}

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return typeof(CaptiontItem<T>).IsAssignableFrom(obj.GetType())
                && ((CaptiontItem<T>)obj).Value.Equals(this.Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return (String.IsNullOrEmpty(this.Caption)?"": this.Caption + ": ")
                + ((Object)this.Value is null?"":this.Value.ToString());
        }
    }
}