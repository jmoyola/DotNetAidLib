using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
    public class FrequencyUnitScale : MultipleValueScale
    {
        private static Dictionary<string, decimal> multiples;

        public FrequencyUnitScale(string value)
            : base(value)
        {
        }

        public FrequencyUnitScale(decimal decimalValue, string multiple)
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
                    multiples.Add("hz", 1);
                    multiples.Add("khz", new decimal(Math.Pow(10, 3)));
                    multiples.Add("mhz", new decimal(Math.Pow(10, 6)));
                    multiples.Add("ghz", new decimal(Math.Pow(10, 9)));
                    multiples.Add("thz", new decimal(Math.Pow(10, 12)));
                    multiples.Add("phz", new decimal(Math.Pow(10, 15)));
                    multiples.Add("hhz", new decimal(Math.Pow(10, 18)));
                }

                return multiples;
            }
        }
    }
}