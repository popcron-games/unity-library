#nullable enable
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Library
{
    public readonly struct RentedArray<T> : IDisposable, IList<T>
    {
        private readonly T[] array;

        public int Length => array.Length;
        public T this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
        
        int ICollection<T>.Count => array.Length;
        bool ICollection<T>.IsReadOnly => false;

        public RentedArray(int length, bool clear = false)
        {
            array = ArrayPool<T>.Shared.Rent(length);
            if (clear)
            {
                Array.Clear(array, 0, array.Length);
            }
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(array);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(array, item);
        }

        public void Clear()
        {
            Array.Clear(array, 0, array.Length);
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(array, item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.array.CopyTo(array, arrayIndex);
        }

        public void Insert(int index, T item)
        {
            array[index] = item;
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
    }
}