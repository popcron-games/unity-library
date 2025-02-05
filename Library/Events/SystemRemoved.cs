#nullable enable
using System;

namespace UnityLibrary.Events
{
    /// <summary>
    /// Occurs before a system is removed from a <see cref="VirtualMachine"/>.
    /// </summary>
    public readonly struct SystemRemoved
    {
        public readonly VirtualMachine vm;
        public readonly Type type;

        public SystemRemoved(VirtualMachine vm, Type type)
        {
            this.vm = vm;
            this.type = type;
        }
    }
}