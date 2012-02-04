using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel.Util
{
    /// <summary>
    /// Provides the abstract base class for a collection whose keys are embedded in the values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public abstract class KeyedCollection<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>, IKeyedCollection<TKey, TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedCollection{TKey,TItem}"/> class that uses the default equality comparer.
        /// </summary>
        protected KeyedCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedCollection{TKey,TItem}"/> class that uses the specified equality comparer.
        /// </summary>
        /// <param name="comparer">The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> generic interface to use when comparing keys, or null to use the default equality comparer for the type of the key, obtained from <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.</param>
        protected KeyedCollection(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedCollection{TKey,TItem}"/> class that uses the specified equality comparer and creates a lookup dictionary when the specified threshold is exceeded.
        /// </summary>
        /// <param name="comparer">The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> generic interface to use when comparing keys, or null to use the default equality comparer for the type of the key, obtained from <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.</param><param name="dictionaryCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary (0 creates the lookup dictionary when the first item is added), or –1 to specify that a lookup dictionary is never created.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="dictionaryCreationThreshold"/> is less than –1.</exception>
        protected KeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
            : base(comparer, dictionaryCreationThreshold)
        {
        }

        /// <summary>
        /// Tries to get the value associated with <paramref name="key"/> located
        /// in the keyed collection.
        /// </summary>
        /// <param name="key">The key to look up in the keyed collection.</param>
        /// <param name="item">The item associated with the key.</param>
        /// <returns>true when the key was found in the collection; otherwise false.</returns>
        public bool TryGetValue(TKey key, out TItem item)
        {
            if (Contains(key))
            {
                item = this[key];
                return true;
            }
            else
            {
                item = default(TItem);
                return false;
            }
        }
    }
}
