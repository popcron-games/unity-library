#nullable enable
using System;
using System.Collections.Generic;

namespace Library
{
    public class TestSystem : IDisposable, IListener<TestEvent>
    {
        public string value;
        public bool disposed;
        public readonly List<TestEvent> events = new();

        public TestSystem(string value)
        {
            this.value = value;
        }

        public void Dispose()
        {
            disposed = true;
        }

        void IListener<TestEvent>.Receive(VirtualMachine vm, TestEvent e)
        {
            events.Add(e);
        }
    }
}