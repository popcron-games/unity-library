#nullable enable
using System;
using UnityLibrary.Systems;

namespace UnityLibrary
{
    public class UnityLibrarySystems : IDisposable
    {
        private readonly VirtualMachine vm;

        public UnityLibrarySystems(VirtualMachine vm)
        {
            this.vm = vm;
            vm.AddSystem(new UnityObjects());
            vm.AddSystem(new UnityEventDispatcher(vm));
        }

        public void Dispose()
        {
            vm.RemoveSystem<UnityEventDispatcher>().Dispose();
            vm.RemoveSystem<UnityObjects>();
        }
    }
}
