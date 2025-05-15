using System;
using UnityLibrary.Systems;

namespace UnityLibrary
{
    /// <summary>
    /// Collection of built-in systems.
    /// </summary>
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
