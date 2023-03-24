using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
	public class NewMemoryUnitScale:MultipleValueScale
	{
        private static Dictionary<String, decimal> multiples = null;

		public NewMemoryUnitScale (String value)
			: base (value) {}

        public NewMemoryUnitScale(Decimal decimalValue, String multiple)
            : base(decimalValue, multiple) { }

        public override IDictionary<String, decimal> Multiples
        {
            get
            {
                if (multiples == null)
                {
                    multiples = new Dictionary<String, decimal>();
                    multiples.Add("", 1);
                    multiples.Add("b", 1);
                    multiples.Add("byte", 1);

                    multiples.Add("kib", new Decimal(Math.Pow(2, 10)));
                    multiples.Add("kibibyte", new Decimal(Math.Pow(2, 10)));
                    multiples.Add("mib", new Decimal(Math.Pow(2, 20)));
                    multiples.Add("mebibyte", new Decimal(Math.Pow(2, 20)));
                    multiples.Add("gib", new Decimal(Math.Pow(2, 30)));
                    multiples.Add("gibibyte", new Decimal(Math.Pow(2, 30)));
                    multiples.Add("tib", new Decimal(Math.Pow(2, 40)));
                    multiples.Add("tebibyte", new Decimal(Math.Pow(2, 40)));
                    multiples.Add("pib", new Decimal(Math.Pow(2, 50)));
                    multiples.Add("pebibyte", new Decimal(Math.Pow(2, 50)));
                    multiples.Add("hib", new Decimal(Math.Pow(2, 60)));
                    multiples.Add("hexbibyte", new Decimal(Math.Pow(2, 60)));
                    multiples.Add("zib", new Decimal(Math.Pow(2, 70)));
                    multiples.Add("zebibyte", new Decimal(Math.Pow(2, 70)));
                    multiples.Add("yib", new Decimal(Math.Pow(2, 80)));
                    multiples.Add("yobibyte", new Decimal(Math.Pow(2, 80)));

                    multiples.Add("kb", new Decimal(Math.Pow(10, 3)));
                    multiples.Add("kilobyte", new Decimal(Math.Pow(10, 3)));
                    multiples.Add("mb", new Decimal(Math.Pow(10, 6)));
                    multiples.Add("megabyte", new Decimal(Math.Pow(10, 6)));
                    multiples.Add("gb", new Decimal(Math.Pow(10, 9)));
                    multiples.Add("gigabyte", new Decimal(Math.Pow(10, 9)));
                    multiples.Add("tb", new Decimal(Math.Pow(10, 12)));
                    multiples.Add("terabyte", new Decimal(Math.Pow(10, 12)));
                    multiples.Add("pb", new Decimal(Math.Pow(10, 15)));
                    multiples.Add("petabyte", new Decimal(Math.Pow(10, 15)));
                    multiples.Add("hb", new Decimal(Math.Pow(10, 18)));
                    multiples.Add("hexabyte", new Decimal(Math.Pow(10, 18)));
                    multiples.Add("zb", new Decimal(Math.Pow(10, 21)));
                    multiples.Add("zettabyte", new Decimal(Math.Pow(10, 21)));
                    multiples.Add("yb", new Decimal(Math.Pow(10, 24)));
                    multiples.Add("yottabyte", new Decimal(Math.Pow(10, 24)));
                }

                return multiples;
            }
        }
	}
}
