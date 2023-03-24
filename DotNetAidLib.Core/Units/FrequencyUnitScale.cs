using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Units
{
    public class FrequencyUnitScale:MultipleValueScale
	{
        private static Dictionary<String, decimal> multiples = null;

        public FrequencyUnitScale(String value)
            : base(value) { }

        public FrequencyUnitScale(Decimal decimalValue, String multiple)
            : base(decimalValue, multiple) { }

        public override IDictionary<String, decimal> Multiples
        {
            get{
                if (multiples == null){
                    multiples = new Dictionary<String, decimal>();
                    multiples.Add("hz", 1);
                    multiples.Add("khz", new Decimal(Math.Pow(10, 3)));
                    multiples.Add("mhz", new Decimal(Math.Pow(10, 6)));
                    multiples.Add("ghz", new Decimal(Math.Pow(10, 9)));
                    multiples.Add("thz", new Decimal(Math.Pow(10, 12)));
                    multiples.Add("phz", new Decimal(Math.Pow(10, 15)));
                    multiples.Add("hhz", new Decimal(Math.Pow(10, 18)));
                }

                return multiples;
            }
        }
	}
}
