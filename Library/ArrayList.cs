#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityLibrary
{
    internal sealed class ArrayList : IReadOnlyList<object>
    {
        public object[] array;
        public int count;
        public int capacity;

        public int Count => count;
        public object this[int index] => array[index];

        public ArrayList(int capacity = 4)
        {
            array = new object[capacity];
            this.capacity = capacity;
        }

        public ArrayList(object[] array)
        {
            this.array = array;
            count = array.Length;
            capacity = array.Length;
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

        [Conditional("DEBUG")]
        public void ThrowIfNotContained(object value)
        {
            if (Array.IndexOf(array, value, 0, count) < 0)
            {
                throw new InvalidOperationException("The specified value is not contained in the ArrayList.");
            }
        }

        public struct Enumerator : IEnumerator<object>
        {
            private readonly ArrayList list;
            private int index;

            public readonly object Current => list.array[index];

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
    }
}