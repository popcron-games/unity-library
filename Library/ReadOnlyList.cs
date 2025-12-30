using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityLibrary
{
    public readonly struct ReadOnlyList<T> : IReadOnlyList<T>
    {
        public static readonly ReadOnlyList<T> Empty = new(Array.Empty<object>());

        private readonly ArrayList list;

        public readonly int Count => list.count;
        public readonly T this[int index] => (T)list.array[index];

        internal ReadOnlyList(ArrayList list)
        {
            this.list = list;
        }

        public ReadOnlyList(object[] array)
        {
            list = new(array);
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

            public readonly T Current => (T)list.array[index];
            readonly object IEnumerator.Current => list.array[index];

            internal Enumerator(ArrayList list)
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

            public readonly void Dispose()
            {
            }
        }
    }
}