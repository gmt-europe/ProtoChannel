using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Util;

namespace ProtoChannel.Test.Util
{
    [TestFixture]
    internal class RingMemoryStreamTest : FixtureBase
    {
        private const int SmallBufferSize = 4;
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz");

        [Test]
        public void BasicReadTest()
        {
            var result = new byte[TestData.Length];

            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(TestData, 0, TestData.Length);
                stream.Position = 0;
                stream.Read(result, 0, result.Length);

                Assert.AreEqual(TestData, result);
                Assert.AreEqual(TestData.Length, stream.Position);
                Assert.AreEqual(TestData.Length, stream.Length);
                Assert.AreEqual(7, stream.PagesAllocated);
                Assert.AreEqual(28, stream.Capacity);
            }
        }

        [Test]
        public void MoveHead()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(TestData, 0, TestData.Length);

                Assert.AreEqual(stream.Head, 0);

                stream.Head = stream.Position;

                Assert.AreEqual(1, stream.PagesAllocated);
            }
        }

        [Test]
        public void PageReUse()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(TestData, 0, TestData.Length);

                stream.Head = stream.Position;

                Assert.AreEqual(1, stream.PagesAllocated);

                stream.Write(TestData, 0, TestData.Length);

                Assert.AreEqual(7, stream.PagesAllocated);
            }
        }

        [Test]
        public void PageValidity()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(TestData, 0, TestData.Length);

                var pages = stream.GetPages(0, stream.Length);

                Assert.AreEqual(7, pages.Length);

                byte[] block;

                for (int i = 0; i < 6; i++)
                {
                    Assert.AreEqual(0, pages[i].Offset);
                    Assert.AreEqual(SmallBufferSize, pages[i].Count);
                    Assert.AreEqual(SmallBufferSize, pages[i].Buffer.Length);

                    block = new byte[SmallBufferSize];

                    Array.Copy(TestData, i * SmallBufferSize, block, 0, SmallBufferSize);

                    Assert.AreEqual(block, pages[i].Buffer);
                }

                Assert.AreEqual(0, pages[6].Offset);
                Assert.AreEqual(2, pages[6].Count);

                block = new byte[2];
                var pagePart = new byte[2];

                Array.Copy(TestData, 6 * SmallBufferSize, block, 0, 2);
                Array.Copy(pages[6].Buffer, 0, pagePart, 0, 2);

                Assert.AreEqual(block, pagePart);
            }
        }

        [Test]
        public void ReadByte()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(TestData, 0, TestData.Length);
                stream.Position = 0;

                for (int i = 0; i < TestData.Length; i++)
                {
                    Assert.AreEqual(TestData[i], stream.ReadByte());
                    Assert.AreEqual(i + 1, stream.Position);
                }
            }
        }

        [Test]
        public void WriteInBytes()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                foreach (byte b in TestData)
                {
                    stream.WriteByte(b);
                }

                stream.Position = 0;

                byte[] buffer = new byte[TestData.Length];

                stream.Read(buffer, 0, buffer.Length);

                Assert.AreEqual(TestData, buffer);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadBeyondEnd()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.ReadByte();
            }
        }

        [Test]
        public void Enlarge_pages_array()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                for (int i = 0; i < 10; i++)
                {
                    stream.Write(TestData, 0, TestData.Length);
                }
            }
        }

        [Test]
        public void SetLargeCapacity()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity = SmallBufferSize * 100;

                Assert.AreEqual(SmallBufferSize * 100, stream.Capacity);
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(0, stream.Head);
                Assert.AreEqual(0, stream.Length);
            }
        }

        [Test]
        public void Set_large_length()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.SetLength(SmallBufferSize * 100);

                Assert.AreEqual(SmallBufferSize * 100, stream.Capacity);
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(0, stream.Head);
                Assert.AreEqual(SmallBufferSize * 100, stream.Length);
            }
        }

        [Test]
        public void CapacityRounded()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.SetLength(1);

                Assert.AreEqual(1, stream.Length);
                Assert.AreEqual(SmallBufferSize, stream.Capacity);
            }
        }

        [Test]
        public void GetPagesBeyondLength()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity++;

                var pages = stream.GetPages(0, stream.Capacity);

                Assert.AreEqual(1, pages.Length);
                Assert.AreEqual(0, pages[0].Offset);
                Assert.AreEqual(SmallBufferSize, pages[0].Count);
            }
        }

        [Test]
        public void ManualWrite()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity++;

                var pages = stream.GetPages(0, stream.Capacity);

                Array.Copy(TestData, pages[0].Buffer, SmallBufferSize);

                stream.SetLength(SmallBufferSize);

                var readBuffer = new byte[SmallBufferSize];

                stream.Read(readBuffer, 0, readBuffer.Length);

                var validateBuffer = new byte[SmallBufferSize];

                Array.Copy(TestData, validateBuffer, SmallBufferSize);

                Assert.AreEqual(validateBuffer, readBuffer);
            }
        }

        [Test]
        public void HeadWithBlockSizeIncrement()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity++;

                stream.SetLength(stream.Capacity);
                stream.Position = stream.Length;
                stream.Head = stream.Length;

                Assert.AreEqual(0, stream.PagesAllocated);
            }
        }

        [Test]
        public void HeadWithOneLessBlockSize()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity++;

                stream.SetLength(stream.Capacity - 1);
                stream.Position = stream.Length;
                stream.Head = stream.Length;

                Assert.AreEqual(1, stream.PagesAllocated);
            }
        }

        [Test]
        public void HeadWithOneMoreBlockSize()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity++;
                stream.Capacity++;

                stream.SetLength(SmallBufferSize + 1);
                stream.Position = stream.Length;
                stream.Head = stream.Length;

                Assert.AreEqual(1, stream.PagesAllocated);
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void VerifyDisposed()
        {
            var stream = new RingMemoryStream(SmallBufferSize);

            stream.Dispose();

            stream.SetLength(0);
        }

        [Test]
        public void GetWriteBuffer()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                var page = stream.GetWriteBuffer();

                Assert.AreEqual(1, stream.PagesAllocated);
                Assert.AreEqual(page.Offset, 0);
                Assert.AreEqual(page.Count, SmallBufferSize);
            }
        }

        [Test]
        public void GetWriteBufferAfterWrite()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.WriteByte(0);

                var page = stream.GetWriteBuffer();

                Assert.AreEqual(1, stream.PagesAllocated);
                Assert.AreEqual(page.Offset, 1);
                Assert.AreEqual(page.Count, SmallBufferSize - 1);
            }
        }

        [Test]
        public void CorrectReadWhenNotOnPageBoundary()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(TestData, 0, TestData.Length);

                stream.Position = 1;

                var readBuffer = new byte[2];

                stream.Read(readBuffer, 0, 2);
            }
        }

        [Test]
        public void GetCorrectSinglePage()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity++;

                var page = stream.GetPage(0, SmallBufferSize);

                Assert.AreEqual(0, page.Offset);
                Assert.AreEqual(SmallBufferSize, page.Count);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPageWithoutCapacity()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.GetPage(0, SmallBufferSize);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetTooLargeSinglePage()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Capacity = SmallBufferSize * 2;

                stream.GetPage(0, SmallBufferSize * 2);
            }
        }

        [Test]
        public void CanSeek()
        {
            Assert.IsTrue(new RingMemoryStream(SmallBufferSize).CanSeek);
        }

        [Test]
        public void NulLengthReadReturnsNul()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                Assert.AreEqual(0, stream.Read(new byte[0], 0, 0));
            }
        }

        [Test]
        public void CanWriteNulLength()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Write(new byte[0], 0, 0);
            }
        }

        [Test]
        [ExpectedException]
        public void CannotSeekBeyondStart()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Seek(-1, SeekOrigin.Current);
            }
        }

        [Test]
        [ExpectedException]
        public void CannotSeekBeyondEnd()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Seek(1, SeekOrigin.Current);
            }
        }

        [Test]
        public void CanSeekFromEnd()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.Seek(0, SeekOrigin.End);
            }
        }

        [Test]
        public void CanDecreaseLength()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.WriteByte(0);
                stream.Position = 0;
                stream.SetLength(0);
            }
        }

        [Test]
        [ExpectedException]
        public void CannotDecreaseBeyondPosition()
        {
            using (var stream = new RingMemoryStream(SmallBufferSize))
            {
                stream.WriteByte(0);
                stream.SetLength(0);
            }
        }
    }
}
