using System;
using System.Collections.Generic;
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
            Require.NotNull(x, "x");
            Require.NotNull(y, "y");

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
            Require.NotNull(x, "x");
            Require.NotNull(y, "y");

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
