using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
    public class MemoryTransferUnitScale:MultipleValueScale
	{
        private static Dictionary<String, decimal> multiples = null;

        public MemoryTransferUnitScale(String value)
            : base(value) { }

        public MemoryTransferUnitScale(Decimal decimalValue, String multiple)
            : base(decimalValue, multiple) { }

        public override IDictionary<String, decimal> Multiples
        {
            get{
                if (multiples == null){
                    multiples = new Dictionary<String, decimal>();
                    multiples.Add("b/s", 1);
                    multiples.Add("byte/s", 1);
                    multiples.Add("kb/s", new Decimal(Math.Pow(2, 10)));
                    multiples.Add("kilobyte/s", new Decimal(Math.Pow(2, 10)));
                    multiples.Add("mb/s", new Decimal(Math.Pow(2, 20)));
                    multiples.Add("megabyte/s", new Decimal(Math.Pow(2, 20)));
                    multiples.Add("gb/s", new Decimal(Math.Pow(2, 30)));
                    multiples.Add("gigabyte/s", new Decimal(Math.Pow(2, 30)));
                    multiples.Add("tb/s", new Decimal(Math.Pow(2, 40)));
                    multiples.Add("terabyte/s", new Decimal(Math.Pow(2, 40)));
                    multiples.Add("pb/s", new Decimal(Math.Pow(2, 50)));
                    multiples.Add("petabyte/s", new Decimal(Math.Pow(2, 50)));
                    multiples.Add("hb/s", new Decimal(Math.Pow(2, 60)));
                    multiples.Add("hexabyte/s", new Decimal(Math.Pow(2, 60)));
                    multiples.Add("zb/s", new Decimal(Math.Pow(2, 70)));
                    multiples.Add("zettabyte/s", new Decimal(Math.Pow(2, 70)));
                    multiples.Add("yb/s", new Decimal(Math.Pow(2, 80)));
                    multiples.Add("yottabyte/s", new Decimal(Math.Pow(2, 80)));
                }

                return multiples;
            }
        }
	}
}
