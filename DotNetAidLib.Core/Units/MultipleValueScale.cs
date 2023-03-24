using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Units
{
	public class MultipleValueScale
	{
        private static String _Pattern = @"(([a-zA-Z/]+)\s*)?(\d+[\.,]?\d*)(\s*([a-zA-Z/]+))?";
		private decimal _DecimalValue = 0;
		private string _Multiple = "";
        private IDictionary<String, Decimal> multiples = new Dictionary<string, decimal>();

        public MultipleValueScale(String multipleList, decimal pow, int startPultiple, Decimal decimalValue, String multiple)
            : this(multipleList, pow, startPultiple)
        {
            this.DecimalValue = decimalValue;
            this.Multiple = multiple;
        }

        public MultipleValueScale(String multipleList, decimal pow, int startPultiple, String value)
            :this(multipleList, pow,startPultiple){
            this.Value = value;
        }

        public MultipleValueScale(String multipleList, decimal pow, int startPultiple)
        {
            String[] aMultipleList = multipleList.Split(',', ';');
            for (int i = startPultiple; i < aMultipleList.Length; i++)
            {
                String[] multiple = aMultipleList[i].Trim().Split('|');
                foreach (String m in multiple)
                    this.multiples.Add(new KeyValuePair<String, Decimal>(m.Trim(), i * pow));
            }
        }

		public MultipleValueScale (){
			this.DecimalValue = 0;
			this.Multiple = "";
		}

        public MultipleValueScale(Decimal decimalValue, String multiple){
            this.DecimalValue = decimalValue;
            this.Multiple = multiple;
        }

		public MultipleValueScale (String value)
		{
			this.Value = value;
		}

        public virtual IDictionary<String, Decimal> Multiples {
            get { return multiples; }
        }

		public decimal DecimalValue { 
			get { return _DecimalValue;}
			set { _DecimalValue = value;}
		}

		public String Multiple {
			get { return _Multiple; }
			set {
				if (!this.Multiples.ContainsKeyIgnoreCase(value))
					throw new Exception ("Multiple '" + value + "' is not valid. Valid multiples are only: " + this.Multiples.Keys.ToStringJoin(", ") + ".");
				_Multiple = value;
			}
		}

		public String Value{
			get{
				return this.DecimalValue + (String.IsNullOrEmpty(this.Multiple) ? "" : " " + this.Multiple);
			}
			set	{
				Regex r = new Regex(_Pattern);
				Match m = r.Match(value);
				if (!m.Success)
					throw new Exception("No valid value.");
				this.DecimalValue = Decimal.Parse(m.Groups[3].Value);

				if (!String.IsNullOrEmpty(m.Groups[2].Value))
					this.Multiple = m.Groups[2].Value;
				else if (!String.IsNullOrEmpty(m.Groups[5].Value))
					this.Multiple = m.Groups[5].Value;
				else
					throw new Exception("Missing multiple.");
			}
		}
		public decimal TotalValue {
            get { return this.DecimalValue * Decimal.Parse(this.Multiples.GetValueIgnoreCase(_Multiple).ToString());}
		}

		public override string ToString ()
		{
			return this.Value;
		}

		public override bool Equals (object obj)
		{
			if (obj != null && this.GetType ().IsAssignableFrom (obj.GetType ())){
				return this.TotalValue.Equals (((MultipleValueScale)obj).TotalValue);
			}
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return this.TotalValue.GetHashCode();
		}

		public static implicit operator Decimal (MultipleValueScale v)
		{
			return v.TotalValue;
		}
	}
}
