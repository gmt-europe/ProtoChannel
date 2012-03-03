using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Util;

namespace ProtoChannel.Test.Util
{
    [TestFixture]
    public class KeyedCollectionFixture : FixtureBase
    {
        [Test]
        public void MissingReturnsDefault()
        {
            var collection = new KeyedCollection();

            KeyedItem result;

            Assert.IsFalse(collection.TryGetValue("0", out result));

            Assert.IsNull(result);
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
