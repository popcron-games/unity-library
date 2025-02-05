#nullable enable
using System;

namespace UnityLibrary.Events
{
    /// <summary>
    /// Occurs after a system is added to a <see cref="VirtualMachine"/>.
    /// </summary>
    public readonly struct SystemAdded
    {
        public readonly VirtualMachine vm;
        public readonly Type type;

        public SystemAdded(VirtualMachine vm, Type type)
        {
            this.vm = vm;
            this.type = type;
        }
    }
}