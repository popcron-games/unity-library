#nullable enable
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Popcron
{
    public struct RecycledList<T> : IList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        private T[] rentedBuffer;
        private int count;
        private int capacity;

        public T this[int index]
        {
            get => rentedBuffer[index];
            set => rentedBuffer[index] = value;
        }

        public int Count => count;
        public bool IsReadOnly => false;

        public RecycledList(int capacity)
        {
            rentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
            count = 0;
            this.capacity = capacity;

            RecycledList<T> me = this;
            new PostLateUpdateEvent().AddListenerOneShot((e) =>
            {
                ArrayPool<T>.Shared.Return(me.rentedBuffer);
            });
        }

        public RecycledList(T firstElement)
        {
            rentedBuffer = ArrayPool<T>.Shared.Rent(4);
            count = 0;
            capacity = 4;

            RecycledList<T> me = this;
            new PostLateUpdateEvent().AddListenerOneShot((e) =>
            {
                ArrayPool<T>.Shared.Return(me.rentedBuffer);
            });

            Add(firstElement);
        }

        public RecycledList(ICollection<T> collection)
        {
            rentedBuffer = ArrayPool<T>.Shared.Rent(collection.Count);
            count = collection.Count;
            capacity = rentedBuffer.Length;

            RecycledList<T> me = this;
            new PostLateUpdateEvent().AddListenerOneShot((e) =>
            {
                ArrayPool<T>.Shared.Return(me.rentedBuffer);
            });

            collection.CopyTo(rentedBuffer, 0);
        }

        public RecycledList(IEnumerable<T> collection)
        {
            rentedBuffer = ArrayPool<T>.Shared.Rent(4);
            count = 0;
            capacity = 4;

            RecycledList<T> me = this;
            new PostLateUpdateEvent().AddListenerOneShot((e) =>
            {
                ArrayPool<T>.Shared.Return(me.rentedBuffer);
            });

            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public ReadOnlySpan<T> AsSpan()
        {
            return new ReadOnlySpan<T>(rentedBuffer, 0, count);
        }

        public void Add(T item)
        {
            GrowIfNecessary(count + 1);
            rentedBuffer[count++] = item;
        }

        public void AddWithoutResize(T item)
        {
            rentedBuffer[count++] = item;
        }

        public void Clear()
        {
            count = 0;
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(rentedBuffer, item, 0, count) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex + i] = rentedBuffer[i];
            }
        }

        public ref T ElementAt(int index)
        {
            return ref rentedBuffer[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return rentedBuffer[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(rentedBuffer, item, 0, count);
        }

        public void Insert(int index, T item)
        {
            GrowIfNecessary(count + 1);
            T lastItem = rentedBuffer[index];
            rentedBuffer[index] = item;
            rentedBuffer[count] = lastItem;
            count++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            rentedBuffer[index] = rentedBuffer[count - 1];
            count--;
        }

        private void GrowIfNecessary(int desiredCapacity)
        {
            if (desiredCapacity > capacity)
            {
                int newCapacity = capacity * 2;
                if (newCapacity < desiredCapacity) newCapacity = desiredCapacity;

                T[] newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);
                rentedBuffer.CopyTo(newBuffer, 0);
                ArrayPool<T>.Shared.Return(rentedBuffer);

                rentedBuffer = newBuffer;
                capacity = newCapacity;
            }
        }
    }
}
