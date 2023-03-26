using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
    public class MemoryTransferUnitScale : MultipleValueScale
    {
        private static Dictionary<string, decimal> multiples;

        public MemoryTransferUnitScale(string value)
            : base(value)
        {
        }

        public MemoryTransferUnitScale(decimal decimalValue, string multiple)
            : base(decimalValue, multiple)
        {
        }

        public override IDictionary<string, decimal> Multiples
        {
            get
            {
                if (multiples == null)
                {
                    multiples = new Dictionary<string, decimal>();
                    multiples.Add("b/s", 1);
                    multiples.Add("byte/s", 1);
                    multiples.Add("kb/s", new decimal(Math.Pow(2, 10)));
                    multiples.Add("kilobyte/s", new decimal(Math.Pow(2, 10)));
                    multiples.Add("mb/s", new decimal(Math.Pow(2, 20)));
                    multiples.Add("megabyte/s", new decimal(Math.Pow(2, 20)));
                    multiples.Add("gb/s", new decimal(Math.Pow(2, 30)));
                    multiples.Add("gigabyte/s", new decimal(Math.Pow(2, 30)));
                    multiples.Add("tb/s", new decimal(Math.Pow(2, 40)));
                    multiples.Add("terabyte/s", new decimal(Math.Pow(2, 40)));
                    multiples.Add("pb/s", new decimal(Math.Pow(2, 50)));
                    multiples.Add("petabyte/s", new decimal(Math.Pow(2, 50)));
                    multiples.Add("hb/s", new decimal(Math.Pow(2, 60)));
                    multiples.Add("hexabyte/s", new decimal(Math.Pow(2, 60)));
                    multiples.Add("zb/s", new decimal(Math.Pow(2, 70)));
                    multiples.Add("zettabyte/s", new decimal(Math.Pow(2, 70)));
                    multiples.Add("yb/s", new decimal(Math.Pow(2, 80)));
                    multiples.Add("yottabyte/s", new decimal(Math.Pow(2, 80)));
                }

                return multiples;
            }
        }
    }
}