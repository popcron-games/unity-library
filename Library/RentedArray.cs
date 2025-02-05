#nullable enable
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// A rented array backed by <see cref="ArrayPool{T}"/> that gets returned
    /// when <see cref="Dispose"/> is called. 
    /// <para></para>
    /// Can be implicitly cast to into an array of <typeparamref name="T"/>,
    /// and can be used as a <see cref="IList{T}"/> directly.
    /// </summary>
    public struct RentedArray<T> : IDisposable, IList<T>
    {
        private T[] buffer;

        public readonly int Length => buffer.Length;
        public readonly T this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }

        public readonly bool IsDisposed => buffer is null;
        readonly int ICollection<T>.Count => buffer.Length;
        readonly bool ICollection<T>.IsReadOnly => false;

        public RentedArray(int length, bool clear = false)
        {
            buffer = ArrayPool<T>.Shared.Rent(length);
            if (clear)
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(buffer);
            buffer = null!;
        }

        public readonly IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)buffer).GetEnumerator();
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        public readonly int IndexOf(T item)
        {
            return Array.IndexOf(buffer, item);
        }

        public readonly void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
        }

        public readonly bool Contains(T item)
        {
            return Array.IndexOf(buffer, item) >= 0;
        }

        /// <summary>
        /// Copies the rented buffer into <paramref name="array"/>, starting at <paramref name="arrayIndex"/>.
        /// </summary>
        public readonly void CopyTo(T[] array, int arrayIndex = 0)
        {
            buffer.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the given <paramref name="array"/> into the rented buffer, starting at <paramref name="arrayIndex"/>.
        /// </summary>
        public readonly void CopyFrom(T[] array, int arrayIndex = 0)
        {
            array.CopyTo(buffer, arrayIndex);
        }

        /// <summary>
        /// Copies the given <paramref name="list"/> into the rented buffer, starting at <paramref name="arrayIndex"/>.
        /// </summary>
        public void CopyFrom(IList<T> list, int arrayIndex = 0)
        {
            int length = list.Count;
            for (int i = 0; i < length; i++)
            {
                buffer[i + arrayIndex] = list[i];
            }
        }

        public readonly void Insert(int index, T item)
        {
            buffer[index] = item;
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        public static implicit operator T[](RentedArray<T> buffer)
        {
            return buffer.buffer;
        }
    }
}