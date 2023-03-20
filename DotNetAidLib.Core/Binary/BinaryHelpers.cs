using System;
using System.Linq;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

// Assert\.NotNullOrEmpty\s*\(\s*nameof\s*\(\s*([^)]+)\s*\)\s*,([^)]+)\);
// Assert.NotNullOrEmpty\($2, nameof($1)); 

// Assert\.NotNull\s*\(\s*nameof\s*\(\s*([^)]+)\s*\)\s*,([^)]+)\);
// Assert.NotNull\($2, nameof($1)); 

namespace DotNetAidLib.Core.Binary
{
    public static class BinaryHelpers
    {
        public static int ToBit(this bool v) {
            return v ? 1 : 0;
        }

        public static TResult ToBit<TResult>(this bool v, TResult trueValue, TResult falseValue){
            return v ? trueValue : falseValue;
        }
        
        public static bool HasFlag(this Int32 v, params Int32[] flags)
        {
	        return flags.All(f=>(f & v)==f);
        }

        public static bool HasFlag(this UInt32 v, params UInt32[] flags)
        {
	        return flags.All(f=>(f & v)==f);
        }

        public static bool GetBit(this UInt64 v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(UInt64.MaxValue)-1, nameof(index));
            UInt64 mask = (UInt64)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this UInt32 v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(UInt32.MaxValue) - 1, nameof(index));
            UInt32 mask = (UInt32)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this UInt16 v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(UInt16.MaxValue) - 1, nameof(index));
            UInt16 mask = (UInt16)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this byte v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(byte.MaxValue) - 1, nameof(index));
            byte mask = (byte)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this Int64 v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(Int64.MaxValue) - 1, nameof(index));
            Int64 mask = (Int64)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this Int32 v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(Int32.MaxValue) - 1, nameof(index));
            Int32 mask = (Int32)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this Int16 v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (Int32)Math.Sqrt(Int16.MaxValue) - 1, nameof(index));
            Int16 mask = (Int16)Math.Pow(2D, (double)index);
            return (v & mask) == mask;
        }
        
        public static byte GetByte(this UInt16[] v, int index)
        {
        	byte ret = (byte)v[index];
        	return ret;
        }

        public static UInt32 GetUInt32(this UInt16[] v, int index)
        {
        	UInt32 ret = (UInt32)v[index] << 16;
        	ret = ret + v[index + 1];
        	return ret;
        }

        public static UInt64 GetUInt64(this UInt16[] v, int index)
        {
        	UInt64 ret = (UInt64)v[index] << 48;
        	ret = ret + (UInt64)v[index + 1] << 32;
        	ret = ret + (UInt64)v[index + 2] << 16;
        	ret = ret + v[index + 3];
        	return ret;
        }

        public static float GetFloat(this UInt16[] v, int index)
        {
        	return v.GetUInt32(index).ToIEE754Float();
        }

        public static void SetByte(this UInt16[] v, int index, byte value)
        {
        	v[index] = (UInt16)(value);
        }

        public static void SetUInt32(this UInt16[] v, int index, UInt32 value)
        {
        	v[index] = (UInt16)(value >> 16);
        	v[index + 1] = (UInt16)(value & 0x0000FFFF);
        }

        public static void SetUInt64(this UInt16[] v, int index, UInt64 value)
        {
        	v[index] = (UInt16)(value >> 48);
        	v[index + 1] = (UInt16)((value & 0x0000FFFF00000000) >> 32);
        	v[index + 2] = (UInt16)((value & 0x00000000FFFF0000) >> 16);
        	v[index + 3] = (UInt16)(value & 0x000000000000FFFF);
        }

        public static void SetFloat(this UInt16[] v, int index, float value)
        {
        	v.SetUInt32(index, value.ToIEE754Int32());
        }

        public static float ToIEE754Float(this UInt32 v)
        {
        	return BitConverter.ToSingle(BitConverter.GetBytes(v), 0);
        }

        public static UInt32 ToIEE754Int32(this float v)
        {
        	return Convert.ToUInt32(v);
        }
        public static byte[] GetBytes(this decimal dec)
        {
            //Load four 32 bit integers from the Decimal.GetBits function
            Int32[] bits = decimal.GetBits(dec);
            //Create a temporary list to hold the bytes
            List<byte> bytes = new List<byte>();
            //iterate each 32 bit integer
            foreach (Int32 i in bits)
            {
                //add the bytes of the current 32bit integer
                //to the bytes list
                bytes.AddRange(BitConverter.GetBytes(i));
            }
            //return the bytes list as an array
            return bytes.ToArray();
        }

        public static decimal FromBytes(this decimal v, byte[] bytes)
        {
            //check that it is even possible to convert the array
            if (bytes.Count() != 16)
                throw new Exception("A decimal must be created from exactly 16 bytes");
            //make an array to convert back to int32's
            Int32[] bits = new Int32[4];
            for (int i = 0; i <= 15; i += 4)
            {
                //convert every 4 bytes into an int32
                bits[i/4] = BitConverter.ToInt32(bytes, i);
            }
            //Use the decimal's new constructor to
            //create an instance of decimal
            return new decimal(bits);
        } 
    }
}