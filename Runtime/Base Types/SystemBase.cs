#nullable enable
using System;

namespace UnityLibrary
{
    [Serializable]
    public abstract class SystemBase : IDisposable
    {
        public readonly VirtualMachine vm;

        public SystemBase(VirtualMachine vm)
        {
            this.vm = vm;
        }

        public abstract void Dispose();
    }
}