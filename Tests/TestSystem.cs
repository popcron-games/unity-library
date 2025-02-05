#nullable enable
using System;
using System.Collections.Generic;

namespace UnityLibrary
{
    public class TestSystem : IDisposable, IListener<DummyTestEvent>
    {
        public string value;
        public bool disposed;
        public readonly List<DummyTestEvent> events = new();

        public TestSystem(string value)
        {
            this.value = value;
        }

        public void Dispose()
        {
            disposed = true;
        }

        void IListener<DummyTestEvent>.Receive(VirtualMachine vm, ref DummyTestEvent e)
        {
            events.Add(e);
        }
    }
}