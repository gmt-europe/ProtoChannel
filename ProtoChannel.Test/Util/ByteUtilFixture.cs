using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Util;

namespace ProtoChannel.Test.Util
{
    [TestFixture]
    public class ByteUtilFixture : FixtureBase
    {
        [Test]
        public void WithoutLength()
        {
            Assert.IsTrue(ByteUtil.Equals(new byte[] { 1, 2 }, new byte[] { 1, 2 }));
            Assert.IsFalse(ByteUtil.Equals(new byte[] { 1 }, new byte[] { 1, 2 }));
            Assert.IsFalse(ByteUtil.Equals(new byte[] { 1, 2 }, new byte[] { 1, 3 }));
        }

        [Test]
        public void WithLength()
        {
            Assert.IsTrue(ByteUtil.Equals(new byte[] { 1, 2 }, new byte[] { 1, 2 }, 1));
            Assert.IsFalse(ByteUtil.Equals(new byte[] { 1, 2 }, new byte[] { 1, 2 }, 3));
            Assert.IsTrue(ByteUtil.Equals(new byte[] { 1, 2 }, new byte[] { 1, 2 }, 2));
            Assert.IsFalse(ByteUtil.Equals(new byte[] { 1 }, new byte[] { 1, 2 }, 2));
            Assert.IsFalse(ByteUtil.Equals(new byte[] { 1, 2 }, new byte[] { 1, 3 }, 2));
        }
    }
}
