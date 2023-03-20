using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
	public class CheckAlgorithms
	{
		public CheckAlgorithms ()
		{
		}


		public static int Mod97(IList<int> digits){
			int ret=0;
			for(int i = 0; i < digits.Count; i++)
				ret = (10 * ret + digits[i]) % 97;
			return ret;
		}


		public static String DC(String CC){
			int[] Pesos = new int[] { 1, 2, 4, 8, 5, 10, 9, 7, 3, 6 };
			int SumaEntidadSucursal = 0;
			int SumaCodigoCuenta = 0;

			if (!Regex.IsMatch (CC, @"\d{20}"))
				throw new Exception ("Invalid format for cc");
			

			for (int i = 0; i < 8; i++) {
				int digit = Int32.Parse (CC.Substring (i, 1));
				SumaEntidadSucursal += digit * Pesos [i + 2];
			}
			int remainder = SumaEntidadSucursal % 11;
			SumaEntidadSucursal = 11 - remainder;
			if (SumaEntidadSucursal >= 10)
				SumaEntidadSucursal = remainder;
			
			for (int i = 0; i <= 9; i++) {
				int digit = Int32.Parse (CC.Substring (i + 10, 1));
				SumaCodigoCuenta += digit * Pesos [i];
			}
			remainder = SumaCodigoCuenta % 11;
			SumaCodigoCuenta = 11 - remainder;
			if (SumaCodigoCuenta >= 10)
				SumaCodigoCuenta = remainder;
			
			return "" + (SumaEntidadSucursal * 10 + SumaCodigoCuenta);
		}
	}
}

