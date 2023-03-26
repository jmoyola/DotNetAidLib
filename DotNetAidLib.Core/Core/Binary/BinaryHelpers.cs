using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Binary
{
    public static class BinaryHelpers
    {
        public static int ToBit(this bool v)
        {
            return v ? 1 : 0;
        }

        public static TResult ToBit<TResult>(this bool v, TResult trueValue, TResult falseValue)
        {
            return v ? trueValue : falseValue;
        }

        public static bool HasFlag(this int v, params int[] flags)
        {
            return flags.All(f => (f & v) == f);
        }

        public static bool HasFlag(this uint v, params uint[] flags)
        {
            return flags.All(f => (f & v) == f);
        }

        public static bool GetBit(this ulong v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(ulong.MaxValue) - 1, nameof(index));
            var mask = (ulong) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this uint v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(uint.MaxValue) - 1, nameof(index));
            var mask = (uint) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this ushort v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(ushort.MaxValue) - 1, nameof(index));
            var mask = (ushort) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this byte v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(byte.MaxValue) - 1, nameof(index));
            var mask = (byte) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this long v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(long.MaxValue) - 1, nameof(index));
            var mask = (long) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this int v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(int.MaxValue) - 1, nameof(index));
            var mask = (int) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static bool GetBit(this short v, int index)
        {
            Assert.BetweenOrEqual(index, 0, (int) Math.Sqrt(short.MaxValue) - 1, nameof(index));
            var mask = (short) Math.Pow(2D, index);
            return (v & mask) == mask;
        }

        public static byte GetByte(this ushort[] v, int index)
        {
            var ret = (byte) v[index];
            return ret;
        }

        public static uint GetUInt32(this ushort[] v, int index)
        {
            var ret = (uint) v[index] << 16;
            ret = ret + v[index + 1];
            return ret;
        }

        public static ulong GetUInt64(this ushort[] v, int index)
        {
            var ret = (ulong) v[index] << 48;
            ret = (ret + v[index + 1]) << 32;
            ret = (ret + v[index + 2]) << 16;
            ret = ret + v[index + 3];
            return ret;
        }

        public static float GetFloat(this ushort[] v, int index)
        {
            return v.GetUInt32(index).ToIEE754Float();
        }

        public static void SetByte(this ushort[] v, int index, byte value)
        {
            v[index] = value;
        }

        public static void SetUInt32(this ushort[] v, int index, uint value)
        {
            v[index] = (ushort) (value >> 16);
            v[index + 1] = (ushort) (value & 0x0000FFFF);
        }

        public static void SetUInt64(this ushort[] v, int index, ulong value)
        {
            v[index] = (ushort) (value >> 48);
            v[index + 1] = (ushort) ((value & 0x0000FFFF00000000) >> 32);
            v[index + 2] = (ushort) ((value & 0x00000000FFFF0000) >> 16);
            v[index + 3] = (ushort) (value & 0x000000000000FFFF);
        }

        public static void SetFloat(this ushort[] v, int index, float value)
        {
            v.SetUInt32(index, value.ToIEE754Int32());
        }

        public static float ToIEE754Float(this uint v)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(v), 0);
        }

        public static uint ToIEE754Int32(this float v)
        {
            return Convert.ToUInt32(v);
        }

        public static byte[] GetBytes(this decimal dec)
        {
            //Load four 32 bit integers from the Decimal.GetBits function
            var bits = decimal.GetBits(dec);
            //Create a temporary list to hold the bytes
            var bytes = new List<byte>();
            //iterate each 32 bit integer
            foreach (var i in bits)
                //add the bytes of the current 32bit integer
                //to the bytes list
                bytes.AddRange(BitConverter.GetBytes(i));
            //return the bytes list as an array
            return bytes.ToArray();
        }

        public static decimal FromBytes(this decimal v, byte[] bytes)
        {
            //check that it is even possible to convert the array
            if (bytes.Count() != 16)
                throw new Exception("A decimal must be created from exactly 16 bytes");
            //make an array to convert back to int32's
            var bits = new int[4];
            for (var i = 0; i <= 15; i += 4)
                //convert every 4 bytes into an int32
                bits[i / 4] = BitConverter.ToInt32(bytes, i);
            //Use the decimal's new constructor to
            //create an instance of decimal
            return new decimal(bits);
        }
    }
}