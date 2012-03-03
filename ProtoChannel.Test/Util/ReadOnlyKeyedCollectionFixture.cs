using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Util;
using System.Collections;

namespace ProtoChannel.Test.Util
{
    [TestFixture]
    public class ReadOnlyKeyedCollectionFixture : FixtureBase
    {
        [Test]
        [ExpectedException]
        public void CannotAdd()
        {
            GetCollection().Add(new KeyedItem("0"));
        }

        [Test]
        [ExpectedException]
        public void CannotClear()
        {
            GetCollection().Clear();
        }

        [Test]
        public void CanContains()
        {
            GetCollection().Contains("0");
        }

        [Test]
        public void CanCopyTo()
        {
            GetCollection().CopyTo(new KeyedItem[0], 0);
        }

        [Test]
        public void CanCount()
        {
            Assert.AreEqual(GetCollection().Count, 0);
        }

        [Test]
        public void CanIsReadOnly()
        {
            Assert.IsTrue(GetCollection().IsReadOnly);
        }

        [Test]
        [ExpectedException]
        public void CannotRemoveByValue()
        {
            GetCollection().Remove(new KeyedItem("0"));
        }

        [Test]
        public void CanGetEnumerator()
        {
            Assert.NotNull(GetCollection().GetEnumerator());
        }

        [Test]
        public void CanIndexOf()
        {
            var item = new KeyedItem("0");

            Assert.AreEqual(0, GetCollection(item).IndexOf(item));
        }

        [Test]
        [ExpectedException]
        public void CannotInsert()
        {
            GetCollection().Insert(0, new KeyedItem("0"));
        }

        [Test]
        [ExpectedException]
        public void CannotRemoveAt()
        {
            GetCollection().RemoveAt(0);
        }

        [Test]
        public void CanGetIndex()
        {
            var item = new KeyedItem("0");

            Assert.AreEqual(item, GetCollection(item)[0]);
            Assert.AreEqual(item, GetCollection(item)["0"]);
        }

        [Test]
        [ExpectedException]
        public void CannotSetIndex()
        {
            GetCollection()[0] = new KeyedItem("0");
        }

        [Test]
        public void CanContainsByKey()
        {
            Assert.IsTrue(GetCollection(new KeyedItem("0")).Contains("0"));
            Assert.IsFalse(GetCollection(new KeyedItem("1")).Contains("0"));
        }

        [Test]
        public void CanContainsByByValue()
        {
            var item = new KeyedItem("0");

            Assert.IsTrue(GetCollection(item).Contains(item));
            Assert.IsFalse(GetCollection().Contains(item));
        }

        [Test]
        [ExpectedException]
        public void CannotRemoveByKey()
        {
            GetCollection().Remove("0");
        }

        [Test]
        [ExpectedException]
        public void CannotAddList()
        {
            ((IList)GetCollection()).Add(new KeyedItem("0"));
        }

        [Test]
        [ExpectedException]
        public void CannotRemoveList()
        {
            ((IList)GetCollection()).Remove(new KeyedItem("0"));
        }

        [Test]
        [ExpectedException]
        public void CannotInsertList()
        {
            ((IList)GetCollection()).Insert(0, new KeyedItem("0"));
        }

        [Test]
        public void CanIndexOfList()
        {
            var item = new KeyedItem("0");

            Assert.AreEqual(0, ((IList)GetCollection(item)).IndexOf(item));
        }

        [Test]
        public void CanGetIndexList()
        {
            var item = new KeyedItem("0");

            Assert.AreEqual(item, ((IList)GetCollection(item))[0]);
        }

        [Test]
        [ExpectedException]
        public void CannotSetIndexList()
        {
            ((IList)GetCollection())[0] = new KeyedItem("0");
        }

        [Test]
        public void CanCopyToList()
        {
            ((IList)GetCollection()).CopyTo(new KeyedItem[0], 0);
        }

        [Test]
        public void CanContainsList()
        {
            var item = new KeyedItem("0");

            Assert.IsTrue(((IList)GetCollection(item)).Contains(item));
            Assert.IsFalse(((IList)GetCollection()).Contains(item));
        }

        [Test]
        public void CanIsFixedSizeList()
        {
            Assert.IsFalse(((IList)GetCollection()).IsFixedSize);
        }

        [Test]
        public void CanIsSynchronizedCollection()
        {
            Assert.IsFalse(((ICollection)GetCollection()).IsSynchronized);
        }

        [Test]
        public void CanSyncRootCollection()
        {
            Assert.IsNotNull(((ICollection)GetCollection()).SyncRoot);
        }

        [Test]
        public void CanGetEnumeratorList()
        {
            Assert.IsNotNull(((IList)GetCollection()).GetEnumerator());
        }

        [Test]
        public void CanGetEqualityComparer()
        {
            Assert.AreEqual(StringComparer.OrdinalIgnoreCase, GetCollection(StringComparer.OrdinalIgnoreCase).Comparer);
        }

        private ReadOnlyKeyedCollection<string, KeyedItem> GetCollection(params KeyedItem[] items)
        {
            var collection = new KeyedCollection();

            if (items != null)
            {
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }

            return new ReadOnlyKeyedCollection<string, KeyedItem>(collection);
        }

        private ReadOnlyKeyedCollection<string, KeyedItem> GetCollection(IEqualityComparer<string> comparer)
        {
            var collection = new KeyedCollection(comparer);

            return new ReadOnlyKeyedCollection<string, KeyedItem>(collection);
        }

        private class KeyedItem
        {
            public string Value { get; private set; }

            public KeyedItem(string value)
            {
                Value = value;
            }
        }

        private class KeyedCollection : KeyedCollection<string, KeyedItem>
        {
            public KeyedCollection()
            {
            }

            public KeyedCollection(IEqualityComparer<string> comparer)
                : base(comparer)
            {
            }

            protected override string GetKeyForItem(KeyedItem item)
            {
                return item.Value;
            }
        }
    }
}
