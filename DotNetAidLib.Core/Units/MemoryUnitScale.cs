using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
	public class MemoryUnitScale:MultipleValueScale
	{
        private static Dictionary<String, decimal> multiples = null;

        public MemoryUnitScale(String value)
            : base(value) { }

        public MemoryUnitScale(Decimal decimalValue, String multiple)
            : base(decimalValue, multiple) { }

        public override IDictionary<String, decimal> Multiples
        {
            get{
                if (multiples == null){
                    multiples = new Dictionary<String, decimal>();
                    multiples.Add("", 1);
                    multiples.Add("b", 1);
                    multiples.Add("byte", 1);
                    multiples.Add("kb", new Decimal(Math.Pow(2, 10)));
                    multiples.Add("kilobyte", new Decimal(Math.Pow(2, 10)));
                    multiples.Add("mb", new Decimal(Math.Pow(2, 20)));
                    multiples.Add("megabyte", new Decimal(Math.Pow(2, 20)));
                    multiples.Add("gb", new Decimal(Math.Pow(2, 30)));
                    multiples.Add("gigabyte", new Decimal(Math.Pow(2, 30)));
                    multiples.Add("tb", new Decimal(Math.Pow(2, 40)));
                    multiples.Add("terabyte", new Decimal(Math.Pow(2, 40)));
                    multiples.Add("pb", new Decimal(Math.Pow(2, 50)));
                    multiples.Add("petabyte", new Decimal(Math.Pow(2, 50)));
                    multiples.Add("hb", new Decimal(Math.Pow(2, 60)));
                    multiples.Add("hexabyte", new Decimal(Math.Pow(2, 60)));
                    multiples.Add("zb", new Decimal(Math.Pow(2, 70)));
                    multiples.Add("zettabyte", new Decimal(Math.Pow(2, 70)));
                    multiples.Add("yb", new Decimal(Math.Pow(2, 80)));
                    multiples.Add("yottabyte", new Decimal(Math.Pow(2, 80)));
                }

                return multiples;
            }
        }
	}
}
