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
    public readonly struct RentedBuffer<T> : IDisposable, IList<T>
    {
        private readonly T[] buffer;

        public int Length => buffer.Length;
        public T this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }

        int ICollection<T>.Count => buffer.Length;
        bool ICollection<T>.IsReadOnly => false;

        public RentedBuffer(int length, bool clear = false)
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
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)buffer).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(buffer, item);
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(buffer, item) >= 0;
        }

        /// <summary>
        /// Copies the rented buffer into <paramref name="array"/>, starting at <paramref name="arrayIndex"/>.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex = 0)
        {
            buffer.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the given <paramref name="array"/> into the rented buffer, starting at <paramref name="arrayIndex"/>.
        /// </summary>
        public void CopyFrom(T[] array, int arrayIndex = 0)
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

        public void Insert(int index, T item)
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

        public static implicit operator T[](RentedBuffer<T> buffer)
        {
            return buffer.buffer;
        }
    }
}