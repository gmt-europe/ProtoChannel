using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel.Util
{
    /// <summary>
    /// Represents a collection whose keys are embedded in the values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public interface IKeyedCollection<TKey, TItem> : IList<TItem>
    {
        /// <summary>
        /// Get the generic equality comparer that is used to determine equality of keys in the collection.
        /// </summary>
        /// <returns>
        /// The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> generic interface that is used to determine equality of keys in the collection.
        /// </returns>
        IEqualityComparer<TKey> Comparer { get; }

        /// <summary>
        /// Get the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key. If an element with the specified key is not found, an exception is thrown.
        /// </returns>
        /// <param name="key">The key of the element to get.</param>
        TItem this[TKey key] { get; }

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.</param>
        bool Contains(TKey key);

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> is not found in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        bool Remove(TKey key);

        /// <summary>
        /// Tries to get the value associated with <paramref name="key"/> located
        /// in the keyed collection.
        /// </summary>
        /// <param name="key">The key to look up in the keyed collection.</param>
        /// <param name="item">The item associated with the key.</param>
        /// <returns>true when the key was found in the collection; otherwise false.</returns>
        bool TryGetValue(TKey key, out TItem item);
    }
}
