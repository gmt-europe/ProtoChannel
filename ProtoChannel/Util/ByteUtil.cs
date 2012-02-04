using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Util
{
    internal static class ByteUtil
    {
        public static void ConvertNetwork(byte[] value)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(value);
        }

        public static void ConvertNetwork(byte[] value, int index, int length)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(value, index, length);
        }

        public static bool Equals(byte[] x, byte[] y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public static bool Equals(byte[] x, byte[] y, int length)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            if (x.Length < length || y.Length < length)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }
    }
}
