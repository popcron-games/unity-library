using System;

namespace Library.Events
{
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