using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
    public class MemoryUnitScale : MultipleValueScale
    {
        private static Dictionary<string, decimal> multiples;

        public MemoryUnitScale(string value)
            : base(value)
        {
        }

        public MemoryUnitScale(decimal decimalValue, string multiple)
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
                    multiples.Add("", 1);
                    multiples.Add("b", 1);
                    multiples.Add("byte", 1);
                    multiples.Add("kb", new decimal(Math.Pow(2, 10)));
                    multiples.Add("kilobyte", new decimal(Math.Pow(2, 10)));
                    multiples.Add("mb", new decimal(Math.Pow(2, 20)));
                    multiples.Add("megabyte", new decimal(Math.Pow(2, 20)));
                    multiples.Add("gb", new decimal(Math.Pow(2, 30)));
                    multiples.Add("gigabyte", new decimal(Math.Pow(2, 30)));
                    multiples.Add("tb", new decimal(Math.Pow(2, 40)));
                    multiples.Add("terabyte", new decimal(Math.Pow(2, 40)));
                    multiples.Add("pb", new decimal(Math.Pow(2, 50)));
                    multiples.Add("petabyte", new decimal(Math.Pow(2, 50)));
                    multiples.Add("hb", new decimal(Math.Pow(2, 60)));
                    multiples.Add("hexabyte", new decimal(Math.Pow(2, 60)));
                    multiples.Add("zb", new decimal(Math.Pow(2, 70)));
                    multiples.Add("zettabyte", new decimal(Math.Pow(2, 70)));
                    multiples.Add("yb", new decimal(Math.Pow(2, 80)));
                    multiples.Add("yottabyte", new decimal(Math.Pow(2, 80)));
                }

                return multiples;
            }
        }
    }
}