using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CheckAlgorithms
    {
        public static int Mod97(IList<int> digits)
        {
            var ret = 0;
            for (var i = 0; i < digits.Count; i++)
                ret = (10 * ret + digits[i]) % 97;
            return ret;
        }


        public static string DC(string CC)
        {
            int[] Pesos = {1, 2, 4, 8, 5, 10, 9, 7, 3, 6};
            var SumaEntidadSucursal = 0;
            var SumaCodigoCuenta = 0;

            if (!Regex.IsMatch(CC, @"\d{20}"))
                throw new Exception("Invalid format for cc");


            for (var i = 0; i < 8; i++)
            {
                var digit = int.Parse(CC.Substring(i, 1));
                SumaEntidadSucursal += digit * Pesos[i + 2];
            }

            var remainder = SumaEntidadSucursal % 11;
            SumaEntidadSucursal = 11 - remainder;
            if (SumaEntidadSucursal >= 10)
                SumaEntidadSucursal = remainder;

            for (var i = 0; i <= 9; i++)
            {
                var digit = int.Parse(CC.Substring(i + 10, 1));
                SumaCodigoCuenta += digit * Pesos[i];
            }

            remainder = SumaCodigoCuenta % 11;
            SumaCodigoCuenta = 11 - remainder;
            if (SumaCodigoCuenta >= 10)
                SumaCodigoCuenta = remainder;

            return "" + (SumaEntidadSucursal * 10 + SumaCodigoCuenta);
        }
    }
}