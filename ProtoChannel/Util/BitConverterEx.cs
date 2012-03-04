using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Util
{
    internal static class BitConverterEx
    {
        public static byte[] GetNetworkBytes(uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return new[]
                {
                    (byte)((value >> 24) & 0xff),
                    (byte)((value >> 16) & 0xff),
                    (byte)((value >> 8) & 0xff),
                    (byte)(value & 0xff)
                };
            }
            else
            {
                return new[]
                {
                    (byte)(value & 0xff),
                    (byte)((value >> 8) & 0xff),
                    (byte)((value >> 16) & 0xff),
                    (byte)((value >> 24) & 0xff)
                };
            }
        }

        public static uint ToNetworkUInt32(byte[] buffer, int offset)
        {
            Require.NotNull(buffer, "buffer");

            if (BitConverter.IsLittleEndian)
            {
                return
                    buffer[offset + 3] |
                    (uint)buffer[offset + 2] << 8 |
                    (uint)buffer[offset + 1] << 16 |
                    (uint)buffer[offset] << 24;
            }
            else
            {
                return
                    buffer[offset] |
                    (uint)buffer[offset + 1] << 8 |
                    (uint)buffer[offset + 2] << 16 |
                    (uint)buffer[offset + 3] << 24;
            }
        }

        public static byte[] GetNetworkBytes(ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return new[]
                {
                    (byte)((value >> 8) & 0xff),
                    (byte)(value & 0xff)
                };
            }
            else
            {
                return new[]
                {
                    (byte)(value & 0xff),
                    (byte)((value >> 8) & 0xff)
                };
            }
        }

        public static ushort ToNetworkUInt16(byte[] buffer, int offset)
        {
            Require.NotNull(buffer, "buffer");

            if (BitConverter.IsLittleEndian)
            {
                return (ushort)(
                    buffer[offset + 1] |
                    (uint)buffer[offset] << 8
                );
            }
            else
            {
                return (ushort)(
                    buffer[offset] |
                    (uint)buffer[offset + 1] << 8
                );
            }
        }
    }
}
