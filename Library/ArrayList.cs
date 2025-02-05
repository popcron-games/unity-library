#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityLibrary
{
    public sealed class ArrayList : IReadOnlyList<object>
    {
        private object[] array;
        private int count;
        private int capacity;

        public int Count => count;
        public object this[int index] => array[index];

        public ArrayList(int capacity = 4)
        {
            array = new object[capacity];
            this.capacity = capacity;
        }

        public void Add(object value)
        {
            if (count == capacity)
            {
                capacity *= 2;
                Array.Resize(ref array, capacity);
            }

            array[count++] = value;
        }

        public void Clear()
        {
            count = 0;
        }

        public void Remove(object value)
        {
            int index = Array.IndexOf(array, value);
            if (index != -1)
            {
                RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            if (index < count)
            {
                count--;
                Array.Copy(array, index + 1, array, index, count - index);
            }
        }

        public IReadOnlyList<T> AsReadOnlyList<T>()
        {
            return new ReadOnlyList<T>(this);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<object>
        {
            private readonly ArrayList list;
            private int index;

            public readonly object Current => list[index];

            public Enumerator(ArrayList list)
            {
                this.list = list;
                index = -1;
            }

            public bool MoveNext()
            {
                return ++index < list.count;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
            }
        }

        public readonly struct ReadOnlyList<T> : IReadOnlyList<T>
        {
            private readonly ArrayList list;

            public readonly int Count => list.Count;
            public readonly T this[int index] => (T)list[index];

            internal ReadOnlyList(ArrayList list)
            {
                this.list = list;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(list);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public struct Enumerator : IEnumerator<T>
            {
                private readonly ArrayList list;
                private int index;

                public readonly T Current => (T)list[index];
                readonly object IEnumerator.Current => Current!;

                public Enumerator(ArrayList list)
                {
                    this.list = list;
                    index = -1;
                }

                public bool MoveNext()
                {
                    return ++index < list.Count;
                }

                public void Reset()
                {
                    index = -1;
                }

                public void Dispose()
                {
                }
            }
        }
    }
}