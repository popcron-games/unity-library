using System;

namespace Library.Events
{
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