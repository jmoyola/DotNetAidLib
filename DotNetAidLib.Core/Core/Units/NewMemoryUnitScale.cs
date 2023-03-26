using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
    public class NewMemoryUnitScale : MultipleValueScale
    {
        private static Dictionary<string, decimal> multiples;

        public NewMemoryUnitScale(string value)
            : base(value)
        {
        }

        public NewMemoryUnitScale(decimal decimalValue, string multiple)
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

                    multiples.Add("kib", new decimal(Math.Pow(2, 10)));
                    multiples.Add("kibibyte", new decimal(Math.Pow(2, 10)));
                    multiples.Add("mib", new decimal(Math.Pow(2, 20)));
                    multiples.Add("mebibyte", new decimal(Math.Pow(2, 20)));
                    multiples.Add("gib", new decimal(Math.Pow(2, 30)));
                    multiples.Add("gibibyte", new decimal(Math.Pow(2, 30)));
                    multiples.Add("tib", new decimal(Math.Pow(2, 40)));
                    multiples.Add("tebibyte", new decimal(Math.Pow(2, 40)));
                    multiples.Add("pib", new decimal(Math.Pow(2, 50)));
                    multiples.Add("pebibyte", new decimal(Math.Pow(2, 50)));
                    multiples.Add("hib", new decimal(Math.Pow(2, 60)));
                    multiples.Add("hexbibyte", new decimal(Math.Pow(2, 60)));
                    multiples.Add("zib", new decimal(Math.Pow(2, 70)));
                    multiples.Add("zebibyte", new decimal(Math.Pow(2, 70)));
                    multiples.Add("yib", new decimal(Math.Pow(2, 80)));
                    multiples.Add("yobibyte", new decimal(Math.Pow(2, 80)));

                    multiples.Add("kb", new decimal(Math.Pow(10, 3)));
                    multiples.Add("kilobyte", new decimal(Math.Pow(10, 3)));
                    multiples.Add("mb", new decimal(Math.Pow(10, 6)));
                    multiples.Add("megabyte", new decimal(Math.Pow(10, 6)));
                    multiples.Add("gb", new decimal(Math.Pow(10, 9)));
                    multiples.Add("gigabyte", new decimal(Math.Pow(10, 9)));
                    multiples.Add("tb", new decimal(Math.Pow(10, 12)));
                    multiples.Add("terabyte", new decimal(Math.Pow(10, 12)));
                    multiples.Add("pb", new decimal(Math.Pow(10, 15)));
                    multiples.Add("petabyte", new decimal(Math.Pow(10, 15)));
                    multiples.Add("hb", new decimal(Math.Pow(10, 18)));
                    multiples.Add("hexabyte", new decimal(Math.Pow(10, 18)));
                    multiples.Add("zb", new decimal(Math.Pow(10, 21)));
                    multiples.Add("zettabyte", new decimal(Math.Pow(10, 21)));
                    multiples.Add("yb", new decimal(Math.Pow(10, 24)));
                    multiples.Add("yottabyte", new decimal(Math.Pow(10, 24)));
                }

                return multiples;
            }
        }
    }
}