using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ProtoChannel.Util
{
    /// <summary>
    /// Provides a read-only wrapper class for a collection whose keys are embedded in the values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public sealed class ReadOnlyKeyedCollection<TKey, TItem> : IKeyedCollection<TKey, TItem>, IList
    {
        private readonly IKeyedCollection<TKey, TItem> _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyKeyedCollection{TKey,TItem}"/>
        /// class with the specified source collection.
        /// </summary>
        /// <param name="source">The <see cref="KeyedCollection{TKey,TValue}"/>
        /// that will be wrapped.</param>
        public ReadOnlyKeyedCollection(IKeyedCollection<TKey, TItem> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _source = source;
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(TItem item)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(TItem item)
        {
            return _source.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination
        /// of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The
        /// number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater
        /// than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.-or-Type <typeparamref name="TItem"/> cannot be cast automatically to the
        /// type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            _source.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Get the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return ((ICollection)_source).Count; }
        }

        /// <summary>
        /// Get a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(TItem item)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TItem> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_source).GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        public int IndexOf(TItem item)
        {
            return _source.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void Insert(int index, TItem item)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Get or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public TItem this[int index]
        {
            get { return ((IList<TItem>)_source)[index]; }
            set
            {
                throw new InvalidOperationException("Keyed collection is read-only");
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <returns>
        /// The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection,
        /// </returns>
        /// <param name="value">The object to add to the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><filterpriority>2</filterpriority>
        int IList.Add(object value)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
        /// </returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>. </param><filterpriority>2</filterpriority>
        bool IList.Contains(object value)
        {
            return ((IList)_source).Contains(value);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>. </param><filterpriority>2</filterpriority>
        int IList.IndexOf(object value)
        {
            return ((IList)_source).IndexOf(value);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted. </param><param name="value">The object to insert into the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><exception cref="T:System.NullReferenceException"><paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception><filterpriority>2</filterpriority>
        void IList.Insert(int index, object value)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Get a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        bool IList.IsFixedSize
        {
            get { return ((IList)_source).IsFixedSize; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><filterpriority>2</filterpriority>
        void IList.Remove(object value)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Get or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.IList"/> is read-only. </exception><filterpriority>2</filterpriority>
        object IList.this[int index]
        {
            get { return ((IList)_source)[index]; }
            set
            {
                throw new InvalidOperationException("Keyed collection is read-only");
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception><exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception><filterpriority>2</filterpriority>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_source).CopyTo(array, index);
        }

        /// <summary>
        /// Get a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_source).IsSynchronized; }
        }

        /// <summary>
        /// Get an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        object ICollection.SyncRoot
        {
            get { return ((ICollection)_source).SyncRoot; }
        }

        /// <summary>
        /// Get the generic equality comparer that is used to determine equality of keys in the collection.
        /// </summary>
        /// <returns>
        /// The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> generic interface that is used to determine equality of keys in the collection.
        /// </returns>
        public IEqualityComparer<TKey> Comparer
        {
            get { return _source.Comparer; }
        }

        /// <summary>
        /// Get the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key. If an element with the specified key is not found, an exception is thrown.
        /// </returns>
        /// <param name="key">The key of the element to get.</param>
        public TItem this[TKey key]
        {
            get { return _source[key]; }
        }

        /// <summary>
        /// Test whether the collection contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.</param>
        public bool Contains(TKey key)
        {
            return _source.Contains(key);
        }

        /// <summary>
        /// Remove the element with the specified key from the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> is not found in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        public bool Remove(TKey key)
        {
            throw new InvalidOperationException("Keyed collection is read-only");
        }

        /// <summary>
        /// Try to get the value associated with <paramref name="key"/> located
        /// in the keyed collection.
        /// </summary>
        /// <param name="key">The key to look up in the keyed collection.</param>
        /// <param name="item">The item associated with the key.</param>
        /// <returns>true when the key was found in the collection; otherwise false.</returns>
        public bool TryGetValue(TKey key, out TItem item)
        {
            return _source.TryGetValue(key, out item);
        }
    }
}
