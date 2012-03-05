using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Util;

namespace ProtoChannel.Test.Util
{
    [TestFixture]
    public class BitConverterExFixture : FixtureBase
    {
        [Test]
        public void ConvertUintToNetwork()
        {
            uint value = 0x12345678;

            var bytes = BitConverter.GetBytes(value);

            ByteUtil.ConvertNetwork(bytes);

            Assert.AreEqual(bytes, BitConverterEx.GetNetworkBytes(value));
        }

        [Test]
        public void ConvertUintFromNetwork()
        {
            var bytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };

            uint value = BitConverterEx.ToNetworkUInt32(bytes, 0);

            ByteUtil.ConvertNetwork(bytes);

            Assert.AreEqual(BitConverter.ToUInt32(bytes, 0), value);
        }

        [Test]
        public void ConvertUshortToNetwork()
        {
            ushort value = 0x1234;

            var bytes = BitConverter.GetBytes(value);

            ByteUtil.ConvertNetwork(bytes);

            Assert.AreEqual(bytes, BitConverterEx.GetNetworkBytes(value));
        }

        [Test]
        public void ConvertUshortFromNetwork()
        {
            var bytes = new byte[] { 0x12, 0x34 };

            ushort value = BitConverterEx.ToNetworkUInt16(bytes, 0);

            ByteUtil.ConvertNetwork(bytes);

            Assert.AreEqual(BitConverter.ToUInt16(bytes, 0), value);
        }
    }
}
